#if WINDOWS || NETFX_CORE
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    /// <summary>
    /// Tracks the Remote Desktop and Remote Control state of the application
    /// </summary>
    public class WTSSessionTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WTSSessionTelemetryInitializer"/> class.
        /// </summary>
        public WTSSessionTelemetryInitializer()
        {
        }

        /// <inheritdoc cref="ITelemetryInitializer.Initialize(ITelemetry)" />
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["Remote Desktop Session"] = WTSSessionHelper.IsRemoteDesktop.ToString();
#if !NETFX_CORE
            telemetry.Context.GlobalProperties["Remote Control Session"] = WTSSessionHelper.IsRemoteControlled.ToString();
#endif
        }
    }
}
#endif
