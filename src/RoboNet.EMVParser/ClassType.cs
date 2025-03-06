namespace RoboNET.EMVParser;

public enum ClassType : byte
{
    UniversalClass = 0,
    ApplicationClass = 1,
    ContextSpecificClass = 2,
    PrivateClass = 3
}