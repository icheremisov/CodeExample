// using System.Threading.Tasks;
// using Client.Cheats.Contracts;
// using Client.Meta.Core.Contracts;
// using Client.Meta.Tutorial.Contracts;
// using Client.Meta.Tutorial.Internal;
// using JetBrains.Annotations;
// using Shared.Definitions.Core;
// using Shared.Definitions.Tutorial;
// using Shared.Logic.Tutorial.Modules;
// using UnityEngine;
// using XLib.Core.Utils;
// using XLib.Unity.Utils;
//
// namespace Client.Cheats {
//
// 	[CheatCategory("Tutorial"), PublicAPI]
// 	public static class TutorialCheats {
// 		[CheatToggle("Tutorial/Debug Info")]
// 		public static bool TutorialDebugInfo { get => TutorialHelper.TutorialDebug; set => TutorialHelper.TutorialDebug = value; }
//
// 		[CheatMethod("Tutorial/Skip Step")]
// 		public static void TutorialSkip(ITutorialProcessor processor) => processor.SkipStep();
//
// 		[CheatMethod("Tutorial/Abort")]
// 		public static void TutorialAbort(ITutorialProcessor processor) => processor.AbortStage(false);
//
// 		[CheatMethod("Tutorial/Unlock")]
// 		public static void TutorialUnlock(ITutorialBehavior behavior) => behavior.UnlockInput();
//
// 		[CheatMethod("Tutorial/Try Start")]
// 		private static void TutorialStart(IGameTriggersObserver observer, ILockable lockable) {
// 			observer.HandleAction(GameEventTrigger.Any);
// 		}
//
// 		[CheatPluginGUI("Tutorial/List")]
// 		private static void TutorialRun(TutorialStageDefinition stage, ITutorialProcessor tutorialProcessor, TutorialModule module, ILockable lockable) {
// 			using (new GUILayout.HorizontalScope()) {
// 				var state = module.GetStageData(stage)?.State ?? TutorialStageState.Unseen;
// 				GUILayout.Label($"{stage} : {state}");
//
// 				GUILayout.FlexibleSpace();
// 				if (GuiEx.Button("Run", Color.green)) {
// 					Cheat.Minimize();
// 					TutorialRunInternal(stage, tutorialProcessor, lockable).Forget();
// 				}
//
// 				if (state is TutorialStageState.Completed or TutorialStageState.AutoCompleted) {
// 					if (GuiEx.Button("Clear", Color.red)) new TutorialModule.CheatResetRequest() { Stage = stage }.Send(lockable);
// 				}
// 				else {
// 					if (GuiEx.Button("Complete", Color.blue)) new TutorialModule.CheatSetComplete() { Stage = stage }.Send(lockable);
// 				}
// 			}
// 		}
//
// 		private static async Task TutorialRunInternal(TutorialStageDefinition stage, ITutorialProcessor tutorialProcessor, ILockable lockable) {
// 			using var _ = lockable.Lock();
// 			await new TutorialModule.CheatResetRequest() { Stage = stage };
// 			tutorialProcessor.ForceTutorialStart(stage);
// 		}
//
// 		[CheatMethod("Tutorial/Reset")]
// 		private static void TutorialReset(ILockable lockable) {
// 			(new TutorialModule.CheatResetRequest()).Send(lockable);
// 		}
//
// 		[CheatMethod("Tutorial/Complete (Hard Only)")]
// 		private static void TutorialCompleteHard(ILockable lockable, ITutorialProcessor processor) {
// 			processor.AbortStage(false);
// 			(new TutorialModule.CheatSetCompleteAll() { Mode = SkipTutorialMode.Main }).Send(lockable);
// 		}
//
// 		[CheatMethod("Tutorial/Complete (Hard & Soft)")]
// 		private static void TutorialCompleteHardSoft(ILockable lockable, ITutorialProcessor processor) {
// 			processor.AbortStage(false);
// 			(new TutorialModule.CheatSetCompleteAll() { Mode = SkipTutorialMode.All }).Send(lockable);
// 		}
// 	}
//
// }