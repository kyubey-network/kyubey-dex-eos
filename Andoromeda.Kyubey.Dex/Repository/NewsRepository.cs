using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Framework.AnnotatedMarkdown;
using Andoromeda.Framework.GitHub;
using Andoromeda.Kyubey.Dex.Repository;

namespace Andoromeda.Kyubey.Dex.Repository
{
    public class NewsRepository : IRepository<News>
    {
        private string _lang;
        private string _path;

        public NewsRepository(string path, string lang) 
        {
            _path = path;
            _lang = lang;
        }

        private IEnumerable<string> EnumerateNewsFiles()
        {
            var files = Directory.EnumerateFiles(_path);
            return files.Where(x => x.EndsWith($".{_lang}.md"));
        }

        public IEnumerable<News> EnumerateAll()
        {
            foreach(var x in EnumerateNewsFiles())
            {
                var md = File.ReadAllText(x);
                var result = AnnotationParser.Parse(md);
                yield return GetSingle(Path.GetFileNameWithoutExtension(x));
            }
        }

        public News GetSingle(object id)
        {
            var path = Path.Combine(_path, $"{id}.{_lang}.md");
            var md = File.ReadAllText(path);
            var result = AnnotationParser.Parse(md);
            return new News
            {
                Id = id.ToString(),
                Title = result.Annotations.ContainsKey("Title") ? result.Annotations["Title"] : null,
                PublishedAt = result.Annotations.ContainsKey("Published at") ? Convert.ToDateTime(result.Annotations["Published at"]) : DateTime.MinValue,
                IsPinned = result.Annotations.ContainsKey("Pinned") ? Convert.ToBoolean(result.Annotations["Pinned"]) : false,
                Content = result.PlainMarkdown
            };
        }
    }

    public class NewsRepositoryFactory : IRepositoryFactory<News>
    {
        private IConfiguration _config;

        public NewsRepositoryFactory(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IRepository<News>> CreateAsync(string lang)
        {
            var path = Path.Combine(_config["RepositoryStore"], "dex-news");
            if (!Directory.Exists(path))
            {
                await GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                    "kyubey-network", "dex-nesws", "master",
                    Path.Combine(_config["RepositoryStore"], "dex-news"));
            }
            return new NewsRepository(path, lang);
        }

        public IRepository<News> Create(string lang)
        {
            return CreateAsync(lang).Result;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewsRepositoryExtensions
    {
        public static IServiceCollection AddNewsRepositoryFactory(this IServiceCollection self)
        {
            return self.AddSingleton<NewsRepositoryFactory>();
        }
    }
}