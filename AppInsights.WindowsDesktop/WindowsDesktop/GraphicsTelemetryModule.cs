#if NET461 || NETCOREAPP3_0
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    /// <summary>
    /// Tracks changes to the WPG Rendering Capability Tier
    /// </summary>
    public class GraphicsTelemetryModule : ITelemetryModule, IDisposable
    {
        private readonly object _lockObject = new object();
        private TelemetryClient _telemetryClient;
        private bool _isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsTelemetryModule"/> class.
        /// </summary>
        public GraphicsTelemetryModule()
        {
        }

        /// <inheritdoc cref="ITelemetryModule.Initialize(TelemetryConfiguration)" />
        public void Initialize(TelemetryConfiguration configuration)
        {
            // Core SDK creates 1 instance of a module but calls Initialize multiple times
            if (!this._isInitialized)
            {
                lock (_lockObject)
                {
                    if (!_isInitialized)
                    {
                        _isInitialized = true;
                        _telemetryClient = new TelemetryClient(configuration);
                        System.Windows.Media.RenderCapability.TierChanged += RenderCapability_TierChanged;
                    }
                }
            }
        }
        private void RenderCapability_TierChanged(object sender, EventArgs e)
        {
            _telemetryClient.TrackEvent("WPF Rendering Capability Tier Changed", new Dictionary<string, string> { { "DX Rendering Capability Tier", System.Windows.Media.RenderCapability.Tier.ToString() } });
        }

        /// <summary>
        /// Disposing GraphicsTelemetryModule instance.
        /// </summary>
        public void Dispose()
        {
            System.Windows.Media.RenderCapability.TierChanged -= RenderCapability_TierChanged;
        }
    }
}
#endif
