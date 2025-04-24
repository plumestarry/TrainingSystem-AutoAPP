using ModbusModule.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModbusModule.Views
{
    /// <summary>
    /// ModbusView.xaml 的交互逻辑
    /// </summary>
    public partial class ModbusView : UserControl
    {
        public ModbusView()
        {
            InitializeComponent();
        }

        // 指定的事件处理方法
        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // sender 就是触发事件的 TextBox
            TextBox? textBox = sender as TextBox;
            // 调用 ScrollToEnd() 方法将滚动条移动到内容的最后
            textBox?.ScrollToEnd();
        }
    }
}
