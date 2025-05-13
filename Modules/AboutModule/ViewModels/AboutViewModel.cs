using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AboutModule.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string developInfo = "开发信息\n" +
            "2025年 4月16日：正式启动项目开发\n" +
            "2025年 4月19日：完成项目 WebAPI 服务端的开发\n" +
            "2025年 4月21日：完成主体框架开发\n" +
            "2025年 4月23日：完成登录模块开发\n" +
            "2025年 4月26日：完成 Modbus 核心功能模块开发\n" +
            "2025年 5月05日：完成实训图表以及测试模块开发\n" +
            "2025年 5月07日：完成实训记录查询模块开发\n" +
            "2025年 5月08日：完成关于页面模块开发\n" +
            "2025年 5月09日：完成主页模块开发\n" +
            "2025年 5月10日：完成最后的修复工作，发布最初版本\n";
    }
}
