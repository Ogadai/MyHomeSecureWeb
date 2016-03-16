using MyHomeSecureWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHomeSecureWeb.Utilities
{
    public class GoogleDriveHelper : IGoogleDriveHelper
    {
        private const string _createFolderAddress = "https://www.googleapis.com/drive/v2/files";
        private const string _searchAddressTemplate = "https://www.googleapis.com/drive/v2/files?corpus=DOMAIN&spaces=drive&q={0}";

        private const string _rootFolderName = "HomeSecureStream";

        public async Task<string> GetFolderId(string accessToken, string folderPath, string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                parentId = await GetFolderIdInternal(accessToken, _rootFolderName);
            }

            var pathParts = folderPath.Split('/');
            string folderId = parentId;
            foreach(var folderName in pathParts)
            {
                folderId = await GetFolderIdInternal(accessToken, folderName, folderId);
            }

            return folderId;
        }

        private async Task<string> GetFolderIdInternal(string accessToken, string folderName, string parentId = null)
        { 
            var query = string.Format("title = '{0}'", folderName);
            if (!string.IsNullOrEmpty(parentId))
            {
                query += string.Format(" and '{0}' in parents", parentId);
            }

            var searchResult = await Search(accessToken, query);
            if (searchResult.Items.Length > 0)
            {
                return searchResult.Items[0].Id;
            }
            else
            {
                return await CreateFolder(accessToken, folderName, parentId);
            }
        }

        public async Task<string[]> GetChildrenIDs(string accessToken, string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
            {
                parentId = await GetFolderIdInternal(accessToken, _rootFolderName);
            }

            var query = string.Format("'{0}' in parents", parentId);

            var searchResult = await Search(accessToken, query);

            return searchResult.Items.Select(item => item.Id).ToArray();
        }

        public async Task<GoogleSearchResult> Search(string accessToken, string query)
        {
            var search = string.Format(_searchAddressTemplate, HttpUtility.UrlEncode(query));

            var request = WebRequest.Create(search);
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);

            var response = await request.GetResponseAsync();

            string responseContent = null;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }

            return JsonConvert.DeserializeObject<GoogleSearchResult>(responseContent);
        }

        private async Task<string> CreateFolder(string accessToken, string folderName, string parentId = null)
        {
            var metadata = new GoogleFileMetadata
            {
                Title = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            if (!string.IsNullOrEmpty(parentId))
            {
                metadata.Parents = new GoogleFileParent[] {
                    new GoogleFileParent { Id = parentId }
                };
            }
            var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata));

            // Get the response
            var request = WebRequest.Create(_createFolderAddress);
            request.ContentType = "application/json";
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

            var parent = JsonConvert.DeserializeObject<GoogleFileParent>(responseContent);
            return parent.Id;
        }
    }
}
