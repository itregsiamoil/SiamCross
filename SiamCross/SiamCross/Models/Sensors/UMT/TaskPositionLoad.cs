namespace SiamCross.Models.Sensors.UMT
{
    public class TaskPositionLoad : Dua.TaskPositionLoad
    {
        public TaskPositionLoad(SensorPosition pos)
            : base(pos, 0x8001)
        {
        }
    }
}
