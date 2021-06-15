# Application Insights for Windows Desktop

This is an update to the original Application Insights for Desktop applications SDK, first blogged about here: http://apmtips.com/blog/2015/08/08/application-insights-for-desktop-applications/.

It goes beyond what the docs outline here https://docs.microsoft.com/en-us/azure/azure-monitor/app/windows-desktop, by ensuring it also works
for .NET Core 3 based desktop apps. With these steps, you don't need to add the `Microsoft.ApplicationInsights.WindowsServer` package,
just this one `AppInsights.WindowsDesktop`.

It adds the following capaiblities (taken from the Windows Server package) to desktop apps

- Persistence Channel 
   `Microsoft.ApplicationInsights.Channel.PersistenceChannel` is a channel optimized for devices and mobile apps and works great in offline scenarios.  It writes events to disk before attempting to send it. Next time app starts - event will be picked up and send to the cloud. Furthermore, if you are running multiple instances of an application - all of them will write to the disk, but only one will be sending data to http endpoint.
- `DeviceTelemetryInitializer`: sets machine name and OEM information
- `FirstChanceExceptionStatisticsTelemetryModule`: reports first chance exceptions
- `DeveloperModeWithDebuggerAttachedTelemetryModule`: sets DebugMode to `true` if in the debugger
- `UnobservedExceptionTelemetryModule`: reports unobserved task exceptions
- `UnhandledExceptionTelemetryModule`: reports unhandled exceptions
- `SessionTelemetryInitializer`: Initializes session id and user id
- `VersionTelemetryInitializer`: Initializes CLR version and Application Version (from `AssemblyInformationalVersion` attribute of the entrypoint)
- `WTSSessionTelemetryInitializer`: Reports Remote Desktop and Remote Control states
- `GraphicsTelemetryInitializer`: Reports various graphics hardware metric like Graphics Card and driver version, supported DirectX Feature Level. Various WPF Rendering settings.
- `WTSSessionTelemetryModule`: Reports changes to the Windows Terminal Services state, like logging on/off, connect/disconnect remote desktop etc.
- `GraphicsTelemetryModule`: Reports changes to the graphics capabilities of the system.


## Usage
Add an `ApplicationInsights.config` file to your main executable project with an action of `CopyIfNewer`. The recommended default is:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationInsights xmlns="http://schemas.microsoft.com/ApplicationInsights/2013/Settings">
  <TelemetryInitializers>
    <Add Type="Microsoft.ApplicationInsights.DependencyCollector.HttpDependenciesParsingTelemetryInitializer, Microsoft.AI.DependencyCollector" />
    <Add Type="Microsoft.ApplicationInsights.WindowsDesktop.DeviceTelemetryInitializer, AppInsights.WindowsDesktop"/>
    <Add Type="Microsoft.ApplicationInsights.WindowsDesktop.SessionTelemetryInitializer, AppInsights.WindowsDesktop"/>
    <Add Type="Microsoft.ApplicationInsights.WindowsDesktop.VersionTelemetryInitializer, AppInsights.WindowsDesktop"/>
    <!--<Add Type="Microsoft.ApplicationInsights.WindowsDesktop.GraphicsTelemetryInitializer, AppInsights.WindowsDesktop"/>-->
    <!--<Add Type="Microsoft.ApplicationInsights.WindowsDesktop.WTSSessionTelemetryInitializer, AppInsights.WindowsDesktop"/>-->
  </TelemetryInitializers>
  <TelemetryModules>
    <Add Type="Microsoft.ApplicationInsights.DependencyCollector.DependencyTrackingTelemetryModule, Microsoft.AI.DependencyCollector" >
      <ExcludeComponentCorrelationHttpHeadersOnDomains>
        <!-- 
        Requests to the following hostnames will not be modified by adding correlation headers.         
        Add entries here to exclude additional hostnames.
        -->
        <Add>core.windows.net</Add>
        <Add>core.chinacloudapi.cn</Add>
        <Add>core.cloudapi.de</Add>
        <Add>core.usgovcloudapi.net</Add>
      </ExcludeComponentCorrelationHttpHeadersOnDomains>
      <IncludeDiagnosticSourceActivities>
        <Add>Microsoft.Azure.EventHubs</Add>
        <Add>Microsoft.Azure.ServiceBus</Add>
      </IncludeDiagnosticSourceActivities>
    </Add>
    <Add Type="Microsoft.ApplicationInsights.WindowsDesktop.DeveloperModeWithDebuggerAttachedTelemetryModule, AppInsights.WindowsDesktop" />
    <Add Type="Microsoft.ApplicationInsights.WindowsDesktop.UnhandledExceptionTelemetryModule, AppInsights.WindowsDesktop"/>
    <Add Type="Microsoft.ApplicationInsights.WindowsDesktop.UnobservedExceptionTelemetryModule, AppInsights.WindowsDesktop" />
    <!--<Add Type="Microsoft.ApplicationInsights.WindowsDesktop.FirstChanceExceptionStatisticsTelemetryModule, AppInsights.WindowsDesktop" />-->
    <!--<Add Type="Microsoft.ApplicationInsights.WindowsDesktop.WTSSessionTelemetryModule, AppInsights.WindowsDesktop" />-->
    <!--<Add Type="Microsoft.ApplicationInsights.WindowsDesktop.GraphicsTelemetryModule, AppInsights.WindowsDesktop" />-->
  </TelemetryModules>  
  <TelemetryProcessors>
    <Add Type="Microsoft.ApplicationInsights.Extensibility.AutocollectedMetricsExtractor, Microsoft.ApplicationInsights"/>
  </TelemetryProcessors>
  <TelemetryChannel Type="Microsoft.ApplicationInsights.Channel.PersistenceChannel, AppInsights.WindowsDesktop"/>
  <ApplicationIdProvider Type="Microsoft.ApplicationInsights.Extensibility.Implementation.ApplicationId.ApplicationInsightsApplicationIdProvider, Microsoft.ApplicationInsights"/>
</ApplicationInsights>
```

You can set your `InstrumentationKey` either in that file or in code via `TelemetryConfiguration.Active.InstrumentationKey`.

See the docs for more details about configuration: https://docs.microsoft.com/en-us/azure/azure-monitor/app/windows-desktop
