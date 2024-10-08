using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using XLib.Configs.Contracts;
#endif

namespace XLib.Configs.Core {

	public class GameItemPatcherSettings : ScriptableObject {
#if UNITY_EDITOR
		[SerializeField] private string[] _syncPaths;
		[SerializeField] private string[] _syncIgnorePaths;
		[SerializeField] private bool _syncAllMissionSteps;
		[SerializeField, HideIf(nameof(_syncAllMissionSteps))] private List<MonoScript> _missionStepTypesSync;

		public string[] SyncPaths => _syncPaths;
		public string[] SyncIgnorePaths => _syncIgnorePaths;
		public bool SyncAllMissionSteps => _syncAllMissionSteps;
		public Type[] MissionStepTypesSync => _missionStepTypesSync.SelectToArray(t => t.GetClass());
#endif
	}

}

