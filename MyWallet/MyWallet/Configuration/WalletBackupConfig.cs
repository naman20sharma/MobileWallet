using System;
using System.Collections.Generic;
using System.Text;

namespace MyWallet.Configuration
{
    public class WalletBackupConfig : IWalletBackupConfig
    {
        public string path { get; set; }
        public string key { get; set; }
    }

    public interface IWalletBackupConfig
    {
        /// <summary>
        /// Path of the file that contains exported/imported wallet content
        /// </summary>
        string path { get; set; }
        /// <summary>
        /// passphrase used to derive export key
        /// </summary>
        string key { get; set; }
    }
}
