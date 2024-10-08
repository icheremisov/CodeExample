//////////////////////////////////////////////////////////////////////
// Pack symbols
//////////////////////////////////////////////////////////////////////

#pragma warning disable 649 

#load utils.cake
#load const.cake
#load build-config.cake
#load build-version.cake

using Path = System.IO.Path;
using SysFile = System.IO.File;
using SysDir = System.IO.Directory;

Task("Publish-Android-Symbols")
	.Does(() => 
{
	var symbols = BuildConfig.Var("AndroidCreateSymbols", "");
	if (symbols == "" || symbols == "Disabled") {
		Information("Symbols disabled by config");
		return;
	}

	var src = Global.StagingDir;
	var dst = Global.ArtefactsDir;

	var symbolsZipMask = "*.symbols.zip";
	
	var fileNames = SysDir.GetFiles(src, symbolsZipMask, SearchOption.TopDirectoryOnly);
	if (fileNames.Length == 0) {
		Information("No symbols found");
		return;
	}
	
	int index = 0;
	foreach (var file in fileNames) {
		var indexStr = index > 0 ? $"-{index}" : "";
        var targetFileName = MakeDstFileName(dst, $"symbols{indexStr}.zip");

		Information($"CopyLocal: {file}->{targetFileName}");

		SysFile.Copy(file, targetFileName, true);

		++index;
	}

	var symbolsDirMask = $"{BuildConfig.Var("BundleId")}_*";

	var folders = SysDir.GetDirectories(src, symbolsDirMask);

	if (folders.Length > 0) {
		var files = new List<string>(128);
		foreach (var folder in folders) {
			files.AddRange(GetFiles(Path.Join(folder, "**", "*.*")).Select(x => x.ToString()));
		}

		Zip(src, MakeDstFileName(dst, $"symbols-additional.zip"), files);
	}
});

