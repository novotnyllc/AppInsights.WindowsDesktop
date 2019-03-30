using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.ApplicationInsights.WindowsDesktop
{
    /// <summary>
    /// A telemetry context initializer that will gather session context information (Session and User Id).
    /// </summary> 
    public class SessionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _userName;
        private readonly string _session = Guid.NewGuid().ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTelemetryInitializer"/> class.
        /// </summary>
        public SessionTelemetryInitializer()
        {
            try
            {
                using (var hash = SHA256.Create())
                {
                    var hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName + Environment.UserDomainName + Environment.UserName));
                    _userName = Convert.ToBase64String(hashBytes);
                }
            }
            catch
            {
                // No user id                
            }
        }

        /// <summary>
        /// Populates session id/user id properties on a telemetry item.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            if (_userName != null)
            {
                telemetry.Context.User.Id = _userName;
            }

            telemetry.Context.Session.Id = _session;         
        }
    }
}
