using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public class DelayTask : BaseTask
    {
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            for (int i = 0; i <= 20; i++)
            {
                if (ct.IsCancellationRequested)
                    return false;
                await Task.Delay(200, ct);
                Progress = (float)i / 20;
                Info = $"DelayTask {Progress * 100}%";
            }
            return true;
        }
    }
}
