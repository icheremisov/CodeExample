// using System.Linq;
// using Client.App.Contracts;
// using Client.Cheats.Contracts;
// using Client.Cheats.Internal;
// using Cysharp.Threading.Tasks;
// using JetBrains.Annotations;
// using Server.Core.Contract.Model;
// using Shared.Core.Contracts;
// using Shared.Logic.Meta.Modules;
// using UnityEngine;
// using XLib.Core.Utils;
// using XLib.Unity.Utils;
//
// namespace Client.Cheats.Tools {
//
// 	[PublicAPI, CheatCategory("Tools")]
// 	public static class SnapshotCheats {
// 		private static string _snapshotName = "";
// 		private static SnapshotData[] _snapshots;
// 		private static Vector2 _snapshotScroll;
//
// 		[CheatPluginGUI("Tools/Snapshots")]
// 		public static void SnapshotList(IMetaHost host, IHostSnapshotService snapshotService, IUnityApplication unityApplication, ILockable locker) {
// 			GUILayout.Label($"ProfileId: {host.Id}");
//
// 			using (new GUILayout.HorizontalScope()) {
// 				GUILayout.Label("Snapshot:");
// 				CheatGui.Input(_snapshotName, out _snapshotName, false);
// 				if (GUILayout.Button("Create", GUILayout.Width(50))) {
// 					snapshotService.Save(host.Dump(), _snapshotName);
// 					ResetList();
// 				}
//
// 				if (GUILayout.Button("Refresh", GUILayout.Width(50))) {
// 					ResetList();
// 				}
// 			}
//
// 			_snapshots ??= snapshotService.GetAll().ToArray();
//
// 			_snapshotScroll = GUILayout.BeginScrollView(_snapshotScroll);
// 			foreach (var snapshot in _snapshots) {
// 				using (new GUILayout.HorizontalScope()) {
// 					GuiEx.Tooltip($"Device Id: {snapshot.DeviceId}");
// 					GuiEx.Label(snapshot.Name, Color.white);
// 					GuiEx.Label($"{snapshot.Timestamp}", Color.gray);
// 					GUILayout.FlexibleSpace();
// 					if (GuiEx.Button("Load", Color.green)) {
// 						var rawData = snapshotService.Load(snapshot);
// 						ResetProfile(rawData, unityApplication, locker).Forget();
// 					}
//
// 					if (GuiEx.Button("Apply", Color.yellow)) {
// 						var rawData = snapshotService.Load(snapshot);
// 						ResetProfile(rawData, null, locker).Forget();
// 					}
//
// 					if (GuiEx.Button("x", Color.red)) {
// 						snapshotService.Remove(snapshot);
// 						ResetList();
// 					}
// 				}
// 			}
//
// 			GUILayout.EndScrollView();
// 		}
//
// 		private static async UniTaskVoid ResetProfile(SharedRawData rawData, IUnityApplication unityApplication, ILockable locker) {
// 			using var _ = locker.Lock();
// 			await new Status.Reset() { Data = rawData };
// 			if (unityApplication != null) {
// 				await unityApplication.Reset(false);
// 			}
//
// 			Cheat.Minimize();
// 			ResetList();
// 		}
//
// 		private static void ResetList() {
// 			_snapshotName = "";
// 			_snapshots = null;
// 		}
// 	}
//
// }