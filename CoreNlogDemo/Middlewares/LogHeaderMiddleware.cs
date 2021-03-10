using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreNlogDemo.Middleware
{
    public class LogHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public LogHeaderMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var header = context.Request.Headers["CorrelationId"];
            string sessionId;

            if (header.Count > 0)
                sessionId = header[0];
            else
            {
                sessionId = Guid.NewGuid().ToString();
                context.Request.Headers.Add("CorrelationId", sessionId);
                context.TraceIdentifier = sessionId;
                MappedDiagnosticsLogicalContext.Set("RequestID", sessionId);
            }
            
            header = context.Request.Headers["CorrelationId"];
            if (header.Count > 0)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<LogHeaderMiddleware>>();
                using (logger.BeginScope("{@CorrelationId}", header[0]))
                {
                    await this._next(context);
                }
            }
            else
                await this._next(context);
        }
    }
}
