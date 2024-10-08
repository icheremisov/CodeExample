namespace Client.Levels.Contracts
{
    public enum ClientLevelState {
        None = 0,
        Start = 1,
        Active = 2,
        End = 3,
        Exit = 4,
        TutorialPause = 5,
    }

    public static class LevelStateExtension
    {
        public static void SafeReplaceBattleState(this LevelContext context, ClientLevelState state) {
            context.ReplaceDesiredLevelState(state);
            if (context.hasLevelStateChange && (~context.levelStateChange != null))
                context.ReplaceLevelState((~context.levelStateChange)?.Invoke(state) ?? state);
            else
                context.ReplaceLevelState(state);
        }

    }
}