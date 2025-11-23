namespace MHServerEmu.Gui.Avalonia.Models
{
    public class ServerConfigSetting
    {
        public string Section { get; init; }
        public string Name { get; init; }
        public string Value { get; set; }
        public string ValueOverride { get; set; }
        public bool IsDirty { get; set; }
    }
}
