using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class DelayTask : BaseTask
    {
        public override async Task<bool> DoExecute()
        {
            for (int i = 0; i <= 20; i++)
            {
                if (_Cts.IsCancellationRequested)
                    return false;
                await Task.Delay(200, _Cts.Token);
                Progress = (float)i / 20;
                Info = $"DelayTask {Progress * 100}%";
            }
            return true;
        }
    }
}
