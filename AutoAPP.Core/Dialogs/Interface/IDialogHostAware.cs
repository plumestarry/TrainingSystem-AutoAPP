using Prism.Commands;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAPP.Core.Dialogs.Interface
{
    public interface IDialogHostAware 
    {
        /// <summary>
        /// DialoHost名称
        /// </summary>
        string DialogHostName { get; set; }

        /// <summary>
        /// 打开过程中执行
        /// </summary>
        /// <param name="parameters"></param>
        void OnDialogOpend(IDialogParameters parameters);

        /// <summary>
        /// 确定
        /// </summary>
        void Save();

        /// <summary>
        /// 取消
        /// </summary>
        void Cancel();
    }
}
