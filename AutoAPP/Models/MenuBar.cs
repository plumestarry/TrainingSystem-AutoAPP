namespace AutoAPP.Models
{
    /// <summary>
    /// 系统导航菜单
    /// </summary>
    public partial class MenuBar : ObservableObject
    {
        /// <summary>
        /// 菜单图标
        /// </summary>
        [ObservableProperty]
        private string? icon;

        /// <summary>
        /// 菜单名称
        /// </summary>
        [ObservableProperty]
        private string? title;

        /// <summary>
        /// 菜单命名空间
        /// </summary>
        [ObservableProperty]
        private string? nameSpace;

    }
}
