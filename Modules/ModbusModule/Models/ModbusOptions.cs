using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModbusModule.Models
{
    public class ModbusOptions : ModbusConfig
    {
        /// <summary>
        /// 连接超时时间（毫秒），默认为 5000ms (5秒)。
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// 读取操作的超时时间（毫秒），默认为 1000ms (1秒)。
        /// </summary>
        public int ReadTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// 写入操作的超时时间（毫秒），默认为 1000ms (1秒)。
        /// </summary>
        public int WriteTimeoutMs { get; set; } = 1000;
    }

}
