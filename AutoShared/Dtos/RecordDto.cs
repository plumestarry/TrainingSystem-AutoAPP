using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoShared.Dtos
{
    /// <summary>
    /// 待办事项数据实体
    /// </summary>
    public class RecordDto : BaseDto
    {
        private string title;
        private string content;

        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(); }
        }

        public string Content
        {
            get { return content; }
            set { content = value; OnPropertyChanged(); }
        }

    }
}
