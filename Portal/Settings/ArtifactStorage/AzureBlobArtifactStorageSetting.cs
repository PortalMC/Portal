using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Portal.Extensions;

namespace Portal.Settings.ArtifactStorage
{
    public class AzureBlobArtifactStorageSetting : ArtifactStorageSetting
    {
        private readonly CloudBlobClient _blobClient;
        private readonly ArtifactProvideMethod _artifactProvideMethod;
        private readonly string _accessPolicy;

        public AzureBlobArtifactStorageSetting(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetValue<string>("ConnectionString"));
            _blobClient = storageAccount.CreateCloudBlobClient();
            _artifactProvideMethod = Enum.Parse<ArtifactProvideMethod>(configuration.GetValue<string>("ProvideMethod"));
            _accessPolicy = configuration.GetValue<string>("AccessPolicy");
        }

        public override DirectoryInfo GetRootDirectory()
        {
            var temp = new DirectoryInfo(Path.GetTempPath()).ResolveDir("portal_artifact");
            temp.Create();
            return temp;
        }

        public override async Task AfterBuildAsync(string projectId)
        {
            var container = _blobClient.GetContainerReference(projectId);
            await container.CreateIfNotExistsAsync();

            var blockBlob = container.GetBlockBlobReference("1.jar");

            var projectArtifactDir = GetRootDirectory().ResolveDir(projectId);
            var file = projectArtifactDir.ResolveDir("libs").Resolve("modid-1.0.jar");

            await blockBlob.UploadFromFileAsync(file.FullName);
            projectArtifactDir.Delete(true);
        }

        public override ArtifactProvideMethod GetArtifactProvideMethod()
        {
            return _artifactProvideMethod;
        }

        public override async Task<Stream> GetArtifactStreamAsync(string projectId, string buildId)
        {
            var container = _blobClient.GetContainerReference(projectId);
            await container.CreateIfNotExistsAsync();
            var blockBlob = container.GetBlockBlobReference("1.jar");
            var stream = new MemoryStream();
            await blockBlob.DownloadToStreamAsync(stream);
            return await Task.FromResult<Stream>(stream);
        }

        public override async Task<string> GetArtifactRedirectUriAsync(string projectId, string buildId)
        {
            var container = _blobClient.GetContainerReference(projectId);
            await container.CreateIfNotExistsAsync();
            var blockBlob = container.GetBlockBlobReference("1.jar");
            return await Task.FromResult(GetBlobSasUri(blockBlob, _accessPolicy));
        }

        private static string GetBlobSasUri(CloudBlob blob, string policyName = null)
        {
            string sasBlobToken;
            if (string.IsNullOrEmpty(policyName))
            {
                var adHocSas = new SharedAccessBlobPolicy
                {
                    SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-15),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                sasBlobToken = blob.GetSharedAccessSignature(adHocSas);
                Console.WriteLine("SAS for blob (ad hoc): {0}", sasBlobToken);
            }
            else
            {
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
                Console.WriteLine("SAS for blob (stored access policy): {0}", sasBlobToken);
            }
            return blob.Uri + sasBlobToken;
        }
    }
}