using Client.Meta.Internal;
using UnityEditor;
using XLib.UI.Controls;
using XLib.Unity.Installers;
using XLib.Unity.Installers.Attributes;

namespace Client.Meta
{
    [InstallerContainer(InstallerContainer.Main)]
    public class MetaGameInstaller : SceneInstaller<MetaGameInstaller> {
        protected override void OnInstallBindings() {

            Container.BindInterfacesTo<WorldRayCasterManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayMetaState>().AsSingle();
        }

        protected override void OnInitialize() { }

        protected override void OnDispose() { }
    }

}