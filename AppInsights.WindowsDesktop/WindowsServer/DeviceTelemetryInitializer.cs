namespace Microsoft.ApplicationInsights.WindowsServer
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.WindowsServer.Implementation;
    
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
                telemetry.Context.Device.Id = reader.GetDeviceUniqueId();
                telemetry.Context.Device.OemName = reader.GetOemName();
                telemetry.Context.Device.Model = reader.GetDeviceModel();
#pragma warning disable CS0618 // Type or member is obsolete
                telemetry.Context.Device.NetworkType = reader.GetNetworkType();
                telemetry.Context.Properties["ai.device.locale"] = reader.GetHostSystemLocale();
                telemetry.Context.Properties["ai.device.language"] = reader.GetDisplayLanguage();                
                telemetry.Context.Properties["ai.location.timeZone"] = TimeZoneInfo.Local.DisplayName;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
