using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Utility
{
    // Setup Twilio SMS service - doesn't work
    // 1. Add class
    // 2. Add to appsettings.json - (course account podaci)
    // 3. Configure Service in Startup.cs
    // 4. Install Twilio to BulkyBook project
    // 5. Add to CartController - OrderConfirmation
    public class TwilioSettings
    {
        public string PhoneNumber { get; set; }
        public string AuthToken { get; set; }
        public string AccountSid { get; set; }
    }
}
