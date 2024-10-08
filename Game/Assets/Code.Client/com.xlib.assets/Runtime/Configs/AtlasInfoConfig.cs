using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;
using XLib.Unity.Utils;

namespace XLib.Assets.Configs {

	[CreateAssetMenu(menuName = "Configs/AtlasInfoConfig")]
	public class AtlasInfoConfig : ScriptableObject {

		[SerializeField] internal AtlasDesc[] _spriteInfo;
		public AtlasDesc[] SpriteInfo => _spriteInfo;

		[Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
		public class AtlasDesc {

			public string name;
			public string[] sprites;

		}

	}

}