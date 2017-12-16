namespace Portal.Models.AdminViewModels
{
    public class VersionsPostViewModel
    {
        public NewVersionViewModel NewVersionViewModel { get; set; }
        public VersionDetailDockerImageVersionViewModel DockerImageVersionViewModel { get; set; }
        public VersionDetailForgeEditViewModel ForgeEditViewModel { get; set; }
    }
}