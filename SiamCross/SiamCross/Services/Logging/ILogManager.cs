namespace SiamCross.Services.Logging
{
    public interface ILogManager
    {
        NLog.Logger GetLog([System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "");
    }
}
