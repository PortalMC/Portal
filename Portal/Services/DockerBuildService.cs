using System;
using System.Collections.Generic;
using System.Linq;
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
            StartBuildInternal(uuid);
        }

        private async void StartBuildInternal(string uuid)
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
            await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());

            /*
            var result = await _client.Containers.ExecCreateContainerAsync(container.ID, new ContainerExecCreateParameters
            {
                AttachStdout = true,
                AttachStderr = true,
                Tty = true,

                Cmd = new List<string>
                {
                    "/bin/bash -c /portal/build.sh"
                }
            });
            var stream = await _client.Containers.StartAndAttachContainerExecAsync(container.ID, false);
            */
            var stream = await _client.Containers.AttachContainerAsync(container.ID, true, new ContainerAttachParameters
            {
                Stderr = true,
                Stdout = true,
                Stream = true
            });
            try
            {
                byte[] buf = new byte[1024];
                while (true)
                {
                    CancellationTokenSource cancellation = new CancellationTokenSource();
                    var res = await stream.ReadOutputAsync(buf, 0, 1024, cancellation.Token);
                    _logger.LogDebug($"{res.Target} : {res.Count} : {System.Text.Encoding.Default.GetString(buf.Take(res.Count).ToArray())}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}