using Andoromeda.Framework.GitHub;
using Andoromeda.Kyubey.Dex.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Andoromeda.Kyubey.Dex.Repository.TokenRespository;

namespace Andoromeda.Kyubey.Dex.Repository
{
    public class TokenRespository : IRepository<TokenManifest>
    {
        private string tokenFolderAbsolutePath;
        private const string manifestFileName = "manifest.json";
        private const string iconFileName = "icon.png";

        private string _lang;

        public TokenRespository(string path, string lang)
        {
            _lang = lang;
            tokenFolderAbsolutePath = path;
        }

        public IEnumerable<TokenManifest> EnumerateAll()
        {
            var tokenFolderDirectories = Directory.GetDirectories(tokenFolderAbsolutePath);
            var result = new List<TokenManifest>();
            foreach (var t in tokenFolderDirectories)
            {
                var item = GetSingle(t);
                if (item != null)
                    result.Add(item);
            }
            return result;
        }

        public TokenManifest GetSingle(object id)
        {
            var filePath = Path.Combine(tokenFolderAbsolutePath, id.ToString(), manifestFileName);
            if (File.Exists(filePath))
            {
                var fileContent = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TokenManifest>(fileContent);
            }
            return null;
        }

        public string GetTokenIconPath(string tokenId)
        {
            var absolutePath = Path.Combine(tokenFolderAbsolutePath, tokenId, iconFileName);
            if (File.Exists(absolutePath))
            {
                return absolutePath;
            }
            return null;
        }

        public string GetPriceJsText(string tokenId)
        {
            var filePath = Path.Combine(tokenFolderAbsolutePath, tokenId, "contract_exchange", "price.js");
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }

        public string GetTokenIncubationDescription(string tokenId, string cultureStr)
        {
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "incubator");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = Directory.GetFiles(folderPath, "description.*.txt", SearchOption.TopDirectoryOnly);
            var availableFiles = GetAvailableFileNames(files, cultureStr);
            var availablePath = availableFiles.Select(x => Path.Combine(folderPath, x)).FirstOrDefault();
            if (availablePath != null)
                return File.ReadAllText(availablePath);
            return null;
        }

        private string[] AvailuableCulcureFileSuffix = new string[] {
            TokenCultureFileSuffix.EN,
            TokenCultureFileSuffix.JP,
            TokenCultureFileSuffix.ZHCN,
            TokenCultureFileSuffix.ZHTW
        };

        private string GetFileNameSuffixByCulture(string cultureStr)
        {
            if (new string[] { "en" }.Contains(cultureStr))
                return TokenCultureFileSuffix.EN;
            if (new string[] { "zh" }.Contains(cultureStr))
                return TokenCultureFileSuffix.ZHCN;
            if (new string[] { "zh-Hant" }.Contains(cultureStr))
                return TokenCultureFileSuffix.ZHTW;
            if (new string[] { "ja" }.Contains(cultureStr))
                return TokenCultureFileSuffix.JP;
            return "";
        }

        private string[] GetAvailableFileNames(string[] fileNames, string cultureStr)
        {
            var cultureSuffix = GetFileNameSuffixByCulture(cultureStr);
            string[] availuableFilenames = null;
            if (!string.IsNullOrEmpty(cultureSuffix))
            {
                //current culture 
                availuableFilenames = fileNames.Where(x => x.Contains(cultureSuffix + ".")).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;

                //zh-cn no file, -->zh-tw
                if (cultureSuffix == TokenCultureFileSuffix.ZHCN)
                    availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.ZHTW + ".")).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;

                //zh-tw no file, -->zh-cn
                if (cultureSuffix == TokenCultureFileSuffix.ZHTW)
                    availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.ZHCN + ".")).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;
            }
            //en
            availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.EN + ".")).ToArray();
            if (availuableFilenames.Count() > 0)
                return availuableFilenames;

            //default
            availuableFilenames = fileNames.Where(x => !AvailuableCulcureFileSuffix.Any(c => x.Contains(c))).ToArray();
            return availuableFilenames;
        }

        public class TokenCultureFileSuffix
        {
            public const string ZHCN = ".zh";
            public const string ZHTW = ".zh-Hant";
            public const string EN = ".en";
            public const string JP = ".ja";
        }

        public class TokenRepositoryFactory 
        {
            private IConfiguration _config;
            private IHostingEnvironment _hostingEnv;

            public TokenRepositoryFactory(IConfiguration config, IHostingEnvironment hostingEnv)
            {
                _config = config;
                _hostingEnv = hostingEnv;
            }

            public async Task<TokenRespository> CreateAsync(string lang)
            {
                var path = Path.Combine(_hostingEnv.ContentRootPath, _config["RepositoryStore"], "token-list");
                if (!Directory.Exists(path))
                {
                    await GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
    "kyubey-network", "token-list", "master",
    Path.Combine(_config["RepositoryStore"], "token-list"));
                }
                return new TokenRespository(path, lang);
            }

            public TokenRespository Create(string lang)
            {
                return CreateAsync(lang).Result;
            }
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TokenRepositoryExtensions
    {
        public static IServiceCollection AddTokenRepositoryactory(this IServiceCollection self)
        {
            return self.AddSingleton<TokenRepositoryFactory>();
        }
    }
}