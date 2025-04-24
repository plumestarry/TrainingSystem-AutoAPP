using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using ModbusModule.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Methods
{
    public static class IODataMethod
    {
        /// <summary>
        /// 初始化数据库：如果表不存在则创建表
        /// </summary>
        public static void InitializeDatabase(string ConnectionString, int EntitiesCount)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                var createTableCommand = connection.CreateCommand();

                for (int i = 1; i <= EntitiesCount * 2; i++)
                {
                    string tableName = (i % 2 == 1) ? $"InputModbusItems{(i + 1) / 2}" : $"OutputModbusItems{(i + 1) / 2}";

                    // 创建 Input 表和 Output 表
                    createTableCommand.CommandText = @$"
                    CREATE TABLE IF NOT EXISTS {tableName} (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        [Index] INTEGER,
                        ModbusType TEXT,
                        Name TEXT,
                        Port INTEGER
                    );";
                    createTableCommand.ExecuteNonQuery();
                }

                createTableCommand.CommandText = @$"
                    CREATE TABLE IF NOT EXISTS ModbusData (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        INFO TEXT,
                    );";
                createTableCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                // 处理初始化错误，例如日志记录或向用户显示错误
                System.Diagnostics.Debug.WriteLine($"数据库初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查表是否为空 (可选助手方法)
        /// </summary>
        public static bool IsTableEmpty(string tableName, SqliteCommand command)
        {
            command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
            var result = command.ExecuteScalar();
            if (result is null)
            {
                return true;
            }
            var count = (long)result;
            return count == 0;
        }


        /// <summary>
        /// 从 SQLite 数据库加载数据
        /// </summary>
        public static void LoadDataFromSqlite(string ConnectionString, int Index, ObservableCollection<ModbusItems> InputItems, ObservableCollection<ModbusItems> OutputItems)
        {
            try
            {
                // 清空现有的集合数据
                InputItems.Clear();
                OutputItems.Clear();

                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // 加载 InputItems 和 OutputItems
                var selectCommand = connection.CreateCommand();

                for (int i = 0; i < 2; i++)
                {
                    string tableName = (i % 2 == 0) ? $"InputModbusItems{Index}" : $"OutputModbusItems{Index}";

                    // 检查表是否存在且不为空
                    if (IsTableEmpty(tableName, selectCommand))
                    {
                        continue;
                    }

                    // ORDER BY [Index] 确保加载的数据按序号排序
                    selectCommand.CommandText = $"SELECT [Index], ModbusType, Name, Port FROM {tableName} ORDER BY [Index]";

                    var items = (i % 2 == 0) ? InputItems : OutputItems;

                    using var reader = selectCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        items.Add(new ModbusItems
                        {
                            // 从 reader 读取数据，注意列的索引
                            Index = reader.GetInt32(0),
                            ModbusType = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Port = reader.GetInt32(3)
                        });
                    }
                }

                // 加载数据后，重新检查并校正序号 (确保序号连续，即使数据库中的 Index 可能不连续)
                ReIndexItems(InputItems);
                ReIndexItems(OutputItems);

                // 如果数据库为空，集合会保持空白，符合要求
            }
            catch (SqliteException sqlEx)
            {
                // 处理特定的 SQLite 错误，例如表不存在（虽然 InitializeDatabase 应该创建它）
                // 如果表不存在，SELECT 查询通常不会抛出异常，而是返回空结果集，这正是我们想要的情况。
                // 所以这里主要捕获其他可能的 SQLite 错误。
                System.Diagnostics.Debug.WriteLine($"加载数据时发生 SQLite 错误: {sqlEx.Message}");
                // 如果发生错误，集合可能部分加载或保持空白，用户可以看到空白界面。
            }
            catch (Exception ex)
            {
                // 处理其他可能的错误
                System.Diagnostics.Debug.WriteLine($"加载数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存数据到 SQLite 数据库
        /// </summary>
        public static void SaveData(string ConnectionString, int Index, ObservableCollection<ModbusItems> InputItems, ObservableCollection<ModbusItems> OutputItems)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // 使用事务确保保存操作的原子性（要么全部成功，要么全部失败）
                using var transaction = connection.BeginTransaction();

                var command = connection.CreateCommand();

                // 添加参数，提高性能并防止潜在问题
                command.Parameters.Add("@Index", SqliteType.Integer);
                command.Parameters.Add("@ModbusType", SqliteType.Text);
                command.Parameters.Add("@Name", SqliteType.Text);
                command.Parameters.Add("@Port", SqliteType.Integer);

                try
                {

                    for (int i = 0; i < 2; i++)
                    {
                        string tableType = (i % 2 == 0) ? "InputModbusItems" : "OutputModbusItems";

                        // 1. 清空现有数据 (简单粗暴的方式，对于小数据集可行)
                        command.CommandText = $"DELETE FROM {tableType}{Index}";
                        command.ExecuteNonQuery();

                        // 2. 插入当前集合中的数据
                        command.CommandText = $"INSERT INTO {tableType}{Index} ([Index], ModbusType, Name, Port) VALUES (@Index, @ModbusType, @Name, @Port)";

                        foreach(var item in (i % 2 == 0) ? InputItems : OutputItems)
                        {
                            // 为参数赋值
                            command.Parameters["@Index"].Value = item.Index;
                            // 处理可能为 null 的字符串，存储为 DBNull
                            command.Parameters["@ModbusType"].Value = item.ModbusType ?? (object)DBNull.Value;
                            command.Parameters["@Name"].Value = item.Name ?? (object)DBNull.Value;
                            command.Parameters["@Port"].Value = item.Port;
                            command.ExecuteNonQuery();
                        }

                    }

                    // 提交事务
                    transaction.Commit();

                    // 可以选择在此处通知用户保存成功
                    System.Diagnostics.Debug.WriteLine("数据保存成功！");
                }
                catch (Exception ex)
                {
                    // 如果事务中发生任何错误，回滚事务
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"保存数据时发生错误，已回滚事务: {ex.Message}");
                    // 实际应用中应该向用户显示错误
                }
            }
            catch (Exception ex)
            {
                // 处理连接或其他外部错误
                System.Diagnostics.Debug.WriteLine($"数据库连接或保存操作失败: {ex.Message}");
                // 实际应用中应该向用户显示错误
            }
        }

        public static void SaveModbusData(string ConnectionString, List<string> linesToSave)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // 使用事务确保保存操作的原子性（要么全部成功，要么全部失败）
                using var transaction = connection.BeginTransaction();

                var command = connection.CreateCommand();

                // 添加参数，提高性能并防止潜在问题
                command.Parameters.Add("@INFO", SqliteType.Text);

                try
                {
                        // 插入当前集合中的数据
                        command.CommandText = $"INSERT INTO ModbusData (INFO) VALUES (@INFO)";

                        foreach (var item in linesToSave)
                        {
                            // 为参数赋值
                            command.Parameters["@INFO"].Value = item ?? (object)DBNull.Value;
                            command.ExecuteNonQuery();
                        }

                    // 提交事务
                    transaction.Commit();

                    // 可以选择在此处通知用户保存成功
                    System.Diagnostics.Debug.WriteLine("数据保存成功！");
                }
                catch (Exception ex)
                {
                    // 如果事务中发生任何错误，回滚事务
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"保存数据时发生错误，已回滚事务: {ex.Message}");
                    // 实际应用中应该向用户显示错误
                }
            }
            catch (Exception ex)
            {
                // 处理连接或其他外部错误
                System.Diagnostics.Debug.WriteLine($"数据库连接或保存操作失败: {ex.Message}");
                // 实际应用中应该向用户显示错误
            }
        }

        /// <summary>
        /// 助手方法：重新索引 ObservableCollection 中的项 (不变)
        /// </summary>
        public static void ReIndexItems(ObservableCollection<ModbusItems> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Index = i + 1;
            }
        }
    }
}
