using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskPositionSave : BaseSensorTask
    {
        readonly SensorPosition _Model;

        public TaskPositionSave(SensorPosition pos)
            : base(pos.Sensor, Resource.SavingLocation)
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
                    bool ret = await DoSaveSingleAsync(linkTsc.Token);
                    if (ret)
                        _Model.UpdateSaved();
                    else
                        _Model.ResetSaved();
                    return ret;
                }
            }
        }
        async Task<bool> DoSaveSingleAsync(CancellationToken ct)
        {
            return false;
        }
    }
}
