using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoShared.Parameters
{
    public class QueryParameter
    {
        /// <summary>
        /// 指定获取数据的起始页
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 指定每页要获取的记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 用于根据关键词进行模糊搜索
        /// </summary>
        public string? Search { get; set; }
    }
}
