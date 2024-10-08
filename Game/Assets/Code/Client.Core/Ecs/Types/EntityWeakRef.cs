using Entitas;

namespace Client.Core.Ecs.Types
{
	/// <summary>
	/// hold weak reference to entity
	/// </summary>
	public class EntityWeakRef
	{
		private Entity _value;
		
		public Entity Value
		{
			get => _value;
			set
			{
				Unbind(_value);
				_value = value;
				Bind(_value);
			}
		}

		public EntityWeakRef()
		{
		}
		
		public EntityWeakRef(Entity value)
		{
			Value = value;
		}

		public void Clear()
		{
			Value = null;
		}

		public static implicit operator Entity(EntityWeakRef obj) 
		{
			return obj?.Value;
		}		
		
		private void Bind(IEntity value)
		{
			if (value == null)
			{
				return;
			}
			
			value.OnDestroyEntity += ValueOnOnDestroyEntity;
		}

		private void Unbind(IEntity value)
		{
			if (value == null)
			{
				return;
			}

			value.OnDestroyEntity -= ValueOnOnDestroyEntity;
		}

		private void ValueOnOnDestroyEntity(IEntity entity)
		{
			Unbind(entity);
			
			if (_value == entity)
			{
				_value = null;
			}
		}
	}
}