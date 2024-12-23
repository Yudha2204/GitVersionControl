namespace GitVersion.Model
{
    public class ChangeVersionParam
    {
        public string ModuleName { get; set; }
        public string Version { get; set; }
        public string Branch { get; set; } = "master";

    }
}
