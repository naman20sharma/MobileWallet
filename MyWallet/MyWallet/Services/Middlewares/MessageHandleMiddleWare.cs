using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Models.Records;
using Hyperledger.Aries.Storage;
using Plugin.LocalNotification;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Runtime;
using MyWallet.Events;
using System.Runtime.InteropServices;

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

        private string GetMessageContent(string type)
        {
            //Check type to show message:
            switch (type)
            {
                case "Hello":
                    return "Hello world!";
                default:
                    return "You've got a message from mediator";
            }
        }

        private string HandleMessageTypeEventAndNotification(string type)
        {
            switch(type)
            {
                case MessageTypes.IssueCredentialNames.OfferCredential:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.GotCredentialOffer });
                        return "You've got an Credential Offer";
                    }
                case MessageTypes.PresentProofNames.RequestPresentation:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.GotProofRequestMessage });
                        return "You've got an Proof Request";
                    }
                case MessageTypes.IssueCredentialNames.IssueCredential:
                    {
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                        return "Accept credential successfully";
                    }
                default:
                    return "You've got a message from mediator";
            }
        }

        public async Task OnMessageAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            await _credentialService.ListAsync(agentContext);           
            var messageType = messageContext.GetMessageType();
            //var content = GetMessageContent(messageType);
            var content = HandleMessageTypeEventAndNotification(messageType);
            var notification = new NotificationRequest
            {
                NotificationId = 100,
                Title = "Test",
                Description = content,
                ReturningData = "Dummy data", // Returning data when tapped on notification.
            };
            NotificationCenter.Current.Show(notification);
            Console.WriteLine($"thinhnnd - Message Type: {messageContext.GetMessageType()}");
        }
    }
}