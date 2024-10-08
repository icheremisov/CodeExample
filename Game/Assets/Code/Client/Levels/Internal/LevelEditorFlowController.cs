using System.Threading;
using Client.Core;
using Client.Core.GameFlow.Contracts;
using Cysharp.Threading.Tasks;

namespace Client.Levels.Internal
{
    public class LevelEditorFlowController : FlowController, ILevelEditorFlowController
    {
        public UniTask ExitLevelEditor(CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }
    }
}