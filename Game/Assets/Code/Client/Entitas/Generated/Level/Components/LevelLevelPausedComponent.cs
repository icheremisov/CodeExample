//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class LevelContext {

    public LevelEntity levelPausedEntity { get { return GetGroup(LevelMatcher.LevelPaused).GetSingleEntity(); } }

    public bool isLevelPaused {
        get { return levelPausedEntity != null; }
        set {
            var entity = levelPausedEntity;
            if (value != (entity != null)) {
                if (value) {
                    CreateEntity().isLevelPaused = true;
                } else {
                    entity.Destroy();
                }
            }
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class LevelEntity {

    static readonly Client.Entitas.Levels.LevelPausedComponent levelPausedComponent = new Client.Entitas.Levels.LevelPausedComponent();

    public bool isLevelPaused {
        get { return HasComponent(LevelComponentsLookup.LevelPaused); }
        set {
            if (value != isLevelPaused) {
                var index = LevelComponentsLookup.LevelPaused;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : levelPausedComponent;

                    AddComponent(index, component);
                } else {
                    RemoveComponent(index);
                }
            }
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class LevelMatcher {

    static Entitas.IMatcher<LevelEntity> _matcherLevelPaused;

    public static Entitas.IMatcher<LevelEntity> LevelPaused {
        get {
            if (_matcherLevelPaused == null) {
                var matcher = (Entitas.Matcher<LevelEntity>)Entitas.Matcher<LevelEntity>.AllOf(LevelComponentsLookup.LevelPaused);
                matcher.componentNames = LevelComponentsLookup.componentNames;
                _matcherLevelPaused = matcher;
            }

            return _matcherLevelPaused;
        }
    }
}
