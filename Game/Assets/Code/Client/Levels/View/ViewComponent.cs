using Client.Core.Ecs.Components;
using UnityEngine.AddressableAssets;

namespace Client.Levels.View {

	[Level]
	public class ViewPrefabComponent : ValueComponent<AssetReference> { }

	[Level]
	public class ViewComponent : ValueComponent<IView> { }


}