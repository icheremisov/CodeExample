namespace Client.Levels.View {

	public interface IView {
		void Link(LevelEntity entity, LevelContext context);
		void Unlink(LevelEntity entity);
	}

}