namespace SiamCross.Models.Tools
{
    struct PropertyName
    {
        public string RuName { get; }

        public string Name { get; }

        public PropertyName(string ruName, string name)
        {
            RuName = ruName;
            Name = name;
        }
    }
}
