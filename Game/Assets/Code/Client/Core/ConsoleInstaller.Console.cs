#if FEATURE_CONSOLE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Client.Core.DebugLogCollector.Contracts;
using Client.Core.DebugLogCollector.UI;
using CodeStage.AdvancedFPSCounter;
using Coffee.UIExtensions;
using DG.Tweening;
using JetBrains.Annotations;
using LunarConsolePlugin;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Core.Utils;
using XLib.Unity.Core;
using XLib.Unity.Utils;
using Zenject;

namespace Client.Core {

	public partial class ConsoleInstaller : IContainerListener, ITickable {
		[SerializeField, Required] private LunarConsole _lunarConsoleFull;
		[SerializeField, Required, UsedImplicitly] private LunarConsole _lunarConsole;
		[SerializeField, Required] private AFPSCounter _fpsCounter;

		public static readonly CVar FpsEnabled = new("FPS Enabled", false, CFlags.NoArchive);
		public static readonly CVar DebugParticles = new("Debug: Particles", false, CFlags.NoArchive);

		private AFPSCounter _fpsInstance;

		private readonly StringBuilder _sb = new(1024);
		private int _maxTweens;

		private readonly List<DiContainer> _containers = new(8);
		private static LunarConsole _console;

		public void OnInstall(DiContainer container) {
			_containers.AddOnce(container);
		}

		public void OnUninstall(DiContainer container) {
			_containers.Remove(container);
		}

		private T Resolve<T>() {
			foreach (var diContainer in _containers) {
				var result = (T)diContainer.TryResolve(TypeOf<T>.Raw);
				if (result != null) return result;
			}

			return default;
		}

		private void SetupConsole() {
			
#if FEATURE_CHEATS
			_console = _lunarConsoleFull.Spawn();
#else			
			_console = _lunarConsole.Spawn();
#endif

			_fpsInstance = _fpsCounter.Spawn();
			FpsEnabled.AddDelegate(cvar => _fpsInstance.OperationMode = cvar.BoolValue ? OperationMode.Normal : OperationMode.Disabled);
			_fpsInstance.OperationMode = FpsEnabled.BoolValue ? OperationMode.Normal : OperationMode.Disabled;
			
#if !FEATURE_CHEATS
			LunarConsole.RegisterAction("Send Logs to Discord", SendLogsToDiscord);
#endif
		}

		private void SendLogsToDiscord() {
			var popup = new GameObject().AddComponent<DebugMessageWindow>();
			popup.Show(Resolve<IDebugLogCollector>());
			LunarConsole.Hide();
		}

		private void UpdateFpsCounter() {
			if (!_fpsInstance) return;

			if (!FpsEnabled.BoolValue) {
				_fpsInstance.AppendText = string.Empty;
				return;
			}

			var tweens = DOTween.TotalPlayingTweens();
			_maxTweens = Mathf.Max(_maxTweens, tweens);

			_sb.Clear();
			_sb.AppendLine($"tweens={tweens}/{_maxTweens}");

			if (DebugParticles.BoolValue) UIParticleDumpInfo(_sb);

			_fpsInstance.AppendText = _sb.ToString();
		}

		private static void UIParticleDumpInfo(StringBuilder result)
		{
			var activeParticles = (List<UIParticle>) Type.GetType("Coffee.UIExtensions.UIParticleUpdater")?.GetField("s_ActiveParticles", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
			if(activeParticles == null) return;
			var psTotal = activeParticles.Sum(x => x.particles.Count);
			
			result.AppendLine($"UIParticles total ui/ps/particles#={activeParticles.Count}/{psTotal}");

			foreach (var (name, count) in activeParticles.Select(x => (x.name, count: x.particles.Sum(z => z.particleCount)))
				         .OrderByDescending(x => x.count).Take(8))
			{
				result.AppendLine($"{name}: {count}");
			}
		}

		
		public void Tick() {
			UpdateFpsCounter();

#if FEATURE_CHEATS
			UpdateCheats();
#endif // FEATURE_CHEATS
		}
	}

}

#endif