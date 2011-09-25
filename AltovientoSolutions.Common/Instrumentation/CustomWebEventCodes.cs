﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    public enum CustomWebEventCodes
    {
        PasswordChangedEvent = WebEventCodes.WebExtendedBase + 1,
        PasswordResetEvent = WebEventCodes.WebExtendedBase + 2,

        InvitationEmailEvent = WebEventCodes.WebExtendedBase + 100
    }
}
