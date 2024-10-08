using Cysharp.Threading.Tasks;

namespace Client.Levels.Internal
{
    public interface IAssetPreloader {
        UniTask PreloadAsync();
    }
}