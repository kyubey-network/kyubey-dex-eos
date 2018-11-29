using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Framework.AnnotatedMarkdown;
using Andoromeda.Kyubey.Dex.Repository;

namespace Andoromeda.Kyubey.Dex.Repository
{
    public class NewsRepository
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
            var files = Directory.EnumerateFiles(Path.Combine(_path, "dex-news"));
            return files.Where(x => x.EndsWith($".{_lang}.md"));
        }

        public IEnumerable<News> EnumerateNews()
        {
            foreach(var x in EnumerateNewsFiles())
            {
                var md = File.ReadAllText(x);
                var result = AnnotationParser.Parse(md);
                yield return GetSingleNews(Path.GetFileNameWithoutExtension(x));
            }
        }

        public News GetSingleNews(string id)
        {
            var path = Path.Combine(_path, "dex-news", $"{id}.{_lang}.md");
            var md = File.ReadAllText(path);
            var result = AnnotationParser.Parse(md);
            return new News
            {
                Id = id,
                Title = result.Annotations.ContainsKey("Title") ? result.Annotations["Title"] : null,
                PublishedAt = result.Annotations.ContainsKey("Published at") ? Convert.ToDateTime(result.Annotations["Published at"]) : DateTime.MinValue,
                IsPinned = result.Annotations.ContainsKey("Pinned") ? Convert.ToBoolean(result.Annotations["Pinned"]) : false,
                Content = result.PlainMarkdown
            };
        }
    }

    public class NewsRepositoryFactory
    {
        private IConfiguration _config;

        public NewsRepositoryFactory(IConfiguration config)
        {
            _config = config;
        }

        public NewsRepository Create(string lang)
        {
            return new NewsRepository(_config["RepositoryStore"], lang);
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