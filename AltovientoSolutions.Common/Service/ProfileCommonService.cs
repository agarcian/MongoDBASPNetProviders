using System;
namespace AltovientoSolutions.Common.Services
{
    public interface IProfileCommonService
    {
        AltovientoSolutions.Common.Services.IProfileCommon Create(string username);
        AltovientoSolutions.Common.Services.IProfileCommon GetProfile(string username);
    }

    public class ProfileCommonService : IProfileCommonService
    {
        public AltovientoSolutions.Common.Services.IProfileCommon Create(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            return AltovientoSolutions.Common.Services.ProfileCommon.Create(username);

        }

        public AltovientoSolutions.Common.Services.IProfileCommon GetProfile(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            return AltovientoSolutions.Common.Services.ProfileCommon.GetProfile(username);
        }


    }
    
}




