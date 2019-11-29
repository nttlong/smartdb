

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net;

using System.Collections;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace ReEngine.Web
{
    
    public class MW
    {

        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _environment;
        private Object lockAuthCheck = new Object();
        static Hashtable cacheUrlHandlers = new Hashtable();
        private ApplicationInfo[] apps;
        private static ApplicationInfo DefaultApp;
        private static ApplicationInfo DefaultMultiTenancyApp;
        private readonly object objLockCacheUrlHandlers = new object();

        public MW(RequestDelegate next, IHostingEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.StartsWith("/api/actions/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
            if ((context.Request.Method == "POST") && (context.Request.Path.Value.StartsWith("/api-page-post/")))
            {
                await _next(context);
                return;
            }
            //if (context.Request.Method == "POST")
            //{
            //    var route = ReEngine.WebHost.Get().Apps.SelectMany(p => p.ApiEndPoints)
            //        .Select(p=>new {
            //            p.Uri,
            //            ReCheck= new Regex(p.Uri),
            //            IsMatch = (new Regex(p.Uri)).IsMatch(context.Request.Path.Value.TrimStart('/').TrimEnd('/')),
            //            Matchs= (new Regex(p.Uri)).Match(context.Request.Path.Value.TrimStart('/').TrimEnd('/'))
            //        })
            //        .FirstOrDefault(p=>p.IsMatch);
            //}
            if (context.Request.Method == "GET")
            {
                if (cacheUrlHandlers[context.Request.Path.Value] == null)
                {
                    lock (objLockCacheUrlHandlers)
                    {

                        string handleFile;
                        ApplicationInfo app = null;
                        if (apps == null)
                        {
                            apps = ReEngine.WebHost.Get().Apps;
                            foreach (var _app in apps)
                            {
                                _app.OnResourceChange(() =>
                                {
                                    cacheUrlHandlers.Clear();
                                });
                            }
                            DefaultApp = apps.FirstOrDefault(p => p.HostDir == "" && p.IsMultiTenancy == false);
                            DefaultMultiTenancyApp = apps.FirstOrDefault(p => p.HostDir == "" && p.IsMultiTenancy == true);

                        }

                        if (cacheUrlHandlers[context.Request.Path.Value] == null)
                        {
                            var items = context.Request.Path.Value.Split('/');
                            if (DefaultApp != null)
                            {
                                if (context.Request.Path == "/" && (!string.IsNullOrEmpty(DefaultApp.IndexPage)))
                                {
                                    handleFile = string.Join(
                                        Path.DirectorySeparatorChar,
                                        ReEngine.Apps.BaseAppDirectory,
                                        ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                                        DefaultApp.Dir.Replace('/', Path.DirectorySeparatorChar),
                                        DefaultApp.IndexPage);
                                    var relFilePath = Path.GetRelativePath(ReEngine.Apps.BaseAppDirectory, handleFile);
                                    var Extension = Path.GetExtension(relFilePath);
                                    var relFilePathWithoutExtension = relFilePath.Substring(0, relFilePath.Length - Extension.Length);
                                    var retlDir = Path.GetDirectoryName(relFilePath);
                                    var dir = Path.GetDirectoryName(handleFile);
                                    cacheUrlHandlers[context.Request.Path.Value] = new HandlerRequest
                                    {
                                        App = DefaultApp,
                                        FileName = handleFile,
                                        RelFilePath = relFilePath,
                                        RefFilePath = Path.GetRelativePath(string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar), DefaultApp.Dir.Replace('/', Path.DirectorySeparatorChar)), relFilePathWithoutExtension).Replace(Path.DirectorySeparatorChar, '/'),
                                        FileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, dir, Path.GetFileNameWithoutExtension(handleFile)),
                                        RelFileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, retlDir, Path.GetFileNameWithoutExtension(handleFile)),
                                    };
                                }
                                else if ((handleFile = ReEngine.TemplateEngine.CheckFile(DefaultApp.Dir, context.Request.Path.Value)) != null)
                                {
                                    var relFilePath = Path.GetRelativePath(ReEngine.WebHost.Get().AppsDirectory, handleFile);
                                    var retlDir = Path.GetDirectoryName(relFilePath);
                                    var dir = Path.GetDirectoryName(handleFile);
                                    var Extension = Path.GetExtension(relFilePath);
                                    var relFilePathWithoutExtension = relFilePath.Substring(0, relFilePath.Length - Extension.Length);
                                    cacheUrlHandlers[context.Request.Path.Value] = new HandlerRequest
                                    {
                                        App = DefaultApp,
                                        FileName = handleFile,
                                        RelFilePath = relFilePath,
                                        RefFilePath = Path.GetRelativePath(DefaultApp.Dir, relFilePathWithoutExtension),
                                        FileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, dir, Path.GetFileNameWithoutExtension(handleFile)),
                                        RelFileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, retlDir, Path.GetFileNameWithoutExtension(handleFile)),
                                    };
                                }
                                else if ((items.Length > 1) && (app = apps.FirstOrDefault(p => string.Equals(p.Name, items[1], StringComparison.OrdinalIgnoreCase))) != null)
                                {
                                    handleFile = CreateHandlerUrl(context, app, items);
                                }
                                if (items.Length > 2 && ((app = apps.FirstOrDefault(p => p.IsMultiTenancy && string.Equals(p.HostDir, items[2], StringComparison.OrdinalIgnoreCase))) != null))
                                {
                                    handleFile = CreateHandlerUrl(context, app, items);
                                }
                                else if(cacheUrlHandlers[context.Request.Path.Value]==null)
                                {
                                    cacheUrlHandlers[context.Request.Path.Value] = false;
                                }
                            }
                            else if (DefaultMultiTenancyApp != null)
                            {
                                if (ReEngine.TenancyProvider.IsValid(items[1]))
                                {
                                    var fPath = context.Request.Path.Value.Substring(items[1].Length + 1, context.Request.Path.Value.Length - items[1].Length - 1);
                                    if ((handleFile = ReEngine.TemplateEngine.CheckFile(DefaultMultiTenancyApp.Dir, fPath)) != null)
                                    {
                                        var relFilePath = Path.GetRelativePath(ReEngine.WebHost.Get().AppsDirectory, handleFile);
                                        var retlDir = Path.GetDirectoryName(relFilePath);
                                        var dir = Path.GetDirectoryName(handleFile);
                                        var Extension = Path.GetExtension(relFilePath);
                                        var relFilePathWithoutExtension = relFilePath.Substring(0, relFilePath.Length - Extension.Length);
                                        cacheUrlHandlers[context.Request.Path.Value] = new HandlerRequest
                                        {
                                            App = app,
                                            FileName = handleFile,
                                            RelFilePath = relFilePath,
                                            RefFilePath = Path.GetRelativePath(app.Dir, relFilePathWithoutExtension),
                                            FileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, dir, Path.GetFileNameWithoutExtension(handleFile)),
                                            RelFileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, retlDir, Path.GetFileNameWithoutExtension(handleFile)),
                                        };
                                    }
                                    else
                                    {
                                        cacheUrlHandlers[context.Request.Path.Value] = false;
                                    }
                                }

                            }
                            else if (items.Length > 2 && ((app = apps.FirstOrDefault(p => p.IsMultiTenancy && string.Equals(p.HostDir, items[2], StringComparison.OrdinalIgnoreCase))) != null))
                            {

                            }
                            else
                            {
                                if (context.Request.Path.Value == "/")
                                {

                                    if (DefaultApp != null)
                                    {
                                        handleFile = string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory, DefaultApp.Dir, DefaultApp.IndexPage);
                                        var relFilePath = Path.GetRelativePath(ReEngine.WebHost.Get().AppsDirectory, handleFile);
                                        var retlDir = Path.GetDirectoryName(relFilePath);
                                        var dir = Path.GetDirectoryName(handleFile);
                                        var Extension = Path.GetExtension(relFilePath);
                                        var relFilePathWithoutExtension = relFilePath.Substring(0, relFilePath.Length - Extension.Length);
                                        cacheUrlHandlers[context.Request.Path.Value] = new HandlerRequest
                                        {
                                            App = DefaultApp,
                                            FileName = handleFile,
                                            RelFilePath = relFilePath,
                                            RefFilePath = Path.GetRelativePath(DefaultApp.Dir, relFilePathWithoutExtension),
                                            FileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, dir, Path.GetFileNameWithoutExtension(handleFile)),
                                            RelFileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, retlDir, Path.GetFileNameWithoutExtension(handleFile)),
                                        };
                                    }
                                    else
                                    {
                                        context.Response.StatusCode = 403;
                                        return;
                                    }


                                }
                                else if ((app = apps.FirstOrDefault(p => string.Equals(p.HostDir, items[1], StringComparison.OrdinalIgnoreCase) && p.IsMultiTenancy == false)) != null)
                                {
                                    var x = app;
                                }
                                else
                                {
                                    if (items.Length == 3)
                                    {
                                        app = apps.FirstOrDefault(p => string.Equals(p.HostDir, items[2], StringComparison.OrdinalIgnoreCase));

                                        if (app != null)
                                        {
                                            cacheUrlHandlers[context.Request.Path.Value] = new HandlerRequest
                                            {
                                                App = app,
                                                FileName = string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory, app.Dir, "index.html")
                                            };
                                        }
                                    }
                                    else if (Path.GetExtension(context.Request.Path.Value) != "")
                                    {
                                        var x = 1;
                                    }
                                }
                            }
                        }
                        if (cacheUrlHandlers[context.Request.Path.Value] == null)
                        {
                            cacheUrlHandlers[context.Request.Path.Value] = true;
                        }
                    }
                }
                if (cacheUrlHandlers[context.Request.Path.Value] is HandlerRequest)
                {
                    await ((HandlerRequest)cacheUrlHandlers[context.Request.Path.Value]).RunContext(_next, context);
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }

        }

        private static string CreateHandlerUrl(HttpContext context, ApplicationInfo app, string[] items)
        {
            string handleFile = null;
            var checkPath = "";
            if (items.Length > 2)
            {
                if (!app.IsMultiTenancy)
                {
                    for (var i = 2; i < items.Length; i++)
                    {
                        checkPath += items[i] + Path.DirectorySeparatorChar.ToString();
                    }
                }
                else
                {
                    for (var i = 3; i < items.Length; i++)
                    {
                        checkPath += items[i] + Path.DirectorySeparatorChar.ToString();
                    }
                }
                checkPath = checkPath.TrimEnd(Path.DirectorySeparatorChar);
                handleFile = ReEngine.TemplateEngine.CheckFile(app.Dir, checkPath);
            }
            else
            {
                handleFile = ReEngine.TemplateEngine.CheckFile(app.Dir, app.IndexPage);
            }
            if (handleFile != null)
            {
                var relFilePath = Path.GetRelativePath(ReEngine.WebHost.Get().AppsDirectory, handleFile);
                var retlDir = Path.GetDirectoryName(relFilePath);
                var dir = Path.GetDirectoryName(handleFile);
                var Extension = Path.GetExtension(relFilePath);
                var relFilePathWithoutExtension = relFilePath.Substring(0, relFilePath.Length - Extension.Length);
                cacheUrlHandlers[context.Request.Path.Value] = new HandlerRequest
                {
                    App = app,
                    FileName = handleFile,
                    RelFilePath = relFilePath,
                    RefFilePath = Path.GetRelativePath(app.Dir, relFilePathWithoutExtension),
                    FileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, dir, Path.GetFileNameWithoutExtension(handleFile)),
                    RelFileNameWithoutExtension = string.Join(Path.DirectorySeparatorChar, retlDir, Path.GetFileNameWithoutExtension(handleFile)),
                };
            }

            return handleFile;
        }
    }

    #region ExtensionMethod
    public static class UseMiddlewareExtension
    {
        public static IApplicationBuilder UseReEngineMiddleWare(this IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<MW>(env);
            return app;
        }
    }

    #endregion
}
