namespace YR_Framework.Core
{
    /// <summary>
    /// 框架全局上下文（可挂载运行参数、版本信息等）
    /// </summary>
    public static class FrameworkContext
    {
        public static string FrameworkVersion => "1.0.2";
        public static string StationName { get; set; } = "未命名";

        public static string RunState { get; set; } = "Idle";
    }
}
