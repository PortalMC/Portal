using System.IO;
using Microsoft.Extensions.Configuration;

namespace Portal.Services
{
    public class ProjectSetting : IProjectSetting
    {
        private readonly DirectoryInfo _root;

        public ProjectSetting(IConfiguration configuration)
        {
            _root = new DirectoryInfo(configuration.GetSection("Root").Value);
        }


        public DirectoryInfo GetProjectsRoot()
        {
            return _root;
        }
    }
}