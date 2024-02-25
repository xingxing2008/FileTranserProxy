using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace FileTranserProxy
{
    public class FileTransfer
    {
        public sealed class FileCallbackResult : FileResult
        {
            private readonly Func<Stream, ActionContext, Task> _callback;

            public FileCallbackResult(string contentType, Func<Stream, ActionContext, Task> callback)
                : base(contentType)
            {
                _ = callback ?? throw new ArgumentNullException(nameof(callback));
                _callback = callback;
            }

            /// <inheritdoc />
            public override Task ExecuteResultAsync(ActionContext context)
            {
                _ = context ?? throw new ArgumentNullException(nameof(context));
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
                    SetHeadersAndLog(context, result, fileLength: null, enableRangeProcessing: false);
                    return result._callback(context.HttpContext.Response.BodyWriter.AsStream(), context);
                }
            }
        
        }
    }
}
