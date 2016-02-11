using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Utilities;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    [RequireHttps]
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request, Services);
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            var email = "unknown";
            ServiceUser user = this.User as ServiceUser;
            var identities = await user.GetIdentitiesAsync();

            //Check if the user has logged in using Google as Identity provider
            var google = identities.OfType<GoogleCredentials>().FirstOrDefault();
            if (google != null)
            {
                var accessToken = google.AccessToken;
                var googleInfo = await GetProviderInfo("https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + accessToken);
                email = googleInfo.Value<string>("email");
            }


            item.Text = string.Format("{0} - {1}", item.Text, email);
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }
        private async Task<JToken> GetProviderInfo(string url)
        {
            var c = new HttpClient();
            var resp = await c.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return JToken.Parse(await resp.Content.ReadAsStringAsync());
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}