using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAPI.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace IdentityAPI
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
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddDbContext<IdentityDBContext>(options => {
                options.UseInMemoryDatabase(databaseName: "EventDb");
                //options.UseSqlServer(Configuration.GetConnectionString("EventSqlConnection"));
            });

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info {
                                            Title ="Identity API",
                                            Version ="v1",
                                            Contact =new Contact { Email="identity@email.com", Name="Identity"}
                                            }
                );
            });

            services.AddCors(c => {
                //c.AddPolicy("All", builder =>
                //{
                //    builder.AllowAnyOrigin().WithMethods("POST").AllowAnyHeader();
                //});
                //c.DefaultPolicyName = "All";

                c.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                            .WithMethods("POST")
                            .AllowAnyHeader();
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            InitializeDatabase(app);

            app.UseSwagger();
            if(env.IsDevelopment())
            {
                app.UseSwaggerUI(c=> {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");
                    c.RoutePrefix = "";
                });
            }

            app.UseCors();
            app.UseMvc();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<IdentityDBContext>();

                db.Users.Add(new Models.UserInfo
                {
                    Email = "sonu@email.com",
                    Password = "sonu123",
                    FirstName = "sonu",
                    LastName = "satha"
                });
                db.Users.Add(new Models.UserInfo
                {
                    Email = "sujit@email.com",
                    Password = "sujit123",
                    FirstName = "sujit",
                    LastName = "D"
                });
                db.Users.Add(new Models.UserInfo
                {
                    Email = "nitish@email.com",
                    Password = "nitish123",
                    FirstName = "Nitish",
                    LastName = "Lad"
                });
                db.SaveChanges();
            }
        }
    }
}
