#if NETCOREAPP3_0 || NET461 || NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    /// <summary>
    /// A telemetry context initializer that will gather graphics capabilities information.
    /// </summary> 
    public class GraphicsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly double _featurelevel = 0;
#if !NETFX_CORE
        private readonly bool _softwareRenderingRegistry;
        private readonly string[] _drivers;
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsTelemetryInitializer"/> class.
        /// </summary>
        public GraphicsTelemetryInitializer()
        {
            try { _featurelevel = GetFeatureLevel(); }
            catch { }
#if !NETFX_CORE
            try { _drivers = GetDrivers().ToArray(); }
            catch { _drivers = new string[] { }; }
            var disable = Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Avalon.Graphics", "DisableHWAcceleration", 0);
            _softwareRenderingRegistry = (disable is int value && value == 1);
            System.Windows.Media.RenderCapability.TierChanged += RenderCapability_TierChanged;
#endif
        }

#if !NETFX_CORE
        private void RenderCapability_TierChanged(object sender, EventArgs e)
        {
            //TODO
            //DiagnosticsClient.TrackEvent("WPF Rendering Capability Tier Changed", new Dictionary<string, string> { { "DX Rendering Capability Tier", System.Windows.Media.RenderCapability.Tier.ToString() } });
        }
#endif
        /// <summary>
        /// Populates graphics properties on a telemetry item.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (_featurelevel > 0)
                telemetry.Context.GlobalProperties["DX FeatureLevel"] = (_featurelevel).ToString();
#if !NETFX_CORE
            telemetry.Context.GlobalProperties["DX Max Hardware Texture Size"] = System.Windows.Media.RenderCapability.MaxHardwareTextureSize.ToString();
            telemetry.Context.GlobalProperties["WPF Registry Forced Software Rendering"] = _softwareRenderingRegistry.ToString();
            telemetry.Context.GlobalProperties["WPF Process RenderMode"] = System.Windows.Media.RenderOptions.ProcessRenderMode.ToString();
            telemetry.Context.GlobalProperties["WPF Rendering Capability Tier"] = System.Windows.Media.RenderCapability.Tier.ToString();
            for (int i = 0; i < _drivers.Length; i++)
            {
                telemetry.Context.GlobalProperties[$"Graphics Driver {i+1}"] = _drivers[i];
            }
#endif
        }

#if !NETFX_CORE
        private static IEnumerable<string> GetDrivers()
        {
            using (var objSearcher = new System.Management.ManagementObjectSearcher("Select * from Win32_VideoController"))
            {
                using (var objCollection = objSearcher.Get())
                {
                    foreach (var obj in objCollection)
                    {
                        yield return $"{obj.Properties["Name"].Value}, Version={obj.Properties["DriverVersion"].Value}";
                    }
                }
            }
        }
#endif

        private static double GetFeatureLevel()
        {
            IntPtr deviceOut;
            IntPtr immediateContextOut;
            uint featureLevelRef;
            uint[] levels = {
                    D3D_FEATURE_LEVEL_12_1, D3D_FEATURE_LEVEL_12_0,
                    D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0,
                    D3D_FEATURE_LEVEL_10_1, D3D_FEATURE_LEVEL_10_0,
                    D3D_FEATURE_LEVEL_9_3, D3D_FEATURE_LEVEL_9_2, D3D_FEATURE_LEVEL_9_1
                };
            D3D11CreateDevice(
                IntPtr.Zero,
                1, //Hardware
                IntPtr.Zero, // software renderer module 
                0, //Creation flags
                levels,
                (uint)levels.Length,
                7, //DX11
                out deviceOut,
                out featureLevelRef,
                out immediateContextOut);
            if (deviceOut != null) Marshal.Release(deviceOut);
            if (immediateContextOut != null) Marshal.Release(immediateContextOut);
            if (featureLevelRef <= D3D_FEATURE_LEVEL_9_1) return 9.1;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_9_2) return 9.2;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_9_3) return 9.3;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_10_0) return 10.0;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_10_1) return 10.1;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_11_0) return 11.0;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_11_1) return 11.1;
            if (featureLevelRef <= D3D_FEATURE_LEVEL_12_0) return 12.0;
            return 12.1;
        }

        [DllImport("d3d11.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int D3D11CreateDevice(IntPtr pAdapter, int driverType, IntPtr software, uint flags, uint[] pFeatureLevels, uint featureLevels, uint sdkVersion, [Out] out IntPtr ppDevice, [Out]out uint pFeatureLevel, [Out] out IntPtr ppImmediateContext);

        private const uint D3D_FEATURE_LEVEL_9_1 = 0x9100;
        private const uint D3D_FEATURE_LEVEL_9_2 = 0x9200;
        private const uint D3D_FEATURE_LEVEL_9_3 = 0x9300;
        private const uint D3D_FEATURE_LEVEL_10_0 = 0xa000;
        private const uint D3D_FEATURE_LEVEL_10_1 = 0xa100;
        private const uint D3D_FEATURE_LEVEL_11_0 = 0xb000;
        private const uint D3D_FEATURE_LEVEL_11_1 = 0xb100;
        private const uint D3D_FEATURE_LEVEL_12_0 = 0xc000;
        private const uint D3D_FEATURE_LEVEL_12_1 = 0xc100;
        private const uint D3D_DRIVER_TYPE_NULL = 3;
    }
}
#endif
