﻿using System.Collections.Generic;
using Amazon.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace IAMRoleService.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
       
            services.AddTransient<BasicAWSCredentials>(x => new BasicAWSCredentials(
                    Configuration["AWS_ACCESS_KEY_ID"],
                    Configuration["AWS_SECRET_ACCESS_KEY"]
                )
            );

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "IAM Role Service",
                    Version = "v1.0.0",
                    
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMvc();
            
            app.UseSwagger(x =>
            {
                const string basePath = "/api";

                x.PreSerializeFilters.Add((doc, req) =>
                {
                    doc.BasePath = basePath;
                });

                x.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var paths = new Dictionary<string, PathItem>();

                    foreach (var path in swaggerDoc.Paths)
                    {
                        paths.Add(path.Key.Replace(basePath, ""), path.Value);
                    }

                    swaggerDoc.Paths = paths;
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IAM Role Service API");
            });
        }
    }
}
