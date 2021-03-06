﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Portal.Settings.ArtifactStorage
{
    public abstract class ArtifactStorageSetting
    {
        public abstract Task AfterBuildAsync(string projectId, FileInfo artifactFile, int buildId);
        public abstract ArtifactProvideMethod GetArtifactProvideMethod();

        public virtual Task<Stream> GetArtifactStreamAsync(string projectId, int buildId)
        {
            throw new NotSupportedException();
        }

        public virtual Task<string> GetArtifactRedirectUriAsync(string projectId, int buildId)
        {
            throw new NotSupportedException();
        }
    }

    public enum ArtifactProvideMethod
    {
        Stream,
        Redirect
    }
}