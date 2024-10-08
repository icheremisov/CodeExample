using Client.Core.Common.Contracts;
using Client.Core.Common.UI.SystemDialog;
using Client.Core.GameFlow.Contracts;
using Client.Levels.Contracts;
using Cysharp.Threading.Tasks;

namespace Client.Levels.UI {

	public static class LevelUtils {
		public static async UniTask ExitClick(this LevelContext levelContext, LevelScreen levelScreen, ILevelFlowController levelFlowController,
			ISystemDialog systemDialog) {
			if (levelScreen.IsLocked) return;
			
			if (levelContext == null || levelFlowController == null) return;

			levelContext.isLevelPaused = true;

			if (await systemDialog.ShowAsync("Leave the level", "Are you sure you want to quit current level? This will be counted as a loss and you will not receive any rewards.",
					"Exit", "Cancel") != DialogResult.Ok) {
				levelContext.isLevelPaused = false;
				return;
			}

			levelContext.isLevelPaused = false;

			using (levelScreen.Lock()) {
				levelContext.SafeReplaceBattleState(ClientLevelState.Exit);
			}

			levelContext.isLevelPaused = true;
			levelFlowController.ExitLevel().Forget();
		}
	}

}