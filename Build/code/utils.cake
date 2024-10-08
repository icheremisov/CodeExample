//////////////////////////////////////////////////////////////////////
// Utility Methods
//////////////////////////////////////////////////////////////////////

using Path = System.IO.Path;
using SysFile = System.IO.File;
using SysDir = System.IO.Directory;

void CreateFolderSymlink(string linkFolder, string sourceFolder) {

	if (sourceFolder.StartsWith("~/")) sourceFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), sourceFolder[1..]);
	if (linkFolder.StartsWith("~/")) linkFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), linkFolder[1..]);

	var link = Path.GetFullPath(linkFolder);
	var source = Path.GetFullPath(sourceFolder);

	Information($"CreateFolderSymlink: link={linkFolder} ({link}); src={sourceFolder} ({source})");

	if (!SysDir.Exists(source)) throw new Exception($"CreateFolderSymlink failed: sourceFolder does not found '{sourceFolder}' (full='{source}')");
	if (SysDir.Exists(link)) return;

	if (IsRunningOnWindows()) {
		var opts = $"/C mklink /j \"{link}\" \"{source}\"";

		Information($"Executing: cmd.exe {opts}");

		var result = StartProcess("cmd.exe", new ProcessSettings { 
			Arguments = opts
		});
		if (result != 0) throw new Exception($"CreateFolderSymlink command failed");
	} else {
		var opts = IsRunningOnMacOs() ? "-shf" : "-sf";
		opts = $"-c \"ln {opts} '{source}' '{link}'\"";

		Information($"Executing: /bin/bash {opts}");

		var result = StartProcess("/bin/bash", new ProcessSettings { 
			Arguments = opts
		});
		if (result != 0) throw new Exception($"CreateFolderSymlink command failed");
	}

}

async Task RecreateDirectory(string path, int retryCount = 2, int timeoutMs = 1000) {
	Information($"RecreateDirectory '{path}'...");

	--retryCount;

	for (int i = 0; i < retryCount; ++i) {
		try {
			if (SysDir.Exists(path)) SysDir.Delete(path, true);
			break;
		} 
		catch (Exception ex) {
			Information($"\tError removing, retrying. Message={ex.Message}");
			await System.Threading.Tasks.Task.Delay(timeoutMs);
		}
	}

	if (SysDir.Exists(path)) SysDir.Delete(path, true);
	SysDir.CreateDirectory(path);
	
	Information($"Directory Recreated");
}


string Base64Encode(string plainText) {
  var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
  return System.Convert.ToBase64String(plainTextBytes);
}

string GetReadableSize(long bytesSize)
{
	string[] sizes = { "b", "Kb", "Mb", "Gb" };
	double len = bytesSize;
	var order = 0;
	while (len >= 1024 && order < sizes.Length - 1) {
		order++;
		len = len / 1024;
	}

	return $"{len:0.##} {sizes[order]}";
}
