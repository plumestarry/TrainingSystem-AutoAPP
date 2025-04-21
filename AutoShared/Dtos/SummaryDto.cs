using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoShared.Dtos
{
    /// <summary>
    /// 汇总
    /// </summary>
    public class SummaryDto : BaseDto
    {
        private int sum;
        private int completedCount;
        private string completedRatio;


        /// <summary>
        /// 实训记录总数
        /// </summary>
        public int Sum
        {
            get { return sum; }
            set { sum = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 实训通过次数
        /// </summary>
        public int CompletedCount
        {
            get { return completedCount; }
            set { completedCount = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 实训通过比例
        /// </summary>
        public string CompletedRatio
        {
            get { return completedRatio; }
            set { completedRatio = value; OnPropertyChanged(); }
        }

        private ObservableCollection<RecordDto> recordList;

        /// <summary>
        /// 实训记录列表
        /// </summary>
        public ObservableCollection<RecordDto> RecordList
        {
            get { return recordList; }
            set { recordList = value; OnPropertyChanged(); }
        }

    }
}
