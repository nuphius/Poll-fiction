using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PollFiction.Data;
using PollFiction.Services;
using PollFiction.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PollFiction.Web
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
            //déclration du service Dbcontext pour rendre accessible de partout
            services.AddDbContext<AppDbContext>(options =>
            {
                string cn = Configuration.GetConnectionString("cn");
                options.UseSqlServer(cn)
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                ;
            });

            services.AddAuthentication("Cookies")
               .AddCookie("Cookies", options =>
               {
                   options.LogoutPath = "/home/index";
                   options.LoginPath = "/home/login";
                   options.AccessDeniedPath = "/home/accesDenied";
                   options.ReturnUrlParameter = "returnUrl";
                   options.ExpireTimeSpan = TimeSpan.FromDays(1);

                   //config du cookie
                   options.Cookie.HttpOnly = true;
                   options.Cookie.IsEssential = true;
               });

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPollService, PollService>();

            services.AddHttpContextAccessor();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var cultureInfo = new CultureInfo("fr-FR");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //ajoujt de l'étape d'authentification dans le Pipeline
            //L'ORDRE COMPTE !!!!
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
