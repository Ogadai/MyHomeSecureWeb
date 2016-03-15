using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using RazorEngine;
using RazorEngine.Templating;

namespace MyHomeSecureWeb.Utilities
{
    public class HtmlActionResult : IHttpActionResult
    {
        private readonly string _view;
        private readonly dynamic _model;

        private static string ViewPath = "~/Views";

        private static bool _initialised = false;
        private static object _initialiseLock = new object();
        private static void InitialiseTemplates()
        {
            lock(_initialiseLock)
            {
                if (!_initialised)
                {
                    var partials = GetTemplates("_*");
                    foreach (var partial in partials)
                    {
                        Razor.Compile(LoadView(partial), partial + ".cshtml");
                    }
                    _initialised = true;
                }
            }
        }

        public HtmlActionResult(string viewName, dynamic model)
        {
            InitialiseTemplates();

            _view = LoadView(viewName);
            _model = model;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var parsedView = Razor.Parse(_view, _model);

            response.Content = new StringContent(parsedView);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return Task.FromResult(response);
        }

        private static string LoadView(string name)
        {
            var viewDirectory = HttpContext.Current.Server.MapPath(ViewPath);
            var view = File.ReadAllText(Path.Combine(viewDirectory, name + ".cshtml"));
            return view;
        }

        private static string[] GetTemplates(string query)
        {
            var viewDirectory = HttpContext.Current.Server.MapPath(ViewPath);
            var files = Directory.GetFiles(viewDirectory, query + ".cshtml");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
        }
    }
}
