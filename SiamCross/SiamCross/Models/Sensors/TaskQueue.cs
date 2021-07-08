using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{
    public class TaskQueue : BaseTask
    {
        IEnumerable<ITask> _Queue;
        public TaskQueue(IEnumerable<ITask> queue)
            : base()
        {
            _Queue = queue;
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            bool ret = true;
            foreach (var t in _Queue)
            {
                if (JobStatus.Сomplete != await t.ExecAsync(Manager, ct))
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }
    }
}
