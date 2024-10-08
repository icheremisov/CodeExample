//////////////////////////////////////////////////////////////////////
// iOS Stuff
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.FileHelpers&version=7.0.0

using Path = System.IO.Path;

#load const.cake
#load build-config.cake
#load client-common.cake

string GetIOSProjectName() => Path.Join(Global.StagingDir, BuildArtefactFileName(), "Unity-iPhone.xcodeproj");
string GetIOSWorkspaceName() => Path.Join(Global.StagingDir, BuildArtefactFileName(), "Unity-iPhone.xcworkspace");
bool HasIOSWorkspace() => DirectoryExists(GetIOSWorkspaceName());
string GetIOSArchiveName() => Path.Join(Global.StagingDir, "game.xcarchive");
string GetIOSExportPListName() => Path.Join(Global.StagingDir, "exportOptions.plist");

void CreatePListFile(string templateName, string targetFileName) {
	var plist = FileReadText(Path.Join("templates", templateName));

	plist = BuildConfig.ReplaceVariables(plist);
	
	CreateDirectory(Path.GetDirectoryName(targetFileName));
	FileWriteText(targetFileName, plist);
}

