namespace ImagioDevice
{
    public enum CommandType
    {
        Unknown,
        Command, //command
        Info,
        Error,
        Warning,
        Analog,
        Digital,
        HandShake,
        Serial,
        Wireless,
        Echo
    }
}