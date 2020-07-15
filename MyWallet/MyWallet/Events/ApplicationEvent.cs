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
        DeleteAllCredentials,
        NotificationUpdated
    }
    public class ApplicationEvent
    {
        public ApplicationEventType Type { get; set; }

    }
}
