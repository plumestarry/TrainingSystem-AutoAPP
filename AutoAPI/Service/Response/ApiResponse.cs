namespace AutoAPI.Service.Response
{
    public class ApiResponse
    {
        public ApiResponse(string message, bool status = false)
        {
            this.Message = message;
            this.Status = status;
        }

        public ApiResponse(bool status, object result)
        {
            this.Status = status;
            this.Result = result;
        }

        /// <summary>
        /// 回复信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 回复状态
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 回复数据
        /// </summary>
        public object Result { get; set; }
    }
}
