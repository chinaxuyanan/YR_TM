namespace YR_Framework.Models
{
    public enum RunMode
    {
        Product,
        DryRun,
        Debug,
    }

    public enum RunState
    {
        Idle,
        Ready,
        Running,
        Resetting,
        FAIL,
        PASS,
        Paused,
        Stop,
        EmerStop,
        WaitingStart,
        Error
    }

    public static class AppState
    {
        public static UserLevel CurrentUser { get; set; } = UserLevel.Operator;
        public static RunMode CurrentRunMode { get; set; } = RunMode.Product;
        public static RunState CurrentRunState { get; set; } = RunState.Idle;
    }
}
