namespace CWBDrone.Config
{
    public class ConfigColor
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public Discord.Color ToDiscordColor()
            => new Discord.Color(Red, Green, Blue);
    }
}
