using System;
using System.Collections.Generic;
using System.Text;

namespace MyWallet.ViewModels.Notification
{
    public class ProofCredentialMapping
    {
        public Dictionary<string, string> proofAndCredentialAttributesMapping { get; set; } = new Dictionary<string, string>();
        public KeyValuePair<string, string> RequestedAttributes { get; set; } = new KeyValuePair<string, string>();

        public KeyValuePair<string, string> CredentialsAttributes { get; set; } = new KeyValuePair<string, string>();
        public ProofCredentialMapping()
        {
            
        }
    } 
}
