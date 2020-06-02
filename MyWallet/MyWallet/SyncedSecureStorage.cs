using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MyWallet
{
    public static class SyncedSecureStorage
    {
        public static async Task<string> GetOrCreateSecureAsync(string key, string value)
        {
            if (IsSimulator())
            {
                if (Preferences.Get(key, null) is string existingValue)
                {
                    return existingValue;
                }
                Preferences.Set(key, value);
                return value;
            }
            if (await SecureStorage.GetAsync(key) is string existingValue2)
            {
                return existingValue2;
            }
            await SecureStorage.SetAsync(key, value);
            return value;
        }

        public static async Task<string> GetAsync(string key)
        {
            return IsSimulator()
                ? Preferences.Get(key, null)
                : await SecureStorage.GetAsync(key);
        }

        private static bool IsSimulator()
        {
            return DeviceInfo.DeviceType == DeviceType.Virtual;
        }
    }
}