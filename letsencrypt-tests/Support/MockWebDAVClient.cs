using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebDAVClient.Model;

namespace letsencrypt_tests.Support
{
    public class MockWebDAVClient : WebDAVClient.IClient
    {
        public MockWebDAVClient(NetworkCredential credential) { }

        public string BasePath { get; set; }

        public int? Port { get; set; }

        public string Server { get; set; }

        public string UserAgent { get; set; }

        public string UserAgentVersion { get; set; }

        public Task<bool> CreateDir(string remotePath, string name)
        {
            return Task.FromResult(true);
        }

        public Task DeleteFile(string path = "/")
        {
            return Task.FromResult(true);
        }

        public Task DeleteFolder(string path = "/")
        {
            return Task.FromResult(true);
        }

        public Task<Stream> Download(string remoteFilePath)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetFile(string path = "/")
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetFolder(string path = "/")
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Item>> List(string path = "/", int? depth = 1)
        {
            IEnumerable<Item> list = new List<Item>();
            return Task.FromResult(list);
        }

        public Task<bool> MoveFile(string srcFilePath, string dstFilePath)
        {
            return Task.FromResult(true);
        }

        public Task<bool> MoveFolder(string srcFolderPath, string dstFolderPath)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Upload(string remoteFilePath, Stream content, string name)
        {
            return Task.FromResult(true);
        }
    }
}
