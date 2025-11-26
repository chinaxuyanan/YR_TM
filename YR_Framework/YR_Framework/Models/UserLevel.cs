namespace YR_Framework.Models
{
    public enum UserLevel
    {
        Operator,
        Engineer,
        Admin
    }

    public enum ButtonType
    {
        Start,
        Pause,                  //停止案件 == 暂停测试
        Reset,
        EmergencyStop,          //急停
        Maintenance             //检修按钮
    }
}
