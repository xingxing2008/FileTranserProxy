using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net.Http.Headers;
using static FileTranserProxy.FileTransfer;

namespace FileTranserProxy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileTransferController : ControllerBase
    {
        private static HttpClient Client { get; } = new HttpClient();
        [HttpPost]
        public IActionResult Post([FromBody][FromForm] string body)
        {
            var filenamesAndUrls = JsonConvert.DeserializeObject <Dictionary<string,string>>(body);

            return new FileCallbackResult(new MediaTypeHeaderValue("application/octet-stream"), async (outputStream, _) =>
            {
                using (var zipArchive = new ZipArchive(new WriteOnlyStreamWrapper(outputStream), ZipArchiveMode.Create))
                {
                    foreach (var kvp in filenamesAndUrls)
                    {
                        var zipEntry = zipArchive.CreateEntry(kvp.Key);
                        using (var zipStream = zipEntry.Open())
                        using (var stream = await Client.GetStreamAsync(kvp.Value))
                            await stream.CopyToAsync(zipStream);
                    }
                }
            })
            {
                FileDownloadName = "Res.zip"
            };
        }
    }
}
