using Client.Core.Ecs.Components;
using Entitas.CodeGeneration.Attributes;
using XLib.Configs.Contracts;

namespace Client.Entitas.Global {

	[Level, Unique]
	public class GameDatabaseComponent : ValueComponent<IGameDatabase> {}
	

}
