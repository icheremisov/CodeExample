using System;
using Client.Core.Common.Configs;
using Client.Core.Ecs.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Client.Core.Configs {

	[Serializable]
	public class ClientConfig {

		[FoldoutGroup("Core", Expanded = true), HideLabel, InlineProperty, SerializeField, Required]
		private CoreConfig _coreConfig;
		
		[FoldoutGroup("ECS", Expanded = true), HideLabel, InlineProperty, SerializeField, Required]
		private EcsConfig _ecsConfig;

		public CoreConfig Core => _coreConfig;
		public EcsConfig Ecs => _ecsConfig;

		public void OnEnable() => _ecsConfig.OnEnable();
		
	}

}