using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskPositionLoad : BaseSensorTask
    {
        readonly SensorPosition _Model;
        public TaskPositionLoad(SensorPosition pos)
            : base(pos.Sensor, Resource.LoadingLocation)
        {
            _Model = pos;
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model)
                return false;
            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    //bool ret = await UpdateAsync(linkTsc.Token);
                    bool ret = false;
                    if (ret)
                        _Model.UpdateSaved();
                    return ret;
                }
            }
        }
    }
}
