using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Pomelo.AspNetCore.TimedJob;
using Andoromeda.Framework.GitHub;

namespace Andoromeda.Kyubey.Dex.Jobs
{
    public class GitHubJob : Job
    {
        [Invoke(Begin = "2018-11-01 0:00", Interval = 1000 * 60 * 5, SkipWhileExecuting = true)]
        public void SyncNewsRepository(IConfiguration config)
        {
            GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                "kyubey-network", "dex-news", "master",
                Path.Combine(config["RepositoryStore"], "dex-news")).Wait();
        }

        [Invoke(Begin = "2018-11-01 0:01", Interval = 1000 * 60 * 5, SkipWhileExecuting = true)]
        public void SyncSlidesRepository(IConfiguration config)
        {
            GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                "kyubey-network", "dex-news", "master",
                Path.Combine(config["RepositoryStore"], "dex-slides")).Wait();
        }

        [Invoke(Begin = "2018-11-01 0:01", Interval = 1000 * 60 * 5, SkipWhileExecuting = true)]
        public void SyncTokensRepository(IConfiguration config)
        {
            GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                "kyubey-network", "token-list", "master",
                Path.Combine(config["RepositoryStore"], "token-list")).Wait();
        }
    }
}
