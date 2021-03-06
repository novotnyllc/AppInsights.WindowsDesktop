﻿#if WINDOWS
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    /// <summary>
    /// Tracks changes to the Windows Terminal Services session, like Remote Desktop, Lock/Unlock etc.
    /// </summary>
    public class WTSSessionTelemetryModule : ITelemetryModule, IDisposable
    {
        private readonly object _lockObject = new object();
        private WTSSessionHelper _helper;
        private TelemetryClient _telemetryClient;
        private bool _isInitialized = false;
        private Action _disposeActivationListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="WTSSessionTelemetryModule"/> class.
        /// </summary>
        public WTSSessionTelemetryModule() : this(Application.Current.MainWindow)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WTSSessionTelemetryModule"/> class.
        /// </summary>
        /// <param name="window">The Window to track WTS state for</param>
        public WTSSessionTelemetryModule(Window window)
        {
            if (window != null)
            {
                InitializeHelper(window);
            }
            else
            {
                var app = Application.Current;
                if (app != null)
                {

                    _disposeActivationListener = () => { app.Activated -= Application_Activated; _disposeActivationListener = null; };
                    app.Activated += Application_Activated;
                }
            }
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            var window = Application.Current?.MainWindow;
            if (window != null)
            {
                _disposeActivationListener?.Invoke();
                InitializeHelper(window);
            }
        }

        private void InitializeHelper(Window window)
        {
            _helper = new WTSSessionHelper(window);
            _helper.SessionStateChanged += WTS_SessionStateChanged;
            _helper.RemoteControlChanged += WTS_RemoteControlChanged;
            _helper.RemoteDesktopChanged += WTS_RemoteDesktopChanged;
        }

        /// <inheritdoc cref="ITelemetryModule.Initialize(TelemetryConfiguration)" />
        public void Initialize(TelemetryConfiguration configuration)
        {
            if (!this._isInitialized)
            {
                lock (_lockObject)
                {
                    if (!_isInitialized)
                    {
                        _isInitialized = true;
                        _telemetryClient = new TelemetryClient(configuration);
                    }
                }
            }
        }

        private void WTS_RemoteDesktopChanged(object sender, bool isRemoteDesktop)
        {
            _telemetryClient?.TrackEvent("WTS Remote Desktop Session", new Dictionary<string, string>() { { "Remote Desktop", isRemoteDesktop.ToString() } });
        }

        private void WTS_RemoteControlChanged(object sender, bool isRemoteControl)
        {
            _telemetryClient?.TrackEvent("WTS Remote Control Session", new Dictionary<string, string>() { { "Remote Controlled", isRemoteControl.ToString() } });
        }

        private void WTS_SessionStateChanged(object sender, WTSSessionHelper.WTSSessionState state)
        {
            _telemetryClient?.TrackEvent("WTS Session State Changed", new Dictionary<string, string>() { { "New State", state.ToString() } });
        }

        /// <summary>
        /// Disposing WTSSessionTelemetryModule instance.
        /// </summary>
        public void Dispose()
        {
            _disposeActivationListener?.Invoke();
            _helper?.Dispose();
        }
    }
}
#endif
