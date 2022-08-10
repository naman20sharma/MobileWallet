namespace MyWallet.Events
{

    public enum ApplicationEventType
    {
        ConnectionsUpdated,
        CredentialsUpdated,
        GotCredentialOffer,
        GotProofRequestMessage,
        CredentialRemoved,
        ConnectionRemoved,
        DeleteAllCredentials
    }
    public class ApplicationEvent
    {
        public ApplicationEventType Type { get; set; }

    }
}
