// #if UNITY_IOS
// using System.IO;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEditor.iOS.Xcode;
//
// namespace XLib.BuildSystem.Editor.iOS
// {
//    public class DevToDevPostBuild
//    {
//        [PostProcessBuild(1)]
//        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
//        {
//            if (target != BuildTarget.iOS)
//            {
//                return;
//            }
//            iOSPostBuild(pathToBuiltProject);
//        }
//        private static void iOSPostBuild(string projPath)
//        {
//            var pbxprojPath = $"{projPath}/Unity-iPhone.xcodeproj/project.pbxproj";
//            
//            var proj = new PBXProject();
//            proj.ReadFromString(File.ReadAllText(pbxprojPath));
//            
//            var projectGuid = proj.GetUnityMainTargetGuid();
//            proj.AddFrameworkToProject(projectGuid, "AdSupport.framework", true);
//            // IOS 14. Xcode 12 required.
//            //proj.AddFrameworkToProject(projectGuid, "AppTrackingTransparency.framework", true);
//            File.WriteAllText(pbxprojPath, proj.WriteToString());
//        }
//    }
// }
// #endif