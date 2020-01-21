namespace ModMyFactory.Win32
{
    public enum WindowLongIndex : int
    {
        WindowProcedure = -4,
        InstanceHandle = -6,
        Id = -12,
        Style = -16,
        ExtendedStyle = -20,
        UserData = -21,

        // Only for message boxes
        MessageResult = 0x0,
        DialogProcedure = 0x4,
        User = 0x8
    }
}
