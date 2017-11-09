using System;
using Microsoft.Extensions.Configuration;

namespace Portal.Settings
{
    public class DockerBuildMethodSetting
    {
        public Uri ApiUri { get; }
        public string ImageName { get; }
        public string SourceDir { get; }
        public string BuildDir { get; }
        public int OutputBufferSize { get; }

        public DockerBuildMethodSetting(IConfiguration configuration)
        {
            ApiUri = new Uri(configuration.GetValue<string>("ApiUri"));
            ImageName = configuration.GetValue<string>("Image");
            SourceDir = configuration.GetValue<string>("SourceDir");
            BuildDir = configuration.GetValue<string>("BuildDir");
            OutputBufferSize = configuration.GetValue<int>("OutputBufferSize");
        }
    }
}