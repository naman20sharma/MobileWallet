using Hyperledger.Aries.Features.IssueCredential;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyWallet.ViewModels.Notification
{
    public class ProofRequestAndCredentialMap
    {
        public string ProofKey { get; set; }
        //public CredentialInfo CredentialInfo
        public KeyValuePair<string, string> CredentialAttribute { get; set; } = new KeyValuePair<string, string>();
        public string Referent { get; set; }

        public ProofRequestAndCredentialMap()
        {

        }

        public ProofRequestAndCredentialMap(string proofKey, string referent, KeyValuePair<string, string> credentialAttribute)
        {
            ProofKey = proofKey;
            CredentialAttribute = credentialAttribute;
            Referent = referent;
        }
    }
}
