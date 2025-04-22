using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Models
{
    /// <summary>
    /// 任务栏
    /// </summary>
    public class TaskBar : BindableBase
    {
        /// <summary>
        /// 任务栏图标
        /// </summary>
        private string? icon;
        public string? Icon
        {
            get { return icon; }
            set { icon = value; }
        }

        /// <summary>
        /// 任务栏标题
        /// </summary>
        private string? title;
        public string? Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// 任务栏内容
        /// </summary>
        private string? content;
        public string? Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 任务栏颜色
        /// </summary>
        private string? color;
        public string? Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// 任务栏链接
        /// </summary>
        private string? target;
        public string? Target
        {
            get { return target; }
            set { target = value; }
        }
    }
}
