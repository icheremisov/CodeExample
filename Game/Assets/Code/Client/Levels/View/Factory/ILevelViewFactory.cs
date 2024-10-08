using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Client.Levels.View.Factory {

	public interface ILevelViewFactory {

		IView CreateView(AssetReference reference);
		void DestroyView(IView view);
		UniTask InitializeAsync();

	}

}