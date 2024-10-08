using Client.Core.Ecs.Components;
using XLib.Configs.Contracts;

namespace Client.Entitas.Components {

	[Level]
	public class IdComponent : PrimaryValueComponent<int> { }

	[Level]
	public class InstanceIdComponent : PrimaryValueComponent<InstanceId> { }

}