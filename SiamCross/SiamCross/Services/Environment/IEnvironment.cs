namespace SiamCross.Services.Environment
{
    public interface IEnvironment // 
    {
        string GetDir_Downloads();
        string GetDir_Measurements();
        string GetDir_LocalApplicationData();
    }
}
