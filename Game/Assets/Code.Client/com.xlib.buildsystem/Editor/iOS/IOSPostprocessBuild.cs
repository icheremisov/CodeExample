#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

// 2019.3+
namespace XLib.BuildSystem.iOS {

	public static class IOSPostprocessBuild {
		private const string TrackingDescription = "We will use your data to provide a better and personalized ad experience.";

		[PostProcessBuild(9999999)]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
			Debug.Log("IOSPostprocessBuild: Start");

			Debug.Assert(target == BuildTarget.iOS);
			
			var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

			var proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));

			SetupFrameworks(proj);
			SetupCapabilities(pathToBuiltProject, proj, projPath);
			DisableBitcode(proj);
			
			File.WriteAllText(projPath, proj.WriteToString());
			
			PatchPodFile(pathToBuiltProject);
			PatchPlist(pathToBuiltProject);
			
			Debug.Log("IOSPostprocessBuild: OK");
		}

		private static void SetupFrameworks(PBXProject proj) {
			Debug.Log("IOSPostprocessBuild: SetupFrameworks");

			var target = proj.GetUnityMainTargetGuid();
			proj.AddFramework(target, "GameKit.framework", true);
			proj.AddFramework(target, "UnityFramework.framework", false);
			
			target = proj.GetUnityFrameworkTargetGuid();
			proj.AddFramework(target, "GameKit.framework", true);
		}

		private static void SetupCapabilities(string pathToBuiltProject, PBXProject proj, string projPath) {
			Debug.Log("IOSPostprocessBuild: SetupCapabilities");

			var target = proj.GetUnityMainTargetGuid();
			var entitlementFilePath = proj.GetEntitlementFilePathForTarget(target);
			Debug.Log($"IOSPostprocessBuild: SetupCapabilities entitlementFilePath='{entitlementFilePath}'");
			
			proj.AddCapability(target, PBXCapabilityType.InAppPurchase);
			proj.AddCapability(target, PBXCapabilityType.GameCenter, entitlementFilePath);
			
			proj.WriteToFile(projPath);
			
			//if (entitlementPath.IsNullOrEmpty()) entitlementPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.entitlements");
			
			var entitlementsPath = Path.Combine(pathToBuiltProject, entitlementFilePath);
			var entitlementsDoc = new PlistDocument();
			if (File.Exists(entitlementsPath)) entitlementsDoc.ReadFromFile(entitlementsPath);

			entitlementsDoc.root.SetBoolean("com.apple.developer.game-center", true);
			
			entitlementsDoc.WriteToFile(entitlementsPath);
		}

		private static void DisableBitcode(PBXProject proj) {
			Debug.Log("IOSPostprocessBuild: DisableBitcode");
     
			// Main
			var target = proj.GetUnityMainTargetGuid();
			proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

			// Unity Tests
			target = proj.TargetGuidByName(PBXProject.GetUnityTestTargetName());
			proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

			// Unity Framework
			target = proj.GetUnityFrameworkTargetGuid();
			proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
		}

		private static void PatchPodFile(string pathToBuiltProject) {
			Debug.Log("IOSPostprocessBuild: PatchPodFile");
     
			var content = "\n\npost_install do |installer|\n" +
				"installer.pods_project.targets.each do |target|\n" +
				"  target.build_configurations.each do |config|\n" +
				$"    config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '{PlayerSettings.iOS.targetOSVersionString}'\n" +
				// https://stackoverflow.com/questions/72561696/xcode-14-needs-selected-development-team-for-pod-bundles
				$"    config.build_settings['CODE_SIGNING_ALLOWED'] = 'NO'\n" +
				"    config.build_settings['ENABLE_BITCODE'] = 'NO'\n" +
				"  end\n" +
				" end\n" +
				"end\n";

			using var streamWriter = File.AppendText(Path.Combine(pathToBuiltProject, "Podfile"));
			streamWriter.WriteLine(content);
		}
		
		private static void PatchPlist(string pathToBuiltProject) {
			Debug.Log("IOSPostprocessBuild: PatchPlist");
     
			var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
			Debug.Log($"IOSPostprocessBuild: PatchPlist plistPath='{plistPath}'");

			var plistObj = new PlistDocument();
				// Read the values from the plist file:
			if (File.Exists(plistPath)) plistObj.ReadFromFile(plistPath);

			// Set values from the root object:
			var plistRoot = plistObj.root;

			// Set the description key-value in the plist:
			plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);
			plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);

			// Save changes to the plist:
			plistObj.WriteToFile(plistPath);
		}
		
		private static void AddFramework(this PBXProject proj, string target, string framework, bool weak) {
			if (!proj.ContainsFramework(target, framework)) proj.AddFrameworkToProject(target, framework, weak);
		}
	}

}

#endif