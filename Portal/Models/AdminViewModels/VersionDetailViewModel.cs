using System.Collections.Generic;

namespace Portal.Models.AdminViewModels
{
    public class VersionDetailViewModel
    {
        public MinecraftVersion MinecraftVersion { get; set; }
        public IEnumerable<ForgeVersion> ForgeVersions { get; set; }
    }
}