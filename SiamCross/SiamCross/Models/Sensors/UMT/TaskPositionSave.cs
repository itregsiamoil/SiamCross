namespace SiamCross.Models.Sensors.Umt
{
    public class TaskPositionSave : Dua.TaskPositionSave
    {
        public TaskPositionSave(SensorPosition pos)
            : base(pos, 0x8001)
        {
        }
    }
}
