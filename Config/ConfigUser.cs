namespace CWBDrone.Config
{
    public class ConfigUser
    {
        public ulong ID { get; set; }
        public ulong Reputation { get; set; }

        public override int GetHashCode() => ID.GetHashCode();
    }
}
