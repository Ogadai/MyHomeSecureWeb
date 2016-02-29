using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class UploadStreamController : ApiController
    {
        public ApiServices Services { get; set; }

        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        // POST: api/AwayStatus
        [HttpPost]
        public async Task<IHttpActionResult> UploadStream([FromUri]string hub, [FromUri]string token, [FromUri]string node)
        {
            // Validate access to this hub
            var homeHub = _homeHubRepository.GetHub(hub);
            if (homeHub == null)
            {
                return NotFound();
            }

            var tokenHash = _passwordHash.Hash(token, homeHub.TokenSalt);
            if (!tokenHash.SequenceEqual(homeHub.TokenHash))
            {
                Services.Log.Error(string.Format("Invalid Token - {0}", token), null, "UploadSnapshot");
                return Unauthorized();
            }

            // Hand over the uploaded file data
            using (var videoHub = VideoHub.Get(homeHub.Id, node))
            {
                var requestStream = new RequestStream(homeHub.Id, node);
                await Request.Content.CopyToAsync(requestStream);
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            _homeHubRepository.Dispose();
            base.Dispose(disposing);
        }

        private class RequestStream : Stream
        {
            private VideoHub _videoHub;
            public RequestStream(string hubId, string node)
            {
                _videoHub = VideoHub.Get(hubId, node);
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (offset != 0)
                {
                    var copy = buffer.Skip(offset).ToArray();
                    _videoHub.ReceivedData(copy, count);
                }
                else
                {
                    _videoHub.ReceivedData(buffer, count);
                }
            }

            public override void Close()
            {
                base.Close();
                if (_videoHub != null)
                {
                    _videoHub.Dispose();
                    _videoHub = null;
                }
            }
        }
    }
}
