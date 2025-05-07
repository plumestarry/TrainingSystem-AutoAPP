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
        private string ipAddress;
        private ushort port;
        private byte slaveID;

        public string IPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; OnPropertyChanged(); }
        }

        public ushort Port
        {
            get { return port; }
            set { port = value; OnPropertyChanged(); }
        }

        public byte SlaveID
        {
            get { return slaveID; }
            set { slaveID = value; OnPropertyChanged(); }
        }
    }
}
