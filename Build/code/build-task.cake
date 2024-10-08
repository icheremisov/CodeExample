#load const.cake
#load client-ios.cake
#load client-android.cake

#addin nuget:?package=Cake.XCode&version=5.0.0

UnityEditorDescriptor editor;

Task("Build-Android")
    .IsDependentOn("Publish-Clean")
    .IsDependentOn("Build-Unity")
    .IsDependentOn("Publish-Android-Artifacts")
    .IsDependentOn("Publish-Android-Symbols")
    ;

Task("Build-iOS")
    .IsDependentOn("Publish-Clean")
    .IsDependentOn("Build-Unity")
    .IsDependentOn("Build-iOS-GeneratePList")
    .IsDependentOn("Build-iOS-XCode-Compile")
    .IsDependentOn("Build-iOS-XCode-Archive")
    .IsDependentOn("Publish-iOS-Artifacts")
    ;

Task("Publish-Clean").Does(async () =>
{
	await RecreateDirectory(Global.ArtefactsDir); 	
});

Task("Build-Unity-TouchCode")
    .IsDependentOn("Build-Clean")
    .IsDependentOn("Build-Prepare-UnityEditor")
	.Does(async () =>
{
	var logFile = Path.Join(Global.StagingDir, "unity-code.log");
	if (System.IO.File.Exists(logFile)) System.IO.File.Delete(logFile);

	var args = new UnityEditorArguments
	{
		ProjectPath = Global.ProjectDir,
		LogFile = logFile,
	};

	switch (Global.Platform) {
		case "Android": 
			args.BuildTarget = BuildTarget.Android;
			break;
		case "iOS": 
			args.BuildTarget = BuildTarget.iOS;
			break;
		default: throw new Exception("Unsupported target");
	}

	args.ExecuteMethod = "XLib.BuildSystem.BuildRunner.ExternalTouchCode";
	args.Custom.optionsFile = BuildConfig.GetBaseFilePath();
	args.Custom.outputPath = Global.StagingDir; 
	args.Custom.outputFileName = BuildArtefactFileName(); 
	args.Custom.versionCode = BuildVersion.VersionCode; 

	try
	{
		UnityEditor(editor, args, new UnityEditorSettings() { RealTimeLog = true });
	}
	catch 
	{
		await DumpUnityErrors(logFile);
		throw;
	}
});

Task("Build-Unity")
    .IsDependentOn("Build-Clean")
    .IsDependentOn("Build-Prepare-UnityEditor")
	.IsDependentOn("Build-Prepare-Version")
	.IsDependentOn("Build-Unity-TouchCode")
	.Does(async () =>
{
	var logFile = Path.Join(Global.StagingDir, "unity.log");
	if (System.IO.File.Exists(logFile)) System.IO.File.Delete(logFile);

	var args = new UnityEditorArguments
	{
		ProjectPath = Global.ProjectDir,
		LogFile = logFile,
	};

	switch (Global.Platform) {
		case "Android": 
			args.BuildTarget = BuildTarget.Android;
			args.ExecuteMethod = "XLib.BuildSystem.BuildRunner.ExternalAndroidBuild";
			break;
		case "iOS": 
			args.BuildTarget = BuildTarget.iOS;
			args.ExecuteMethod = "XLib.BuildSystem.BuildRunner.ExternalIOSBuild";
			break;
		default: throw new Exception("Unsupported target");
	}

	args.Custom.optionsFile = BuildConfig.GetBaseFilePath();
	args.Custom.outputPath = Global.StagingDir; 
	args.Custom.outputFileName = BuildArtefactFileName(); 
	args.Custom.versionCode = BuildVersion.VersionCode; 

	try
	{
		UnityEditor(editor, args, new UnityEditorSettings() { RealTimeLog = true });
	}
	catch 
	{
		await DumpUnityErrors(logFile);
		throw;
	}
});

Task("Publish-Android-Artifacts")
	.Does(() =>
{
	var target = Path.Join(Global.ArtefactsDir, ResultFileName());
	if (FileExists(target)) DeleteFile(target);

	MoveFile(Path.Join(Global.StagingDir, BuildArtefactFileName()), target);
});

Task("Build-iOS-GeneratePList")
	.Does(() =>
{
	var templateName = BuildConfig.Var("PList");
	CreatePListFile(templateName, GetIOSExportPListName());
});

Task("Build-iOS-XCode-Compile")
	.Does(() =>
{
	var settings = new XCodeBuildSettings()
	{
		// AllowProvisioningUpdates = true,
		Scheme = "Unity-iPhone",
		Configuration = "Release",
		Archive = true,
		ArchivePath = GetIOSArchiveName()
	};

	if (HasIOSWorkspace()) settings.Workspace = GetIOSWorkspaceName();
	else settings.Project = GetIOSProjectName();

	settings.BuildSettings["DEVELOPMENT_TEAM"] = BuildConfig.Var("TeamId");
	settings.BuildSettings["CODE_SIGN_STYLE"] = "Automatic";
	settings.BuildSettings["USYM_UPLOAD_AUTH_TOKEN"] = "empty";
	settings.BuildSettings["ENABLE_BITCODE"] = "NO";
	
	XCodeBuild(settings);
	
});

Task("Build-iOS-XCode-Archive")
	.Does(() =>
{
	var settings = new XCodeBuildSettings()
	{
		// AllowProvisioningUpdates = true,
		ExportArchive = true,
		ArchivePath = GetIOSArchiveName(),
		ExportOptionsPlist = GetIOSExportPListName(),
		ExportPath = Global.StagingDir
	};

	XCodeBuild(settings);
});

Task("Publish-iOS-Artifacts")
	.Does(() =>
{
	var files = System.IO.Directory.GetFiles(Global.StagingDir, "*.ipa", SearchOption.TopDirectoryOnly);
	if (files.Length == 0) {
		Information("No IPA files found to publish - Skip");
		return;
	}

	if (files.Length > 1) {
		var fileStr = string.Join(", ", files.Select(Path.GetFileNameWithoutExtension));
		throw new Exception($"More than 1 IPA found: {fileStr}");
	} 

	var target = Path.Join(Global.ArtefactsDir, ResultFileName());
	if (FileExists(target)) DeleteFile(target);
	MoveFile(files[0], target);
});

Task("Build-Clean")
    .IsDependentOn("Build-Library-Remove")
    .IsDependentOn("Build-Library-LinkCache")
	.IsDependentOn("Build-Library-ClearAssembles")
	.Does(async () =>
{
	await RecreateDirectory(Global.StagingDir);
});

Task("Build-Prepare-UnityEditor").Does(() =>
{
	(var major, var minor, var patch) = DetectUnityProjectVersion();
    Information($"Detected project version: {major}.{minor}.{patch}");

    editor = FindUnityEditor(major, minor, patch);
    if (editor != null)
        Information("Found Unity Editor {0} at path {1}", editor.Version, editor.Path);
    else
        Error($"Cannot find Unity Editor {major}.{minor}.{patch}");
});

Task("Build-Prepare-Version").Does(() =>
{
	BuildVersion.Save();
});