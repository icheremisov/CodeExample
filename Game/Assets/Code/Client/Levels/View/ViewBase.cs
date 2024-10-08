using UnityEngine;

namespace Client.Levels.View {

	public class ViewBase : MonoBehaviour, IView, IPositionListener, IRotationListener, IScaleListener {
		protected Transform _transform;
		protected LevelContext _levelContext;

		protected virtual void Awake() {
			_transform = transform;
		}

		public virtual void Link(LevelEntity entity, LevelContext context) {
			_levelContext = context;
			
			if (entity.hasPosition) {
				((IPositionListener)this).OnPosition(entity, entity.position);
				entity.AddPositionListener(this);
			}
			if (entity.hasRotation) {
				((IRotationListener)this).OnRotation(entity, entity.rotation);
				entity.AddRotationListener(this);
			}
			if (entity.hasScale) {
				((IRotationListener)this).OnRotation(entity, entity.scale);
				entity.AddScaleListener(this);
			}
		}

		public virtual void Unlink(LevelEntity entity) {
			if (entity.hasPosition) entity.RemovePositionListener(this);
			if (entity.hasRotation) entity.RemoveRotationListener(this);
			if (entity.hasScale) entity.RemoveScaleListener(this);
		}

		void IPositionListener.OnPosition(LevelEntity entity, Vector3 value) => _transform.position = value;

		void IRotationListener.OnRotation(LevelEntity entity, Vector3 value) => _transform.rotation = Quaternion.Euler(value);

		void IScaleListener.OnScale(LevelEntity entity, Vector3 value) => _transform.localScale = value;
	}

}