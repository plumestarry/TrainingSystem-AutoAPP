﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoShared.Dtos
{
    public class UserDto : BaseDto
    {
        private string account;

        public string Account
        {
            get { return account; }
            set { account = value; OnPropertyChanged(); }
        }

        private string passWord;

        public string PassWord
        {
            get { return passWord; }
            set { passWord = value; OnPropertyChanged(); }
        } 
    }
}
