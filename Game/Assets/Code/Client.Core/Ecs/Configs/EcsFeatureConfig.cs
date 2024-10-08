using System;
using System.Collections.Generic;
using Client.Core.Ecs.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Core.Utils;

namespace Client.Core.Ecs.Configs {

	public partial class EcsFeatureConfig : ScriptableObject {

		[Space, InfoBox("You can set one or more Wildcards for type names (with * and ? symbols). OR You can define Feature['featureName'] attribute for specific systems"),
		 ListDrawerSettings(DefaultExpandedState = true, ShowItemCount = true, NumberOfItemsPerPage = 32), SerializeField, InlineProperty, Required]
		private string[] _wildcards;

		[SerializeField, HideInInspector, InlineProperty, Required]
		private string[] _systemTypes;

		private Dictionary<Type, int> _executionOrder;
		private int _lastOrder;
		private List<Type> _typeInstances;

		public List<Type> Types {
			get {
				Create();
				return _typeInstances;
			}
		}

		public int SortingOrder { get; set; }

		public FeatureId Id => new(name);

		private void OnEnable() {
			Create();
		}

		public int GetOrder(Type ecsSystemType) {
			if (_executionOrder.TryGetValue(ecsSystemType, out var result)) return result;
			
			result = _lastOrder++;
			_executionOrder.Add(ecsSystemType, result);

			return result;
		}

		private void Create() {
			if (_executionOrder != null) return;

#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

			_executionOrder = new Dictionary<Type, int>(_systemTypes?.Length ?? 0);
			_typeInstances = new(_executionOrder.Count);

			_lastOrder = 1;

			if (_systemTypes.IsNullOrEmpty()) return;

			foreach (var t in _systemTypes) {
				var type = TypeUtils.GetType(t);
				if (type == null) continue;

				_executionOrder.Add(type, _lastOrder);
				_typeInstances.Add(type);
				_lastOrder++;
			}
		}

	}

}