using Client.Core.Ecs.Components;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Client.Entitas.Components {

	[Level, Event(EventTarget.Self)]
	public class PositionComponent : ValueComponent<Vector3> {}
	
	[Level]
	public class PositionFromComponent : ValueComponent<Vector3> {}
	
	[Level]
	public class PositionToComponent : ValueComponent<Vector3> {}
	
	[Level, Event(EventTarget.Self)]
	public class RotationComponent : ValueComponent<Vector3> {}
	
	[Level, Event(EventTarget.Self)]
	public class ScaleComponent : ValueComponent<Vector3> {}
	
}