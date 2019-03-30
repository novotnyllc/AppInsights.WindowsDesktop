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
    /// Telemetry initializer that adds the CLR version, Application Version, and WPF version (if available).
    /// </summary>
    public class VersionTelemetryInitializer : ITelemetryInitializer
    {
#if NETCOREAPP3_0 || NET461
        private readonly string _wpfVersion;
#endif
        private readonly string _clrVersion;
        private readonly string _appVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionTelemetryInitializer"/> class.
        /// </summary>
        public VersionTelemetryInitializer()
        {
#if NETCOREAPP3_0 || NET461
            _wpfVersion = typeof(System.Windows.Application).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
#endif
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

#if NETCOREAPP3_0 || NET461
            if (_wpfVersion != null)
            {
                telemetry.Context.GlobalProperties["WPF version"] = _wpfVersion;
            }
#endif
            
        }
    }

}
