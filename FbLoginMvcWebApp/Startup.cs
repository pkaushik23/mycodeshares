using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace onlyFbLogin
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
            services.AddControllersWithViews();
            
            // Option 1.
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

            //Option 2.
           // services.AddAuthentication(options =>
           //  {
           //    //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
           //    //options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
           //})
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = new PathString("/Account/Login/");
                
            })
            .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                /*
                 * specifying a sign in scheme is required, when we dont configure the defaults in AddAuthentication(), option 2
                 */
                //options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // "MyCookieMiddlewareInstance";
                options.AppId = "";
                options.AppSecret = "";
                /*
                 * specifying a CallbackPath, default is signin-facebook in which cookie handler handles
                 */
                //cfg.CallbackPath = new PathString("/Account/SignIn");
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
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
