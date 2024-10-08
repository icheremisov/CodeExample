/////////////////////////////////////////////////////////////////////
// INFO TASKS
//////////////////////////////////////////////////////////////////////
#load const.cake
#load build-config.cake
#load build-version.cake
#load build-library.cake

Task("Info")
	.Does(() => 
{
	Information($"Platform={Global.Platform}");
	Information($"Target={Global.Target}");
	Information($"Gitlab={BuildSystem.GitLabCI.IsRunningOnGitLabCI}");
	Information($"BuildConfig={BuildConfig.GetBaseFilePath()}");
	Information($"StagingDir={Global.StagingDir}");
	Information($"ArtefactsDir={Global.ArtefactsDir}");
	Information($"BuildArtefactFileName={BuildArtefactFileName()}");
	Information($"ResultFileName={ResultFileName()}");
    Information($"Branch={BuildVersion.EnvName}");
	Information($"LibraryCacheDir={Global.LibraryCacheDir}");
	Information($"FeatureDefines={BuildConfig.FeatureDefines}");
});

Task("Dump-Errors")
	.Does(async () => 
{
	var logFile = Argument<string>("log");
	await DumpUnityErrors(logFile);

});
