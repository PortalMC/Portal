using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Portal.Extensions;
using Portal.Models;
using Portal.Settings;

namespace Portal.Services
{
    public class DockerBuildService : IBuildService
    {
        private readonly StorageSetting _storageSetting;
        private readonly IMinecraftVersionProvider _minecraftVersionProvider;
        private readonly ILogger<DockerBuildService> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly DockerBuildMethodSetting _dockerSetting;
        private readonly DockerClient _client;

        public DockerBuildService(
            StorageSetting storageSetting,
            BuildSetting buildSetting,
            IMinecraftVersionProvider minecraftVersionProvider,
            ILogger<DockerBuildService> logger,
            WebSocketConnectionManager connectionManager)
        {
            _storageSetting = storageSetting;
            _minecraftVersionProvider = minecraftVersionProvider;
            _logger = logger;
            _connectionManager = connectionManager;
            _dockerSetting = buildSetting.GetBuildMethodSetting<DockerBuildMethodSetting>();
            _client = new DockerClientConfiguration(_dockerSetting.ApiUri).CreateClient();
        }

        public async Task CheckImageAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start checking Docker...");
            _logger.LogInformation("Checking builder image is available...");
            var images = await _client.Images.ListImagesAsync(new ImagesListParameters(), cancellationToken);
            foreach (var minecraftVersion in _minecraftVersionProvider.GetMinecraftVersions())
            {
                var repo = _dockerSetting.ImageName;
                var tag = $"mc-{minecraftVersion.Version}-{minecraftVersion.DockerImageVersion}";
                if (images.Any(i => i.RepoTags.Contains($"{repo}:{tag}"))) continue;
                _logger.LogInformation($"Image '{repo}:{tag}' has not been pulled.");
                _logger.LogInformation("Start pulling image.");
                try
                {
                    await _client.Images.CreateImageAsync(new ImagesCreateParameters
                    {
                        Repo = repo,
                        Tag = tag
                    }, null, null, cancellationToken);
                    _logger.LogInformation($"Pull image '{repo}:{tag}' successfully!");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to pull image '{repo}:{tag}'. Reason: {ex.Message}");
                }
            }
        }

        public void StartBuild(Project project, string userId)
        {
            _logger.LogInformation("Start build : " + project.Id);
            StartBuildInternal(project, userId).FireAndForget();
        }

        private async Task StartBuildInternal(Project project, string userId)
        {
            var destination = _storageSetting.GetArtifactStorageSetting().GetRootDirectory().ResolveDir(project.Id);
            if (!destination.Exists)
            {
                destination.Create();
            }
            _logger.LogInformation("Creating container...");
            var repo = _dockerSetting.ImageName;
            var minecraftVersion = _minecraftVersionProvider.GetMinecraftVersions().Single(a => a.Version == project.MinecraftVersion);
            var tag = $"mc-{project.MinecraftVersion}-{minecraftVersion.DockerImageVersion}";
            var container = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                //Name = "portal-build-daemon",
                Image = $"{repo}:{tag}",
                HostConfig = new HostConfig
                {
                    Binds = new List<string>
                    {
                        $"{_storageSetting.GetProjectStorageSetting().GetRootDirectory().ResolveDir(project.Id).FullName}:{_dockerSetting.SourceDir}:ro",
                        $"{destination.FullName}:{_dockerSetting.BuildDir}:rw"
                    }
                },
                AttachStdout = true,
                AttachStderr = true,
                Tty = true
            });
            var buffer = new byte[_dockerSetting.OutputBufferSize];
            using (var stream = await _client.Containers.AttachContainerAsync(container.ID, true, new ContainerAttachParameters
            {
                Stdout = true,
                Stderr = true,
                Stream = true
            }))
            {
                await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
                while (true)
                {
                    var result = await stream.ReadOutputAsync(buffer, 0, buffer.Length, default(CancellationToken));
                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.Write(text);
                    _connectionManager.SendMessage(project.Id, userId, text);
                    if (result.EOF)
                    {
                        break;
                    }
                }
            }
            _logger.LogInformation("Build finished. Removing container...");
            await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters());
            _logger.LogInformation("Container removed.");
            await _storageSetting.GetArtifactStorageSetting().AfterBuildAsync(project.Id);
        }
    }
}