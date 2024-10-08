using System.Threading.Tasks;

namespace Client.Core.Common.Contracts
{
    public interface ILogicRawStored
    {
        void FromRawCopy(SharedRawData raw, bool dirty);

        SharedRawData Dump();

        string GetHash();
    }
    
    public interface ILogicPlayerContext : ILogicRawStored
    {
        string Id { get; }

        string Service { get; set; }

        Task<SharedLogicResult> HandleMessage(SharedMessage message);
    }
    public sealed class SharedLogicResult
    {
        public SharedMessage Message { get; }
        public bool ForceUpdate { get; }

        public SharedLogicResult(SharedMessage message, bool forceUpdate)
        {
            Message = message;
            ForceUpdate = forceUpdate;
        }
    }
 
}