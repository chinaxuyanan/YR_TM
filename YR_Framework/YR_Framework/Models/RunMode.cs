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
        FAIL,
        PASS,
        Paused,
        Stop,
        EmerStop,
        Error
    }

    public enum TestStep
    {
        None,
        Step_0001,
        Step_0002,
        Step_0003,
        Step_0004,
        Step_0005,
        Step_0006,
        Step_0007,
        Step_0008,
        Step_0009,
        Step_0010
    }

    public static class AppState
    {
        public static UserLevel CurrentUser { get; set; } = UserLevel.Operator;
        public static RunMode CurrentRunMode { get; set; } = RunMode.Product;
        public static RunState CurrentRunState { get; set; } = RunState.Idle;
    }
}
