using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MyHomeSecureWeb.Utilities
{
    public class GoogleDriveUploader : IDisposable
    {
        private IGoogleDriveHelper _driveHelper = new GoogleDriveHelper();
        private IGoogleDriveAuthorization _driveAuth = new GoogleDriveAuthorization();

        private const string _uploadFileAddress = "https://www.googleapis.com/upload/drive/v2/files?uploadType=multipart";

        private const string _multipartBoundary = "multipart-file-boundary";
        private const string _multipartTemplate = @"
--multipart-file-boundary
Content-Type: application/json; charset=UTF-8

{0}

--multipart-file-boundary
Content-Type: image/jpeg

";
        private const string _multipartTemplateEnd = @"
--multipart-file-boundary--";

        public async Task UploadFile(string emailAddress, string folderPath, string fileName, byte[] byteArray)
        {
            string accessToken = _driveAuth.GetAccessToken(emailAddress);
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    await UploadWithAccessToken(accessToken, folderPath, fileName, byteArray);
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
//                        services.Log.Info(string.Format("Renewing Drive access token for {0}", emailAddress));
                        accessToken = await _driveAuth.RefreshAccessToken(emailAddress);

                        try
                        {
                            // Try again
                            await UploadWithAccessToken(accessToken, folderPath, fileName, byteArray);
                        }
                        catch(Exception e)
                        {
//                            services.Log.Error("Error uploading file to Drive", e);
                        }
                    }
                    else
                    {
//                        services.Log.Error("Error uploading file to Drive", ex);
                    }
                }
                catch(Exception e)
                {
//                    services.Log.Error("Error uploading file to Drive", e);
                }
            }
        }

        private async Task UploadWithAccessToken(string accessToken, string folderPath, string fileName, byte[] jpegData)
        {
            var parentId = await _driveHelper.GetFolderId(accessToken, folderPath);

            var metadata = new GoogleFileMetadata {
                Title = fileName,
                Parents = new GoogleFileParent[] {
                    new GoogleFileParent { Id = parentId }
                }
            };
            var byteArray = GetMultiPartUpload(metadata, jpegData);

            // Get the response
            var request = WebRequest.Create(_uploadFileAddress);
            request.ContentType = string.Format("multipart/related; boundary={0}", _multipartBoundary);
            request.ContentLength = byteArray.Length;
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);
            request.Method = "POST";

            using (var writeStream = await request.GetRequestStreamAsync())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            var response = await request.GetResponseAsync();

            string responseContent = null;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }
        }
        
        private byte[] GetMultiPartUpload(GoogleFileMetadata metadata, byte[] jpegData)
        {
            var formData = string.Format(_multipartTemplate,
                JsonConvert.SerializeObject(metadata));
            var formBytes = Encoding.UTF8.GetBytes(formData);
            var endBytes = Encoding.UTF8.GetBytes(_multipartTemplateEnd);

            var allBytes = new byte[formBytes.Length + jpegData.Length + endBytes.Length];

            formBytes.CopyTo(allBytes, 0);
            jpegData.CopyTo(allBytes, formBytes.Length);
            endBytes.CopyTo(allBytes, formBytes.Length + jpegData.Length);

            return allBytes;
        }

        public void Dispose()
        {
            _driveAuth.Dispose();
        }
    }
}
