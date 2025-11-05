using Serilog.Core;
using Serilog.Events;

namespace HubManager.Logging
{
    public class ClassNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextProperty))
            {
                var sourceContext = sourceContextProperty.ToString().Trim('"');
                var className = sourceContext.Split('.').Last();

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ClassName", className));
            }
        }
    }
}