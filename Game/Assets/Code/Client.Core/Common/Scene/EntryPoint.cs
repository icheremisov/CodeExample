using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Client.Core.Common.Scene {

	/// <summary>
	///     provides entry point for Application
	/// </summary>
	public class EntryPoint : MonoBehaviour {
		
		private void Awake() {
			AppLogger.Log($"{nameof(EntryPoint)}: Awake");

			StartGame().Forget();
		}

		private async UniTask StartGame() {
			
			AppLogger.Log($"{nameof(EntryPoint)}: StartGame");
			
			// skip frame for Initialize scene fully loaded
			await UniTask.NextFrame();
			
			var ctx = ProjectContext.Instance;
			
			// skip frame for initializing ProjectContext's installers and bindings
			await UniTask.NextFrame();
			
			// enter game
			ctx.Container.Resolve<IApplicationLoader>().Play().Forget();

			AppLogger.Log($"{nameof(EntryPoint)}: StartGame OK");
		}
	}

}