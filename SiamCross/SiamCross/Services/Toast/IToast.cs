namespace SiamCross.Services.Toast
{
    public interface IToast
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
