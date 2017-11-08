using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Comments.Demo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            const string authSchema = CookieAuthenticationDefaults.AuthenticationScheme;

            loggerFactory.AddConsole();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = authSchema
            });
            
            // middleware to fake sign in and sign out user
            app.Use(async (httpCtx, next) =>
            {
                if (httpCtx.Request.Method == "POST")
                {
                    if (httpCtx.Request.Path.StartsWithSegments("/user/login"))
                    {
                        if (!httpCtx.User.Identity.IsAuthenticated)
                        {
                            var identity = new ClaimsIdentity(new List<Claim> { new Claim("comments-admin", "") }, authSchema);
                            await httpCtx.Authentication.SignInAsync(
                                authSchema,
                                new ClaimsPrincipal(identity));
                        }
                        httpCtx.Response.StatusCode = StatusCodes.Status302Found;
                        httpCtx.Response.Headers["Location"] = "/";
                        return;
                    }
                    if (httpCtx.Request.Path.StartsWithSegments("/user/logout"))
                    {
                        if (httpCtx.User.Identity.IsAuthenticated)
                        {
                            await httpCtx.Authentication.SignOutAsync(authSchema);
                        }
                        httpCtx.Response.StatusCode = StatusCodes.Status302Found;
                        httpCtx.Response.Headers["Location"] = "/";
                        return;
                    }
                }
                await next.Invoke();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseComments(o =>
            {
                o.CommentSourceMaxLength = 1000;
                o.IsUserAdminModeratorCheck = httpCtx => 
                    httpCtx.User.Identity.IsAuthenticated && httpCtx.User.Claims.Any(x => x.Type == "comments-admin");
                // It's called in fire and forget manner
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
                o.InformModerator = async comment =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
                {
                    await Task.Delay(500);
                    Console.WriteLine($"Comment posted on {comment.PageUrl}");
                };
                // o.RequireCommentApproval = true;
            });

            app.Run(async (context) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("not found");
            });
        }
    }
}
