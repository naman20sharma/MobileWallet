using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.IssueCredential;
using System;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using Hyperledger.Aries.Contracts;
using MyWallet.Events;
using Hyperledger.Aries.Features.BasicMessage;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace MyWallet.Services.Middlewares
{
    public class MessageHandleMiddleWare : IAgentMiddleware
    {
        private readonly ICredentialService _credentialService;
        private readonly IEventAggregator _eventAggregator;
        public MessageHandleMiddleWare(ICredentialService credentialService,
            IEventAggregator eventAggregator)
        {
            _credentialService = credentialService;
            _eventAggregator = eventAggregator;
        }

        private string HandleMessageTypeEventAndNotification(string type)
        {
            switch (type)
            {
                case MessageTypes.IssueCredentialNames.OfferCredential:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.GotCredentialOffer });
                        return "Credential Offer Received, Tap here to view";
                    }
                case MessageTypes.PresentProofNames.RequestPresentation:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.GotProofRequestMessage });
                        return "Proof Request Received, Tap here to view";
                    }
                case MessageTypes.IssueCredentialNames.IssueCredential:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                        return "Credentials has been issued successfully, Tap here to view";
                    }
                case MessageTypes.BasicMessageType:
                    {
                        return "You have received a message";
                    }

                case MessageTypes.ConnectionResponse:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });
                        return "A new Connection is available, Tap here to view";
                    }
                default:
                    return string.Empty;
            }
        }

        public async Task OnMessageAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
           
            var messageType = messageContext.GetMessageType();
            var content = HandleMessageTypeEventAndNotification(messageType);
            if (content.Equals(string.Empty))
                return;
            var notification = new NotificationRequest
            {
                Android = new AndroidOptions()
                { IconName = "AppNotification"},
                NotificationId = 100,
                Title = "Mediator",
                Description = content,
                ReturningData = content, // Returning data when tapped on notification.
            };
            if (messageType.Equals(MessageTypes.BasicMessageType))
            {
                if ((messageContext.ContextRecord as BasicMessageRecord).Text.Contains("received"))
                    return;
                notification.Description = String.Format($"{content} : {Environment.NewLine} {(messageContext.ContextRecord as BasicMessageRecord).Text}");
            }
            NotificationCenter.Current.Show(notification);
            Console.WriteLine($"Mediator - Message Type: {messageContext.GetMessageType()}");
        }
    }
}