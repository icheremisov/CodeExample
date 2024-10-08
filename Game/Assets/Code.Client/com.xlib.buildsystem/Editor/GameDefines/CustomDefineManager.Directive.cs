using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace XLib.BuildSystem.GameDefines {

	[Serializable]
	public class Directive {
		[FormerlySerializedAs("name")] [SerializeField]
		public string _name;

		[FormerlySerializedAs("targets")] [SerializeField]
		public CustomDefineManager.CdmBuildTargetGroup _targets;

		[FormerlySerializedAs("enabled")] [SerializeField]
		public bool _enabled = true;

		[FormerlySerializedAs("sortOrder")] [SerializeField]
		public int _sortOrder = 0;

		public override string ToString() => $"{_name} : {_targets.ToString()}";
	}

}