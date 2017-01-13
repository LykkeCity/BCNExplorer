using System;using System.IO;using System.Text;using System.Threading.Tasks;using System.Web.Mvc;using Common;using Microsoft.ApplicationInsights;namespace BCNExplorer.Web.ErrorHandler{    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]    public class CustomHandleErrorAttribute : HandleErrorAttribute    {        public override void OnException(ExceptionContext filterContext)        {            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)            {
                var ai = new TelemetryClient();
                ai.TrackException(filterContext.Exception);

                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();                var request = filterContext.RequestContext.HttpContext.Request;                Task.Run(() => App.Log.WriteError(controller, action, new                {                    request.Url?.PathAndQuery,                    request.HttpMethod,                    Body = GetDocumentContents(request)                }.ToJson(), filterContext.Exception).ConfigureAwait(false)).Wait();                                base.OnException(filterContext);            }        }        private static string GetDocumentContents(System.Web.HttpRequestBase Request)
        {
            string documentContents;
            using (Stream receiveStream = Request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }    }}