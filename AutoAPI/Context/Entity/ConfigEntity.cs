namespace AutoAPI.Context.Entity
{
    public class ConfigEntity : BaseEntity
    {
        public string IPAddress { get; set; }

        public string Port { get; set; }

        public string SlaveID { get; set; }
    }
}
