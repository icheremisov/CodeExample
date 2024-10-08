using Client.Core.Ecs.Components;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Client.Entitas.Global.Time {
	
	[Global, Unique]
	public class TimeComponent : IComponent {
		public float Delta;
		public float Time;
	}

	[Global, Unique, Event(EventTarget.Self)]
	public class TimeScaleComponent : ValueComponent<float> { }

}