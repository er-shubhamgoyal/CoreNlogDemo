using NLog.Config;
using System.Text;

namespace NLog.LayoutRenderers
{
    [LayoutRenderer("requestId")]
    public class RequestDetailsLayoutRenderer : LayoutRenderer
    {
        public RequestDetailsLayoutRenderer()
        {            
        }
       

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(MappedDiagnosticsLogicalContext.Get("RequestID"));
        }
    }
}
