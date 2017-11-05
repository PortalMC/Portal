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

namespace Portal.Services
{
    public class DockerBuildService : IBuildService
    {
        private IProjectSetting _projectSetting;
        private readonly ILogger<DockerBuildService> _logger;
        private DockerClient _client;

        public DockerBuildService(IProjectSetting projectSetting, ILogger<DockerBuildService> logger)
        {
            _projectSetting = projectSetting;
            _logger = logger;
            _client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();
            Console.WriteLine("Start checking Docker...");
            CheckImageReady().Wait();
        }

        private async Task CheckImageReady()
        {
            try
            {
                Console.WriteLine("Searching jdk image...");
                var images = await _client.Images.ListImagesAsync(new ImagesListParameters());
                if (images.Count(i => i.RepoTags.Contains("openjdk:8-jdk-alpine")) == 0)
                {
                    Console.WriteLine("No pulled image found.");
                    Console.WriteLine("Try pulling image.");
                    await _client.Images.CreateImageAsync(new ImagesCreateParameters
                    {
                        Repo = "openjdk",
                        Tag = "8-jdk-alpine"
                    }, null, null);
                    Console.WriteLine("Pulled image successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void StartBuild(string uuid)
        {
            Console.WriteLine("Start:" + uuid);
            StartBuildInternal(uuid).FireAndForget();
        }

        private async Task StartBuildInternal(string uuid)
        {
            Console.WriteLine("Creating container...");
            var container = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                //Name = "portal-build-daemon",
                Image = "rralphh/portal",
                HostConfig = new HostConfig
                {
                    Binds = new List<string>
                    {
                        $"{_projectSetting.GetProjectsRoot().ResolveDir(uuid).FullName}:/portal/src:ro",
                        $"{_projectSetting.GetProjectsRoot().ResolveDir(uuid).FullName}:/portal/build:rw"
                    }
                },
                AttachStdout = true,
                AttachStderr = true,
                Tty = true
            });
            var buffer = new byte[1024];
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
                    if (result.EOF)
                    {
                        break;
                    }
                }
            }
            Console.WriteLine("Build finished. Removing container...");
            await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters());
            Console.WriteLine("Container removed.");
        }
    }
}