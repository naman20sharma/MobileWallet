using System;

using Xamarin.Forms;

namespace MyWallet.Framework.Services
{
    public static class Constants
    {
        public const string IOSAppCenterSecret = "";
        public const string AndroidAppCenterSecret = "";
        
        public const string LocalWalletProvisioned = "LocalWalletProvisioned";

        public const string LocalWalletIdKey = "LocalWalletIdKey";
        public const string LocalWalletCredentialKey = "LocalWalletCredentialKey";
        public const string RemoteWalletIdKey = "RemoteWalletIdKey";

        public  const string PoolName = "DefaultPool";

        public const string PushNotificationsAllowed = "PushNotificationsAllowed";
        public const string TermsAndConditionsAgreed = "TermsAndConditionsAgreed";

        public const string MediatorTag = "MediatorConnectionId";

        #region UI
        public const string OpenDrawer = "OpenDrawer";
        public const string CloseDrawer = "CloseDrawer";
        #endregion

        #region Configuration
        public const string PoolConfigurationName = "PoolConfigurationName";
        #endregion
    }
}

