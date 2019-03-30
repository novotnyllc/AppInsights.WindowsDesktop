using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    /// <summary>
    /// Telemetry initializer that adds the CLR version and Application Version.
    /// </summary>
    public class VersionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _clrVersion;
        private readonly string _appVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionTelemetryInitializer"/> class.
        /// </summary>
        public VersionTelemetryInitializer()
        {
            _clrVersion = typeof(string).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            _appVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }

        /// <summary>
        /// Populates device properties on a telemetry item.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (_appVersion != null)
            {
                telemetry.Context.Component.Version = _appVersion;
            }

            if (_clrVersion != null)
            {
                telemetry.Context.GlobalProperties["CLR version"] = _clrVersion;
            }            
        }
    }

}
