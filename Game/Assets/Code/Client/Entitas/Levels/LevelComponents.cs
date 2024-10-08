using System;
using Client.Core.Ecs.Components;
using Client.Definitions;
using Client.Levels.Contracts;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Client.Entitas.Levels
{
    [Level, Unique, Event(EventTarget.Self)]
    public class LevelStateComponent : ValueComponent<ClientLevelState> { }

    [Level, Unique, Event(EventTarget.Self)]
    public class DesiredLevelStateComponent : ValueComponent<ClientLevelState> { }

    [Level, Unique]
    public class LevelStateChangeComponent : ValueComponent<Func<ClientLevelState, ClientLevelState>> { }
    
    [Level, Unique]
    public class LevelDefinitionComponent : ValueComponent<LevelDefinition> { }
    
    [Level, Unique]
    public class LevelPausedComponent : IComponent { }


}