using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class CameraStreamController : ApiController
    {
        public ApiServices Services { get; set; }

        [HttpGet]
        public Task<HttpResponseMessage> Get(string node)
        {
            string hubId = "2fc81f9e-7ba6-4869-8346-6130942f1c7a"; // "74d1192d-d369-465b-a17b-99f47c28c58d";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
                {
                    Services.Log.Info(string.Format("Requesting camera stream from {0} - {1}", node, hubId));
                    using (var chatHub = ChatHub.Get(hubId))
                    {
                        chatHub.MessageToHome(new HubCameraCommand
                        {
                            Node = node,
                            Active = true
                        });

                        try
                        {
                            using (var videoHub = VideoHub.Get(hubId, node))
                            {
                                using (var videoWaitable = new VideoHubWaitable(videoHub))
                                {
                                    var moreData = true;
                                    while (moreData)
                                    {
                                        var videoData = await videoWaitable.WaitData();

                                        if (videoData.Length != 0)
                                        {
                                            await outputStream.WriteAsync(videoData.Bytes, 0, videoData.Length);
                                        }
                                        else
                                        {
                                            moreData = false;
                                        }
                                    }
                                }
                            }
                        }
                        catch (HttpException ex)
                        {
                            if (ex.ErrorCode == -2147023667) // The remote host closed the connection. 
                            {
                                return;
                            }
                        }
                        finally
                        {
                            // Close output stream as we are done
                            outputStream.Close();

                            chatHub.MessageToHome(new HubCameraCommand
                            {
                                Node = node,
                                Active = false
                            });
                        }
                    }
                }, new MediaTypeHeaderValue("video/webm")) // "text/plain"
            };

            return Task.FromResult(response);
        }
    }
}
