using System;
using Client.Core;
using Entitas;
using JetBrains.Annotations;

namespace Client.Entitas.Levels.Systems
{
    [UsedImplicitly]
    public class TimeScaleSystem : IInitializeSystem, IDisposable
    {
        private readonly GlobalContext _globalContext;
        private readonly LevelContext _levelContext;

        public TimeScaleSystem(GlobalContext globalContext, LevelContext levelContext)
        {
            _globalContext = globalContext;
            _levelContext = levelContext;

            _levelContext.GetGroup(LevelMatcher.LevelPaused).OnEntityAdded += OnStateChange;
            _levelContext.GetGroup(LevelMatcher.LevelPaused).OnEntityRemoved += OnStateChange;
            _levelContext.GetGroup(LevelMatcher.LevelState).OnEntityAdded += OnStateChange;
        }

        public void Initialize()
        {
#if FEATURE_CHEATS
            ConsoleInstaller.DisableTimeScaling = true;
            ConsoleInstaller.TimeScaleChanged += CheatsInstallerOnTimeScaleChanged;
#endif // FEATURE_CHEATS

            UpdateTimeScale();
        }

        public void Dispose()
        {
#if FEATURE_CHEATS
            ConsoleInstaller.DisableTimeScaling = false;
            ConsoleInstaller.TimeScaleChanged -= CheatsInstallerOnTimeScaleChanged;
#endif // FEATURE_CHEATS

            _levelContext.GetGroup(LevelMatcher.LevelPaused).OnEntityAdded -= OnStateChange;
            _levelContext.GetGroup(LevelMatcher.LevelPaused).OnEntityRemoved -= OnStateChange;
            _levelContext.GetGroup(LevelMatcher.LevelState).OnEntityAdded -= OnStateChange;
        }

        private void OnStateChange(IGroup<LevelEntity> group, LevelEntity entity, int index, IComponent component) =>
            UpdateTimeScale();


        private void CheatsInstallerOnTimeScaleChanged() => UpdateTimeScale();

        private void UpdateTimeScale()
        {
            if (!_levelContext.hasLevelState) return;

            var timeScale = _levelContext.isLevelPaused ? 0 : 1.0f;

#if FEATURE_CHEATS
            timeScale *= ConsoleInstaller.CurrentTimeScale;
#endif // FEATURE_CHEATS

            UnityEngine.Time.timeScale = timeScale;
            _globalContext.ReplaceTimeScale(timeScale);
        }
    }
}