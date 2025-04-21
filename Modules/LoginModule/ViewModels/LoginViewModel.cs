using AutoAPP.Core.Service;
using AutoShared.Dtos;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoAPP.Core.Service.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoAPP.Core.Extensions;
using System.Reflection;
using System.CodeDom;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace LoginModule.ViewModels
{
    public partial class LoginViewModel : ObservableObject, IDialogAware
    {

        private readonly ILoginService loginService;
        private readonly IEventAggregator aggregator;

        public LoginViewModel(ILoginService loginService, IEventAggregator aggregator)
        {
            Title = "分拣装置实训登录平台";
            UserDto = new RegisterUserDto();
            this.loginService = loginService;
            this.aggregator = aggregator;
        }

        //DialogCloseListener IDialogAware.RequestClose { get; }

        //public event Action<IDialogResult> RequestClose;

        public DialogCloseListener RequestClose { get; } // 实现为属性

        public event Action<IDialogResult> RequestCloseEvent; // 保留现有的事件，用于在 ViewModel 内部触发关闭请求


        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }

        #region ************************************************** Login **************************************************

        [ObservableProperty]
        string title;

        [ObservableProperty]
        string account;

        [ObservableProperty]
        string passWord;

        [ObservableProperty]
        int selectIndex;

        [ObservableProperty]
        RegisterUserDto userDto;

        [RelayCommand]
        void Execute(string obj)
        {
            switch (obj)
            {
                case "Login": Login(); break;
                case "LoginOut": LoginOut(); break;
                case "Register": Register(); break;
                case "RegisterPage": SelectIndex = 1; break;
                case "Return": SelectIndex = 0; break;
                case "Guest": GuestLogin(); break;
            }
        }

        async void Login()
        {
            if (string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(PassWord))
            {
                return;
            }

            if (Account == "Guest" && PassWord == "Guest" && AppSession.UserName == "Guest")
            {
                RequestClose.Invoke(ButtonResult.OK);
                return;
            }

            var loginResult = await loginService.Login(new UserDto()
            {
                Account = Account,
                PassWord = PassWord,
                UserName = "",
            });

            if (loginResult != null && loginResult.Status)
            {
                AppSession.UserName = loginResult.Result.UserName;
                //RequestCloseEvent?.Invoke(new DialogResult(ButtonResult.OK));
                RequestClose.Invoke(ButtonResult.OK);
            }
            else
            {
                // 登录失败提示...
                aggregator.SendMessage(loginResult?.Message ?? string.Empty, "Login");
            }
        }

        private async void Register()
        {
            if (string.IsNullOrWhiteSpace(UserDto.Account) ||
                string.IsNullOrWhiteSpace(UserDto.UserName) ||
                string.IsNullOrWhiteSpace(UserDto.PassWord) ||
                string.IsNullOrWhiteSpace(UserDto.NewPassWord))
            {
                aggregator.SendMessage("请输入完整的注册信息！", "Login");
                return;
            }

            if (UserDto.PassWord != UserDto.NewPassWord)
            {
                aggregator.SendMessage("密码不一致,请重新输入！", "Login");
                return;
            }

            // 检查用户名是否为中文，检查账号和密码是否为字母或数字
            if (!Regex.IsMatch(UserDto.Account, @"^[a-zA-Z0-9]+$") ||
                !Regex.IsMatch(UserDto.PassWord, @"^[a-zA-Z0-9]+$") ||
                !Regex.IsMatch(UserDto.UserName, @"^[\u4e00-\u9fa5]+$"))
            {
                aggregator.SendMessage("用户名必须为中文, 账号和密码必须为字母或数字！", "Login");
                return;
            }

            var registerResult = await loginService.Register(new UserDto()
            {
                Account = UserDto.Account,
                UserName = UserDto.UserName,
                PassWord = UserDto.PassWord
            });

            if (registerResult != null && registerResult.Status)
            {
                aggregator.SendMessage("注册成功", "Login");
                // 注册成功,返回登录页页面
                SelectIndex = 0;
            }
            else
                aggregator.SendMessage(registerResult?.Message ?? string.Empty, "Login");
        }

        void LoginOut()
        {
            RequestClose.Invoke(ButtonResult.No);
        }

        void GuestLogin()
        {
            try
            {
                AppSession.UserName = "Guest";
                Account = "Guest";
                PassWord = "Guest";
            }
            catch (Exception)
            {
                aggregator.SendMessage("登录失败", "Login");
            }
        }

        #endregion ************************************************** Login **************************************************
    }
}
