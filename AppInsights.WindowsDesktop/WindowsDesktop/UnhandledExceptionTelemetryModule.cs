namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Common;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.WindowsDesktop.Implementation;

    /// <summary>
    /// The module subscribed to AppDomain.CurrentDomain.UnhandledException to send exceptions to ApplicationInsights.
    /// </summary>
    public sealed class UnhandledExceptionTelemetryModule : ITelemetryModule, IDisposable
    {
        private readonly Action<UnhandledExceptionEventHandler> unregisterAction;
        private readonly Action<UnhandledExceptionEventHandler> registerAction;
        private readonly object lockObject = new object();

        private TelemetryClient telemetryClient;
        private bool isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionTelemetryModule"/> class.
        /// </summary>
        public UnhandledExceptionTelemetryModule() : this(
            action => AppDomain.CurrentDomain.UnhandledException += action,
            action => AppDomain.CurrentDomain.UnhandledException -= action)
        {
        }

        internal UnhandledExceptionTelemetryModule(
            Action<UnhandledExceptionEventHandler> registerAction,
            Action<UnhandledExceptionEventHandler> unregisterAction)
        {
            this.unregisterAction = unregisterAction;
            this.registerAction = registerAction;
        }

        /// <summary>
        /// Initializes the telemetry module.
        /// </summary>
        /// <param name="configuration">Telemetry Configuration used for creating TelemetryClient for sending exceptions to ApplicationInsights.</param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            // Core SDK creates 1 instance of a module but calls Initialize multiple times
            if (!this.isInitialized)
            {
                lock (this.lockObject)
                {
                    if (!this.isInitialized)
                    {
                        this.isInitialized = true;

                        this.telemetryClient = new TelemetryClient(configuration);
                        this.telemetryClient.Context.GetInternalContext().SdkVersion = SdkVersionUtils.GetSdkVersion("unhnd:");

                        this.registerAction(this.CurrentDomainOnUnhandledException);
                    }
                }
            }
        }

        /// <summary>
        /// Disposing UnhandledExceptionTelemetryModule instance.
        /// </summary>
        public void Dispose()
        {
            this.unregisterAction(this.CurrentDomainOnUnhandledException);
        }



        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            WindowsServerEventSource.Log.CurrentDomainOnUnhandledException();
            try
            {

                var exp = new ExceptionTelemetry(unhandledExceptionEventArgs.ExceptionObject as Exception)
                {
                    SeverityLevel = SeverityLevel.Critical,
                };
                exp.Properties.Add("handledAt", "Unhandled");

                telemetryClient.TrackException(exp);
                telemetryClient.Flush(); // we want this to send right away
            }
            catch
            {
                // Nothing we can do here
            }
        }
    }
}
