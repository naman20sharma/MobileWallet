using System.Threading.Tasks;
using Hyperledger.Aries.Agents;

namespace MyWallet.Framework.Services
{
    public interface IMessageDecodeService
    {
        Task<AgentMessage> ParseMessageAsync(string value);
    }
}