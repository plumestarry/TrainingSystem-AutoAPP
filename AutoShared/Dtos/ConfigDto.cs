using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoShared.Dtos
{
    /// <summary>
    /// 备忘录数据实体
    /// </summary>
    public class ConfigDto : BaseDto
    {
        private string title;
        private string ipAddress;
        private string ioDefinitions;
        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }

        public string IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; OnPropertyChanged(); }
        }

        public string IODefinitions
        {
            get { return ioDefinitions; }
            set { ioDefinitions = value; OnPropertyChanged(); }
        }
    }
}
