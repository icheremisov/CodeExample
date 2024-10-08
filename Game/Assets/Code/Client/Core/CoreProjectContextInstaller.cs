using Client.Core.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Unity.Installers;
using Zenject;

namespace Client.Core {

	public class CoreProjectContextInstaller : ProjectContextInstaller<CoreProjectContextInstaller> {
		[SerializeField, Required, AssetsOnly] private ClientConfigHolder _clientConfig;
		// [SerializeField, Required, AssetsOnly] private BattleConfigAsset _battleConfig;

		protected override void OnInstallBindings() {
			_clientConfig.BindSelf(Container);
			// Container.BindInterfacesAndSelfTo<BattleConfigAsset>().FromInstance(_battleConfig);

			Container.Settings = new ZenjectSettings(ValidationErrorResponses.Log, displayWarningWhenResolvingDuringInstall: false);
		}

		protected override void OnInitialize() { }

		protected override void OnDispose() { }
	}

}