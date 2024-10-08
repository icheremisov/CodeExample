using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Core.GameFlow.Contracts
{
    /// <summary>
    ///     entry point for meta game
    /// </summary>
    public interface IMetaFlowController
    {
        UniTask EnterMeta(CancellationToken ct = default);
        UniTask ExitMeta(CancellationToken ct = default);
    }
}