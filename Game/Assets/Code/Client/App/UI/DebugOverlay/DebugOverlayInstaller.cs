using Client.Meta.UI.DebugOverlay;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Installers;

namespace Client.App.UI.DebugOverlay {

	public class DebugOverlayInstaller : ProjectContextInstaller<DebugOverlayInstaller> {
		[SerializeField, Required] private DebugOverlayView _debugOverlayView;
		
		protected override void OnInstallBindings() {
			Container.BindInterfacesTo<DebugOverlayView>().FromInstance(_debugOverlayView).AsSingle().NonLazy();
		}

		protected override void OnInitialize() {
		}

		protected override void OnDispose() { }
	}

}