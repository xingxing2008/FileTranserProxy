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
        [Route("zipArchive")]
        public IActionResult PostZip([FromBody] string body)
        {
            var filenamesAndUrls = JsonConvert.DeserializeObject <Dictionary<string,string>>(body);

            return new FileCallbackResult("application/octet-stream", async (outputStream, _) =>
            {
                using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create,true))
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
        [HttpPost]
        [Route("source")]
        public IActionResult Post([FromBody] string body)
        {
            var filenamesAndUrls = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            var url = filenamesAndUrls.Values.Last();
            var name= filenamesAndUrls.Keys.Last();

            return new FileCallbackResult("application/octet-stream", async (outputStream, _) =>
            {
                using (var httpClient = new HttpClient())
                {
                    using (var stream = await httpClient.GetStreamAsync(url))
                    {
                        await stream.CopyToAsync(outputStream);
                    }
                }
            })
            {
                FileDownloadName = name
            };
        }
    }
}
