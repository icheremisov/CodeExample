using System;
using Client.Levels.View.Factory;
using Entitas;

namespace Client.Levels.View {

	public class ViewSystem : IInitializeSystem, IDisposable {
		private readonly LevelContext _context;
		private readonly ILevelViewFactory _levelViewFactory;

		public ViewSystem(LevelContext context, ILevelViewFactory levelViewFactory) {
			_context = context;
			_levelViewFactory = levelViewFactory;
		}

		void IInitializeSystem.Initialize() {
			var viewGroup = _context.GetGroup(LevelMatcher.ViewPrefab);
			viewGroup.OnEntityAdded += OnViewCreate;
			viewGroup.OnEntityRemoved += OnViewDestroy;
		}

		void IDisposable.Dispose() {
			var viewGroup = _context.GetGroup(LevelMatcher.ViewPrefab);
			if (viewGroup == null) return;
			viewGroup.OnEntityAdded -= OnViewCreate;
			viewGroup.OnEntityRemoved -= OnViewDestroy;
		}
		
		private void OnViewCreate(IGroup<LevelEntity> group, LevelEntity entity, int index, IComponent component) {
			var view = _levelViewFactory.CreateView((ViewPrefabComponent)component);
			// if (view is Character character) _animationActorsStorage.Register(entity.instanceId.Value, character.Actor);
			view.Link(entity, _context);
			entity.ReplaceView(view);
		}

		private void OnViewDestroy(IGroup<LevelEntity> group, LevelEntity entity, int index, IComponent component) {
			if (!entity.hasView) return;
			var view = ~entity.view;
			
			view.Unlink(entity);
			_levelViewFactory.DestroyView(view);
			entity.RemoveView();

			// if (entity.isEnabled) _animationActorsStorage.Unregister(entity.instanceId.Value);
		}
	}

}