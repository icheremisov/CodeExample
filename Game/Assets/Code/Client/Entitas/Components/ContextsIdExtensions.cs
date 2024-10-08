using Entitas;

namespace Client.Entitas.Components {

	public static class ContextsIdExtensions {
		public static void SubscribeId(this LevelContext context) {
			context.OnEntityCreated += AddLevelId;
		}

		private static void AddLevelId(IContext context, IEntity entity) {
			(entity as LevelEntity)?.ReplaceId(entity.creationIndex);
		}
	}

}