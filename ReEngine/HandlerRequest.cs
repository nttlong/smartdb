using Microsoft.AspNetCore.Http;
using RazorLight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReEngine
{
    public class HandlerRequest
    {
        public HandlerRequest()
        {
            TemplateEngine.PushToCache(HttpUtils.HttpContext.Request.Path.Value, this);
        }
        static Hashtable Engines = new Hashtable();
        public ApplicationInfo App { get;  set; }
        public string FileName { get;  set; }
        public string RelFilePath { get;  set; }
        public string FileNameWithoutExtension { get;  set; }
        public string RelFileNameWithoutExtension { get;  set; }
        public string RefFilePath { get;  set; }
        public string TemplateDirPath { get;  set; }

        static bool HasRegistWriter = false;
        private string _renderFile;
        private readonly object objLockEngine = new object();
        static Hashtable CachceRootDirs = new Hashtable();
        public async Task RunContext(RequestDelegate next, HttpContext context)
        {
            if (Path.GetExtension(this.FileName) == ".cshtml")
            {
                DynamicTemplateInfo templateInfo = DynamicObjects.GetInfo(App, context, this.FileName);
                if (templateInfo == null)
                {
                    context.Response.StatusCode = 403;
                }
                else
                {
                    if (Engines[this.App.Name] == null)
                    {
                        lock (objLockEngine)
                        {
                            if (Engines[this.App.Name] == null)
                            {
                                var rootTemplatePath = string.Join(
                                    Path.DirectorySeparatorChar,
                                    ReEngine.Apps.BaseAppDirectory.Replace('/', Path.DirectorySeparatorChar),
                                    ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                                    this.App.Dir.Replace('/', Path.DirectorySeparatorChar));

                                CachceRootDirs[this.App.Name] = rootTemplatePath;
                                Engines[this.App.Name] = new RazorLightEngineBuilder()
                              .UseFileSystemProject(rootTemplatePath)
                              .UseMemoryCachingProvider()
                              .Build();
                            }
                        }
                    }

                    object data = null;
                    //if (templateInfo.GetModelMethod != null)
                    //{
                    //    data = templateInfo.GetModelMethod.Invoke(null, new object[] { templateInfo.GetContext(context)});
                    //}
                    if (string.IsNullOrEmpty(this._renderFile))
                    {
                        this.TemplateDirPath = CachceRootDirs[this.App.Name].ToString();
                        this._renderFile = Path.GetRelativePath(this.TemplateDirPath, this.FileName);
                        this._renderFile = this._renderFile.Substring(0, this._renderFile.Length - ".cshtml".Length);

                    }
                    ReEngine.HttpUtils.SetAppToCurrentContext(this.App);
                    ReEngine.HttpUtils.SetPagePathToCurrentContext(this.RefFilePath);

                    string result = await ((RazorLightEngine)Engines[this.App.Name]).CompileRenderAsync(this._renderFile, new { });
                    if (context.Response.StatusCode == 302)
                    {
                        return;
                    }
                    var bff = UnicodeEncoding.UTF8.GetBytes(result);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/html;charset=utf-8";
                    context.Response.ContentLength = bff.Length;

                    using (var stream = context.Response.Body)
                    {
                        await stream.WriteAsync(bff, 0, bff.Length);
                        await stream.FlushAsync();
                    }
                }

            }
            else
            {
                await StaticObjects.GetStaticFileInApp(context, this.FileName);
            }
        }
    }
}
