using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using MyWallet.Configuration;
using Xamarin.Forms;

namespace MyWallet.Utils
{
    public static class Utils
    {
        public static Color ToColor(this string self)
        {
            return Color.FromHex(self);
        }

        public static void DisposeWith(this IDisposable self, CompositeDisposable container)
        {
            container.Add(self);
        }

        public static string ResolveConnectionId(this RecordBase self)
        {
            switch (self)
            {
                case CredentialRecord cr:
                    return cr.ConnectionId;
                case ProofRecord pr:
                    return pr.ConnectionId;
            }
            
            throw new InvalidOperationException($"Unable to resolve ConnectionId on type {self.GetType().Name}");
        }

        internal static string GenerateRandomAsync(int maxSize)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            var data = new byte[maxSize];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
            }

            var result = new StringBuilder(maxSize);
            foreach (var b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets configuration from assembly and deserializes into object
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>
        ///     <cref>WallletAppConfiguration</cref>
        /// </returns>
        public static IWalletAppConfiguration GetEmbeddedConfiguration(this Assembly assembly)
        {
            var embeddedResourceStream = assembly.GetManifestResourceStream("MyWallet.Configuration.config.json");
            if (embeddedResourceStream == null)
                throw new InvalidOperationException($"Error reading configuration from assembly {assembly.GetName()}");

            using (var streamReader = new StreamReader(embeddedResourceStream))
            {
                var jsonString = streamReader.ReadToEnd();
                var r = JsonConvert.DeserializeObject<WalletAppConfiguration>(jsonString);
                return r;
            }
        }
    }
}
