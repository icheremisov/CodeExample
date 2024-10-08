
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;

namespace Client.Ecs {

	public static class JennyUtils {

#if UNITY_EDITOR_WIN
		private const  int SW_SHOWNORMAL = 1;

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
		
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool SetForegroundWindow(IntPtr hwnd);
		
		[MenuItem("Tools/Jenny/Start Server %&R", false, 100)]
		public static void StartJennyServer() {
			var jenny = FindWindow(null, "Jenny server");
			if (jenny != IntPtr.Zero) {
				ShowWindow(jenny, SW_SHOWNORMAL);
				SetForegroundWindow(jenny);
				return;
			}
			Process.Start("cmd", $"/k \"{Directory.GetCurrentDirectory()}/Jenny-Server.bat\"");
		}
#endif
#if UNITY_EDITOR_OSX
		[MenuItem("Tools/Jenny/Start Server %&R", false, 100)]
		public static void StartJennyServer()
		{
			var process = new Process
			{
				StartInfo =
				{
					FileName = "/bin/bash",
					Arguments = $"-c \"open -a Terminal {Directory.GetCurrentDirectory()}/Jenny-Server\"",
					CreateNoWindow = false,
					WindowStyle = ProcessWindowStyle.Minimized,
					UseShellExecute = true
				}
			};
			// Process.Start("/bin/bash", "-c open -a Terminal \"" + $"{Directory.GetCurrentDirectory()}/Jenny-Server" + "\"");
			UnityEngine.Debug.Log($"Run: {process.StartInfo.Arguments}");
			process.Start();
		}
#endif
	}
}

