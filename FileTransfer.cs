using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace FileTranserProxy
{
    public class FileTransfer
    {
        public class FileCallbackResult : FileResult
        {
            private Func<Stream, ActionContext, Task> _callback;

            public FileCallbackResult(MediaTypeHeaderValue contentType, Func<Stream, ActionContext, Task> callback)
                : base(contentType?.ToString())
            {
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback));
                _callback = callback;
            }

            public override Task ExecuteResultAsync(ActionContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));
                var executor = new FileCallbackResultExecutor(context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>());
                return executor.ExecuteAsync(context, this);
            }

            private sealed class FileCallbackResultExecutor : FileResultExecutorBase
            {
                public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
                    : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory))
                {
                }

                public Task ExecuteAsync(ActionContext context, FileCallbackResult result)
                {
                    SetHeadersAndLog(context, result,null,true);
                    return result._callback(context.HttpContext.Response.Body, context);
                }
            }
        }

        public class fileinfo
        {
            public string Name { get; set; }
            public string url { get; set; }
        }
    }
}
