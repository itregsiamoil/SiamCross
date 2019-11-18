namespace SiamCross.Models
{
    public class SensorData
    {
        public string Name { get; private set; }

        public string Type { get; private set; }

        public string Status { get; set; }

        public int Id { get; private set; }

        public SensorData(int id, string name, string type, string status)
        {
            Id = id;
            Name = name;
            Type = type;
            Status = status;
        }
    }
}
