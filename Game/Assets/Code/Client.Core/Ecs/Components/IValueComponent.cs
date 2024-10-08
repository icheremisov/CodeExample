using Entitas;

namespace Client.Core.Ecs.Components
{
	public interface IValueComponent<T>: IComponent
	{
		T Value { get; set; }
	}
}