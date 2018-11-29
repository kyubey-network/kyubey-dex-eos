using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Andoromeda.Kyubey.Dex
{
    public class Startup
    {
        private IConfiguration configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddConfiguration(out configuration, "appsettings");
            services.AddDbContext<KyubeyContext>(x => x.UseMySql(configuration["MySql"]));
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new Info() { Title = "Kyubey Dex", Version = "v1" });
                x.DocInclusionPredicate((docName, apiDesc) => apiDesc.HttpMethod != null);
                x.DescribeAllEnumsAsStrings();
            });

            services.AddCors(c => c.AddPolicy("Kyubey", x =>
                x.AllowCredentials()
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            ));

            services.AddTimedJob();

            services.AddNewsRepositoryFactory()
                .AddTokenRepositoryactory();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.UseCors("Kyubey");
            app.UseErrorHandlingMiddleware();
            app.UseStaticFiles();

            var tokensFolder = Path.Combine(env.ContentRootPath, configuration["RepositoryStore"], "token-list");
            if (Directory.Exists(tokensFolder))
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(tokensFolder),
                    RequestPath = new PathString("/token_assets")
                });
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kyubey Dex"));

            app.UseMvcWithDefaultRoute();
            app.UseVueMiddleware();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<KyubeyContext>().Database.EnsureCreated();
                app.UseTimedJob();
            }
        }
    }
}
