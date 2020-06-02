namespace MyWallet.Events
{

    public enum ApplicationEventType
    {
        ConnectionsUpdated,
        CredentialsUpdated,
        GotCredentialOffer,
        GotProofRequestMessage,
        CredentialRemoved
    }
    public class ApplicationEvent
    {
        public ApplicationEventType Type { get; set; }

    }
}
