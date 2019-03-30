namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.WindowsDesktop.Implementation;
    
    /// <summary>
    /// A telemetry context initializer that will gather device context information.
    /// </summary>
    public class DeviceTelemetryInitializer : ITelemetryInitializer
    {      
        /// <summary>
        /// Populates device properties on a telemetry item.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            if (telemetry.Context != null && telemetry.Context.Device != null)
            {
                var reader = DeviceContextReader.Instance;
                telemetry.Context.Device.Type = reader.GetDeviceType();

#if NET461 || NETCOREAPP3_0
                telemetry.Context.Device.Id = reader.GetDeviceUniqueId();
                telemetry.Context.Device.OemName = reader.GetOemName();
                telemetry.Context.Device.Model = reader.GetDeviceModel();
#endif

                telemetry.Context.GlobalProperties["Network type"] = reader.GetNetworkType();
                telemetry.Context.GlobalProperties["Thread culture"] = reader.GetHostSystemLocale();
                telemetry.Context.GlobalProperties["UI culture"] = reader.GetDisplayLanguage();                
                telemetry.Context.GlobalProperties["Time zone"] = TimeZoneInfo.Local.Id;                
            }
        }
    }
}
