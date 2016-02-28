﻿using Microsoft.WindowsAzure.Mobile.Service;
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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class CameraSnapshotController : ApiController
    {
        public ApiServices Services { get; set; }

        [HttpGet]
        public Task<HttpResponseMessage> Get(string hub, string node)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
                {
                    try
                    {
                        CameraActivator.Trigger(hub, node);
                        using (var videoHub = VideoHub.Get(hub, node))
                        {
                            using (var videoWaitable = new VideoHubWaitable(videoHub))
                            {
                                var moreData = true;
                                var imageSize = 0;
                                while (moreData)
                                {
                                    var videoData = await videoWaitable.WaitData();

                                    if (videoData.Length != 0 &&
                                            !(videoData.Length == 1 && videoData.Bytes[0] == 0))
                                    {
                                        await outputStream.WriteAsync(videoData.Bytes, 0, videoData.Length);
                                        imageSize += videoData.Length;
                                    }
                                    else
                                    {
                                        moreData = false;
                                    }
                                }
                                Services.Log.Info(string.Format("Sent camera snapshot with {0} bytes", imageSize));
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
                    }
                }, new MediaTypeHeaderValue("image/jpeg"))
            };

            return Task.FromResult(response);
        }

        private class CameraActivator
        {
            private static Dictionary<string, CameraActivator> _activators = new Dictionary<string, CameraActivator>();
            public static void Trigger(string hub, string node)
            {
                var id = Id(hub, node);
                if (!_activators.ContainsKey(id))
                {
                    _activators[id] = new CameraActivator(hub, node);
                }
                _activators[id].Trigger();
            }
            private static string Id(string hub, string node)
            {
                return string.Format("{0}|{1}", hub, node);
            }

            private ChatHub _chatHub;
            private string _hub;
            private string _node;
            private Deactivator _deactivator;
            protected CameraActivator(string hub, string node)
            {
                _chatHub = ChatHub.Get(hub);
                _hub = hub;
                _node = node;

                _chatHub.MessageToHome(new HubCameraCommand
                {
                    Node = _node,
                    Active = true
                });
            }

            public void Trigger()
            {
                if (_deactivator != null)
                {
                    _deactivator.Cancelled = true;
                }
                _deactivator = new Deactivator(() =>
                {
                    _activators.Remove(Id(_hub, _node));
                    _chatHub.MessageToHome(new HubCameraCommand
                    {
                        Node = _node,
                        Active = false
                    });
                });
            }

            private class Deactivator
            {
                public bool Cancelled { get; set; }
                public Deactivator(Action callback)
                {
                    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(5000);
                        if (!Cancelled)
                        {
                            callback();
                        }
                    })).Start();
                }
            }
        }

    }
}