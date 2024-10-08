#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Client.Core.Ecs.Attributes;
using Entitas;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XLib.Core.Reflection;
using XLib.Core.Utils;
using XLib.Unity.Utils;

namespace Client.Core.Ecs.Configs {

	[Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
	public struct EcsSystemInfo {
		[HorizontalGroup, HideLabel]
		public bool Add;
		[HorizontalGroup, ReadOnly, HideLabel]
		public MonoScript EcsSystemScript;
	}
	
	public partial class EcsFeatureConfig {
		[Space, SerializeField, FolderPath, Required]
		private string _ecsSystemsPath;
		[Space, ListDrawerSettings(DefaultExpandedState = true, ShowItemCount = true, NumberOfItemsPerPage = 32), SerializeField, InlineProperty, OnValueChanged(nameof(UpdateList))]
		private EcsSystemInfo[] _ecsSystems;

		private HashSet<MonoScript> EnumerateSystems() {
			var needRefresh = _ecsSystems.Any(info => info.EcsSystemScript == null);

			if (!needRefresh) {
				var map = _ecsSystems.Where(x => x.EcsSystemScript != null).ToDictionary(info => info.EcsSystemScript.GetClass(), info => info);
				var types = TypeOf<ISystem>.Inheritors.Where(IsValidType);
				needRefresh = types.Any(type => !map.ContainsKey(type));	
			}

			if (!needRefresh) return null;
			var scripts = EditorUtils.LoadAssets<MonoScript>(string.Empty, _ecsSystemsPath ?? string.Empty);
			if (scripts.Count == 0) return null;
			
			var ecsSystem = TypeOf<ISystem>.Raw;
			return scripts.Where(m => {
				var x = m.GetClass();
				if (x == null) return false;
				return !x.IsAbstract && !x.IsGenericType && ecsSystem.IsAssignableFrom(x) && IsValidType(x);
			}).ToHashSet();
		}

		private bool IsValidType(Type type) {
			var attr = type.GetAttribute<FeatureAttribute>();
			if (attr != null) return attr.Feature == name;
			return _wildcards?.Any(x => !x.IsNullOrEmpty() && type.FullName.IsMatch(x)) ?? true;
		}

		[Button]
		public void UpdateList() {
			var existingSystems = EnumerateSystems();
			if (existingSystems == null) return;

			var result = new List<EcsSystemInfo>(32);

			var loadedSystems = new HashSet<MonoScript>(32);
			if (!_ecsSystems.IsNullOrEmpty()) {
				foreach (var info in _ecsSystems.Where(x => existingSystems.Contains(x.EcsSystemScript))) {
					result.Add(info);
					loadedSystems.Add(info.EcsSystemScript);
				}
			}
			
			existingSystems.ExceptWith(loadedSystems);

			result.AddRange(existingSystems.Select(x => new EcsSystemInfo { EcsSystemScript = x, Add = true }));

			_ecsSystems = result.ToArray();
			_systemTypes = _ecsSystems.Where(x => x.Add)
				.Select(x => x.EcsSystemScript.GetClass().FullName).ToArray();
			EditorUtility.SetDirty(this);
		}

		private void OnValidate() => UpdateList();
	}

}

#endif