using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReEngine.Web
{
    [ApiController]
    public class ApiActions : ControllerBase
    {
        static Hashtable AppsCache = new Hashtable();
        private readonly object objLockAppsCahce = new object();
        private ApplicationInfo GetAppInfo(string AppName)
        {
            if (AppsCache[AppName] == null)
            {
                lock (objLockAppsCahce)
                {
                    if (AppsCache[AppName] == null)
                    {
                        AppsCache[AppName] = ReEngine.WebHost.Get().Apps.FirstOrDefault(p => string.Equals(p.Name, AppName, StringComparison.OrdinalIgnoreCase));
                        if (AppsCache[AppName] == null)
                        {
                            AppsCache[AppName] = false;
                        }
                    }
                }
            }
            ReEngine.HttpUtils.SetAppToCurrentContext((ApplicationInfo)AppsCache[AppName]);
           
            return AppsCache[AppName] as ApplicationInfo;
        }
        [Route("api/actions/{AppName}/{ActionId}")]
        [HttpPost]
        [EnableCors("CorsPolicy")]
        public IActionResult PostData(string AppName, string ActionId, [FromBody] Hashtable Data)
        {
            GetAppInfo(AppName);
            var log = log4net.LogManager.GetLogger(typeof(ApiActions));
            try
            {
                var inputParams = Data.Cast<DictionaryEntry>().Select(p => new
                {
                    Name = p.Key.ToString(),
                    Value = p.Value
                });
                var ActionHandler = Actions.GetActionRegister(AppName, ActionId);
                if (ActionHandler == null)
                {
                    return StatusCode(403);
                }
                var retData = ActionHandler(new ActionContext()
                {

                    PostData = Data

                });
                return Ok(retData);

            }
            catch (UnauthorizedAccessException ex)
            {
                log.Debug(ex);
                return StatusCode(401);
            }
            catch (Exception ex)
            {

                log.Error(ex);
                throw (ex);
            }


        }



        [Route("api-media/{tenancy}/{appName}/{CallPage}/{MethodCall}/{Id}")]
        [HttpGet]
        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetDataVersion3(string tenancy, string appName, string CallPage, string MethodCall, string Id)
        {
            var log = log4net.LogManager.GetLogger(typeof(ApiActions));
            var path = CallPage.TrimStart('$').TrimEnd('$').Replace('-', Path.DirectorySeparatorChar).Replace('$', Path.DirectorySeparatorChar);
            var fPath = string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar), appName, path);
            DynamicMethodInfo methodResult = null;
            var rContext = new ReEngine.Context()
            {
                AppName = appName,
                Tenancy = tenancy,
                HttpContext = HttpContext,
                ScriptPath = fPath + ".csx",
                PageId = CallPage

            };
            try
            {
                methodResult = ReEngine.ScriprLoader.LoadMethod(fPath + ".csx", MethodCall);
            }
            catch (Exception ex)
            {
                rContext.Logger.Debug(ex);
                log.Debug(ex);
                return StatusCode(500);
            }


            var Params = new List<object>();
            Params.Add(rContext);
            Params.AddRange(Id.Split('$'));
            if (methodResult == null || methodResult.Method == null)
            {
                return StatusCode(403);
            }
            try
            {
                var retVal = methodResult.Method.Invoke(null, Params.ToArray());
                return await ProcessMediaContent(fPath + ".csx", methodResult.Method, rContext, retVal);
            }
            catch (UnauthorizedAccessException ex)
            {
                log.Debug(ex);
                rContext.Logger.Debug(ex);
                return StatusCode(401);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is UnauthorizedAccessException)
                {
                    log.Debug(ex.InnerException);
                    rContext.Logger.Debug(ex.InnerException);
                    return StatusCode(401);
                }
                else
                {
                    log.Debug(ex.InnerException);
                    rContext.Logger.Debug(ex.InnerException);
                    return StatusCode(500);
                }
            }
            catch (TargetParameterCountException ex)
            {
                log.Debug(ex);
                rContext.Logger.Debug(ex);
                return StatusCode(500);
            }
            catch (Exception ex)
            {

                log.Debug(ex);
                rContext.Logger.Debug(ex);
                throw (ex);
            }
            //var strParams = Id.Split('$');
            //if ((appName == "default") || (appName == "_"))
            //{
            //    appName = "";
            //}
            //if ((tenancy == "default") || (appName == "_"))
            //{
            //    tenancy = "";
            //}
            //this.HttpContext.Items["tenancy"] = tenancy;
            //this.HttpContext.Items["app"] = appName;
            //string path = "";
            //var log = log4net.LogManager.GetLogger(typeof(CallApiController));
            //try
            //{
            //    string AppName = appName;
            //    path = CallPage.TrimStart('$').TrimEnd('$').Replace('-', Path.DirectorySeparatorChar).Replace('$', Path.DirectorySeparatorChar);
            //    var dir = Path.GetDirectoryName(path);
            //    var items = path.Split(Path.DirectorySeparatorChar).ToList();
            //    var crtCode = items[items.Count - 1];
            //    items.Add("server");
            //    items.Add(crtCode);
            //    path = string.Join(Path.DirectorySeparatorChar, items) + ".csx";
            //    var PageId = CallPage.Replace("$", "/").Replace("-", "/").TrimStart('/').TrimEnd('/') + ".html";
            //    if (this.Request.Query["pageId"].Count > 0)
            //    {
            //        PageId = this.Request.Query["pageId"][0].Replace("//", "/").TrimStart('/').TrimEnd('/') + ".html";
            //    }
            //    string Token = ((this.HttpContext != null) ? this.HttpContext.Session.Id : null);
            //    string LanguageCode = "en";
            //    if (Request.Query["token"].Count > 0)
            //    {
            //        Token = Request.Query["token"][0];
            //    }
            //    if (Request.Query["lang"].Count > 0)
            //    {
            //        LanguageCode = Request.Query["lang"][0];
            //    }
            //    string Tenancy = tenancy;

            //    var sourcePath = string.Join(Path.DirectorySeparatorChar, ApiConfig.GetBaseApiDir(), path);
            //    var methodResult = ReEngine.ScriprLoader.LoadMethod(sourcePath, MethodCall);
            //    if (methodResult == null || methodResult.Method == null)
            //    {
            //        return StatusCode(403);
            //    }
            //    else
            //    {
            //        var method = methodResult.Method;
            //        var inputParsOfMethod = method.GetParameters();
            //        var pars = new List<object>();
            //        var rPars = new List<object>();
            //        if (inputParsOfMethod.Length == 0)
            //        {
            //            log.Error(string.Format("'{0}' in '{1}' require one param", method.Name, sourcePath));
            //            return StatusCode(403);
            //        }
            //        var contextParamType = inputParsOfMethod[0].ParameterType;
            //        var rContext = new ReEngine.Context
            //        {
            //            AppName = appName,
            //            Tenancy = tenancy,
            //            Token = string.IsNullOrEmpty(Token) ? HttpContext.Session.Id : Token,
            //            HttpContext = HttpContext,
            //            AbsUrl = this.AbsUrl,
            //            ScriptPath = sourcePath,
            //            PageId = path

            //        };
            //        var contextParam = ReEngine.ScriprLoader.CreateContext(contextParamType, rContext);

            //        rPars.Add(contextParam);
            //        rPars.AddRange(Id.Split('$'));

            //        try
            //        {
            //            var retVal = method.Invoke(null, rPars.ToArray());
            //            return await ProcessMediaContent(sourcePath, method, rContext, retVal);

            //        }
            //        catch (Exception ex)
            //        {
            //            rContext.Logger.Debug(ex);
            //            throw (ex);
            //        }

            //    }
            //}
            //catch (UnauthorizedAccessException ex)
            //{
            //    log.Debug(path, ex);
            //    log.Error(FlattenException(ex));
            //    return StatusCode(401);
            //}
            //catch (TargetInvocationException ex)
            //{
            //    if (ex.InnerException is UnauthorizedAccessException)
            //    {
            //        log.Debug(path, ex.InnerException);
            //        return StatusCode(401);
            //    }
            //    else
            //    {
            //        log.Debug(path, ex.InnerException);
            //        log.Error(FlattenException(ex));
            //        return StatusCode(500);
            //    }
            //}
            //catch (TargetParameterCountException ex)
            //{
            //    log.Debug(path, ex);
            //    log.Error(FlattenException(ex));
            //    return StatusCode(500);
            //}
            //catch (Exception ex)
            //{

            //    log.Error(FlattenException(ex));
            //    throw (ex);
            //}

            return null;
        }
        [Route("{AppName}/api/media/{CallPage}/{MethodCall}/{Id}")]
        [HttpGet()]
        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> ApiGetCommonMedia(string AppName, string CallPage, string MethodCall, string Id)
        {
            CallPage = CallPage.Replace("_", "-");
            return await GetDataVersion3("", AppName, CallPage, MethodCall, Id);
        }
        private async Task<IActionResult> ProcessMediaContent(string sourcePath, MethodInfo method, Context rContext, object retVal)
        {
            
            if (retVal is ReEngine.Redirect)
            {
                return Redirect(((ReEngine.Redirect)retVal).Url);
            }
            else if (retVal is MediaContent)
            {
                var mContent = retVal as MediaContent;
                return File(mContent.Data, mContent.MimeType);
            }
            else if (retVal is MediaFile)
            {
                Stream stream = await LoadFile(((MediaFile)retVal).FileName);

                if (stream == null)
                    return NotFound(); // returns a NotFoundResult with Status404NotFound response.

                return File(stream, ReEngine.MediaContent.GetMimeTypeFromFilePath(((MediaFile)retVal).FileName));
            }
            else if (retVal is AsyncMediaContent)
            {
                if (((AsyncMediaContent)retVal).FileName != null)
                {
                    return File(
                        await ((AsyncMediaContent)retVal).AysncStream,
                        ((AsyncMediaContent)retVal).MimeType,
                        ((AsyncMediaContent)retVal).FileName,true

                        );
                }
                else
                {
                    return File(
                        await ((AsyncMediaContent)retVal).AysncStream,
                        ((AsyncMediaContent)retVal).MimeType, true
                        );
                }
            }
            else
            {
                var msg = string.Format(
                    "the method '{0}' in '{1}' must return '{2}' or '{3}' or '{4}'",
                    method.Name, sourcePath,
                    typeof(MediaContent).FullName,
                    typeof(Redirect).FullName,
                    typeof(MediaFile).FullName,
                    typeof(AsyncMediaContent).FullName);
                rContext.Logger.Debug(msg);
                throw (new Exception(msg));
            }
        }
        private async Task<Stream> LoadFile(string fileName)
        {

            var fx = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read) as Stream;
            return fx;
        }
        [HttpPost]
        [Route("api-page-post/{Tenancy}/{AppName}/{PagePath}/{Method}")]
        [Route("api/page-post/{Tenancy}/{AppName}/{PagePath}/{Method}")]
        public IActionResult ApiPagePost(string Tenancy, string AppName, string PagePath, string Method, [FromBody] Hashtable Data)
        {
            var log = log4net.LogManager.GetLogger(typeof(ApiActions));

            var app = GetAppInfo(AppName);
            if (app != null)
            {
                var strPath = PagePath.Replace('-', Path.DirectorySeparatorChar);
                var handlerPath = string.Join(
                    Path.DirectorySeparatorChar,
                    ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                    app.Dir,
                    strPath + ".csx"
                    );
                var context = new ReEngine.Context()
                {
                    AbsUrl = HttpUtils.GetAbsUrl(),
                    Application = app,
                    AppName = app.Name,
                    DbSchema = "",
                    HttpContext = this.HttpContext,
                    LanguageCode = HttpUtils.GetLanguageCode(),
                    ScriptPath = handlerPath,
                    Token = this.HttpContext.Session.Id
                };


                try
                {
                    var scriptHost = ReEngine.ScriprLoader.LoadMethod(handlerPath, Method);
                    var method = scriptHost.Method;
                    if (method == null)
                    {
                        context.Logger.Debug(new
                        {
                            code = "MethodNotFound",
                            message = $"'{Method}' was not found in '{handlerPath}'"
                        });
                        return StatusCode(500);
                    }
                    var inputParsOfMethod = method.GetParameters();
                    var inputParams = Data.Cast<DictionaryEntry>().Select(p => new
                    {

                        Name = p.Key.ToString(),
                        Value = p.Value
                    });
                    var pars = (from y in inputParsOfMethod
                                from x in inputParams.Where(p => string.Equals(p.Name.ToString(), y.Name, StringComparison.OrdinalIgnoreCase)).DefaultIfEmpty()
                                select new
                                {
                                    Value = (x != null) ? x.Value : null,
                                    Type = y.ParameterType
                                }).ToList();
                    var rPars = new List<object>();
                    rPars.Add(context);
                    foreach (var x in pars)
                    {
                        if (pars.IndexOf(x) > 0)
                        {
                            if (x.Value is Newtonsoft.Json.Linq.JObject)
                            {
                                var v = x.Value as Newtonsoft.Json.Linq.JObject;
                                rPars.Add(v.ToObject(x.Type));
                            }
                            else if (x.Value is Newtonsoft.Json.Linq.JArray)
                            {
                                var v = x.Value as Newtonsoft.Json.Linq.JArray;
                                rPars.Add(v.ToObject(x.Type));
                            }
                            else
                            {
                                rPars.Add(x.Value);
                            }
                        }
                    }
                    object retVal = null;
                    try
                    {
                        retVal = method.Invoke(null, rPars.ToArray());
                        return Ok(retVal);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        context.Logger.Debug(ex);
                        return StatusCode(401);
                    }
                    catch (Exception ex)
                    {
                        context.Logger.Debug(ex);
                        return StatusCode(500);
                    }

                    return Ok(retVal);
                }
                catch (Exception ex)
                {

                    context.Logger.Debug(ex);
                    return StatusCode(500);
                }

            }
            else
            {
                log.Debug(new
                {
                    url = this.Request.Path.Value,
                    message = $"App '{AppName} was not found"
                });
                return StatusCode(500);
            }


        }
        [HttpPost]
        [Route("{AppName}/api/pagesAjax/{PagePath}/{Method}/post")]
        public IActionResult ApiCommonPost(string AppName, string PagePath, string Method, [FromBody] PageAjaxPostInfo Data)
        {
            var log = log4net.LogManager.GetLogger(typeof(ApiActions));

            var app = GetAppInfo(AppName);
            if (app != null)
            {
                var strPath = PagePath.TrimStart(@"n$".ToArray()).Replace('-', Path.DirectorySeparatorChar).Replace('$', Path.DirectorySeparatorChar);
                
                var handlerPath = string.Join(
                    Path.DirectorySeparatorChar,
                    ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                    app.Dir,
                    //"server-api",
                    strPath + ".csx"
                    );
                var context = new ReEngine.Context()
                {
                    AbsUrl = HttpUtils.GetAbsUrl(),
                    Application = app,
                    AppName = app.Name,
                    DbSchema = "",
                    HttpContext = this.HttpContext,
                    LanguageCode = HttpUtils.GetLanguageCode(),
                    ScriptPath = handlerPath,
                    Token = this.HttpContext.Session.Id
                };


                try
                {
                    var scriptHost = ReEngine.ScriprLoader.LoadMethod(handlerPath, Method);
                    var method = scriptHost.Method;
                    if (method == null)
                    {
                        context.Logger.Debug(new
                        {
                            code = "MethodNotFound",
                            message = $"'{Method}' was not found in '{handlerPath}'"
                        });
                        return StatusCode(500);
                    }
                    var inputParsOfMethod = method.GetParameters();
                    if (Data.Data == null)
                    {
                        Data.Data = new Hashtable();
                    }
                    var inputParams =Data.Data.Cast<DictionaryEntry>().Select(p => new
                    {

                        Name = p.Key.ToString(),
                        Value = p.Value
                    });
                    var pars = (from y in inputParsOfMethod
                                from x in inputParams.Where(p => string.Equals(p.Name.ToString(), y.Name, StringComparison.OrdinalIgnoreCase)).DefaultIfEmpty()
                                select new
                                {
                                    Value = (x != null) ? x.Value : null,
                                    Type = y.ParameterType
                                }).ToList();
                    var rPars = new List<object>();
                    rPars.Add(context);
                    foreach (var x in pars)
                    {
                        if (pars.IndexOf(x) > 0)
                        {
                            if (x.Value is Newtonsoft.Json.Linq.JObject)
                            {
                                var v = x.Value as Newtonsoft.Json.Linq.JObject;
                                rPars.Add(v.ToObject(x.Type));
                            }
                            else if (x.Value is Newtonsoft.Json.Linq.JArray)
                            {
                                var v = x.Value as Newtonsoft.Json.Linq.JArray;
                                rPars.Add(v.ToObject(x.Type));
                            }
                            else
                            {
                                rPars.Add(x.Value);
                            }
                        }
                    }
                    object retVal = null;
                    try
                    {
                        retVal = method.Invoke(null, rPars.ToArray());
                        return Ok(retVal);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        context.Logger.Debug(ex);
                        log.Debug(ex);
                        return StatusCode(401);
                    }
                    catch (Exception ex)
                    {
                        context.Logger.Debug(ex);
                        log.Debug(ex);
                        return StatusCode(500);
                    }

                }
                catch (Exception ex)
                {

                    context.Logger.Debug(ex);
                    log.Debug(ex);
                    return StatusCode(500);
                }

            }
            else
            {
                log.Debug(new
                {
                    url = this.Request.Path.Value,
                    message = $"App '{AppName} was not found"
                });
                return StatusCode(500);
            }
        }
        [HttpPost]
        [Route("api/{Tenancy}/{AppName}/{PagePath}/getdata")]
        public IActionResult ApiCommonGetData(string Tenancy, string AppName, string PagePath, [FromBody] ApiCommonPost Data)
        {
            var data= ExecCommonAPICaller(Tenancy, AppName, PagePath, Data);
            if(data is StatusCodeResult)
            {
                return data as StatusCodeResult;
            }
            else
            {
                return Ok(new { Data = data });
            }
        }

        private object ExecCommonAPICaller(string Tenancy, string AppName, string PagePath, ApiCommonPost Data)
        {
            
            var token = "";
            if (this.HttpContext.Request.Query["token"].Count > 0)
            {
                token = this.HttpContext.Request.Query["token"][0];
            }
            if (this.HttpContext.Request.Headers["token"].Count > 0)
            {
                token = this.HttpContext.Request.Headers["token"][0];
            }
            if (this.HttpContext.Session!=null)
            {
                token = this.HttpContext.Session.Id;
            }
            if (string.Equals(Tenancy, "default", StringComparison.OrdinalIgnoreCase) || Tenancy == "_")
            {
                Tenancy = "";
            }
            var log = log4net.LogManager.GetLogger(typeof(ApiActions));
            var app = GetAppInfo(AppName);
            var sourcePath = "";
            var methodName = "";
            if (Data.Path == null)
            {
                Data.Path = PagePath;
            }
            if (Data.Path.IndexOf('@') == -1)
            {
                var CallPaths = Data.Path.Replace('-', '/').Split('/');

                sourcePath = string.Join(
                    Path.DirectorySeparatorChar,
                    ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                    app.Dir,
                    "server-api",
                    CallPaths[0].Replace('.', Path.DirectorySeparatorChar),
                    CallPaths[1]
                    ) + ".csx";
                methodName = CallPaths[2];
            }
            else
            {
                var CallPaths = Data.Path.Split('@');
                sourcePath = string.Join(
                    Path.DirectorySeparatorChar,
                    ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar),
                    app.Dir,
                    "server-api",
                    CallPaths[1].Replace('.', Path.DirectorySeparatorChar),
                    CallPaths[0]
                    ) + ".csx";
                methodName = CallPaths[0];
            }
            var ViewId = "-";
            if (Request.Query["view-id"].Count > 0 && Request.Query["view-id"][0] != "undefined")
            {
                ViewId = Request.Query["view-id"];
            }
            var context = new ReEngine.Context()
            {
                AbsUrl = this.HttpContext.Request.Scheme + "://" + this.HttpContext.Request.Host,
                Application = app,
                AppName = AppName,
                HttpContext = this.HttpContext,
                LanguageCode = HttpUtils.GetLanguageCode(),
                ScriptPath = sourcePath,
                Tenancy = Tenancy,
                ViewId = ViewId,
                Token=token
            };
            try
            {
                var scriptHost = ReEngine.ScriprLoader.LoadMethod(sourcePath, methodName);
                var method = scriptHost.Method;
                if (method == null)
                {
                    context.Logger.Debug(new
                    {
                        code = "MethodNotFound",
                        message = $"'{methodName}' was not found in '{sourcePath}'"
                    });
                    return StatusCode(500);
                }
                var inputParsOfMethod = method.GetParameters();
                var inputParams = Data.Data.Cast<DictionaryEntry>().Select(p => new
                {

                    Name = p.Key.ToString(),
                    Value = p.Value
                });
                var pars = (from y in inputParsOfMethod
                            from x in inputParams.Where(p => string.Equals(p.Name.ToString(), y.Name, StringComparison.OrdinalIgnoreCase)).DefaultIfEmpty()
                            select new
                            {
                                Value = (x != null) ? x.Value : null,
                                Type = y.ParameterType
                            }).ToList();
                var rPars = new List<object>();
                rPars.Add(context);
                foreach (var x in pars)
                {
                    if (pars.IndexOf(x) > 0)
                    {
                        if (x.Value is Newtonsoft.Json.Linq.JObject)
                        {
                            var v = x.Value as Newtonsoft.Json.Linq.JObject;
                            rPars.Add(v.ToObject(x.Type));
                        }
                        else if (x.Value is Newtonsoft.Json.Linq.JArray)
                        {
                            var v = x.Value as Newtonsoft.Json.Linq.JArray;
                            rPars.Add(v.ToObject(x.Type));
                        }
                        
                        else
                        {
                            rPars.Add(x.Value);
                        }
                    }
                }
                object retVal = null;
                try
                {
                    retVal = method.Invoke(null, rPars.ToArray());
                    return retVal;
                }
                catch (UnauthorizedAccessException ex)
                {
                    context.Logger.Debug(ex);
                    return StatusCode(401);
                }
                catch (Exception ex)
                {
                    if(ex.InnerException is UnauthorizedAccessException)
                    {
                        return StatusCode(401);
                    }
                    context.Logger.Debug(ex);
                    return StatusCode(500);
                }

               
            }
            catch (Exception ex)
            {

                context.Logger.Debug(ex);
                log.Debug(ex);
                return StatusCode(403);
            }


            return Ok(new { });
        }

        [HttpPost]
        [Route("{AppName}/api/v2/{ApiPath}/{Methodname}/post")]
        public IActionResult APIV2(string AppName,string ApiPath,string MethodName,[FromBody] ApiCommonPost Data)
        {
            if (Data.Data == null)
            {
                Data.Data = new Hashtable();
            }
            var data = ExecCommonAPICaller("", AppName, ApiPath + "-" + MethodName, Data);
            if (data is StatusCodeResult)
            {
                return data as StatusCodeResult;
            }
            else
            {
                return Ok(data);
            }
        }
        [HttpPost]
        [Route("api/app-post/{Tenancy}/{AppName}/{ApiPath}/{MethodName}")]
        [EnableCors("CorsPolicy")]
        public IActionResult AppApiCaller(string Tenancy, string AppName,string ApiPath,string MethodName, [FromBody] Hashtable Data)
        {
            
            var data = ExecCommonAPICaller(Tenancy, AppName, "api-"+ApiPath + "-" + MethodName,new Web.ApiCommonPost {
                Data=Data
            });
            if (data is StatusCodeResult)
            {
                return data as StatusCodeResult;
            }
            else
            {
                return Ok(data);
            }
        }
        [HttpGet]
        [Route("api/app-get/{Tenancy}/{AppName}/{ApiPath}/{MethodName}/{Id}")]
        public async Task<IActionResult> AppApiGet(string Tenancy, string AppName, string ApiPath, string MethodName, string Id)
        {
            ReEngine.HttpUtils.SetAppToCurrentContext(GetAppInfo(AppName));
            var log = log4net.LogManager.GetLogger(typeof(ApiActions));
            var path = ApiPath.TrimStart('$').TrimEnd('$').Replace('-', Path.DirectorySeparatorChar).Replace('$', Path.DirectorySeparatorChar);
            var fPath = string.Join(Path.DirectorySeparatorChar, ReEngine.WebHost.Get().AppsDirectory.Replace('/', Path.DirectorySeparatorChar), AppName,"server-api","api", path);
            DynamicMethodInfo methodResult = null;
            var rContext = new ReEngine.Context
            {
                AppName = AppName,
                Tenancy = Tenancy,
                HttpContext = HttpContext,
                AbsUrl = ReEngine.HttpUtils.GetAbsUrl(),
                ScriptPath = fPath + ".csx",
                PageId = ApiPath,
                Application = GetAppInfo(AppName)

            };
            if (this.HttpContext.Request.Query["token"].Count > 0)
            {
                rContext.Token = this.HttpContext.Request.Query["token"][0];
            }
            else if (this.HttpContext.Session != null && this.HttpContext.Session.Id != null)
            {
                rContext.Token = this.HttpContext.Session.Id;
            }
            try
            {
                methodResult = ReEngine.ScriprLoader.LoadMethod(fPath + ".csx", MethodName);
            }
            catch (Exception ex)
            {
                rContext.Logger.Debug(ex);
                log.Debug(ex);
                return StatusCode(500);
            }


            var Params = new List<object>();
            Params.Add(rContext);
            Params.AddRange(Id.Split('$'));
            if (methodResult == null || methodResult.Method == null)
            {
                return StatusCode(403);
            }
            try
            {
                var retVal = methodResult.Method.Invoke(null, Params.ToArray());
                return await ProcessMediaContent(fPath + ".csx", methodResult.Method, rContext, retVal);
            }
            catch (UnauthorizedAccessException ex)
            {
                log.Debug(ex);
                rContext.Logger.Debug(ex);
                return StatusCode(401);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is UnauthorizedAccessException)
                {
                    log.Debug(ex.InnerException);
                    rContext.Logger.Debug(ex.InnerException);
                    return StatusCode(401);
                }
                else
                {
                    log.Debug(ex.InnerException);
                    rContext.Logger.Debug(ex.InnerException);
                    return StatusCode(500);
                }
            }
            catch (TargetParameterCountException ex)
            {
                log.Debug(ex);
                rContext.Logger.Debug(ex);
                return StatusCode(500);
            }
            catch (Exception ex)
            {

                log.Debug(ex);
                rContext.Logger.Debug(ex);
                throw (ex);
            }
        }
    }
}
