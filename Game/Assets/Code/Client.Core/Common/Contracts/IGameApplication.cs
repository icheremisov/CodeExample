using System.Threading;
using System.Threading.Tasks;

namespace Client.Core.Common.Contracts
{
    public interface IGameApplication
    {
        Task Initialize(CancellationToken ct, SharedVersion version);

        Task<ILogicPlayerContext> CreatePlayerContext(string service, SharedRawData raw);
        
    }
}