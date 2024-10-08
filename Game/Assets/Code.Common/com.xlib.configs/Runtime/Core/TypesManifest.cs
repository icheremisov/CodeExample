using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Contracts;

namespace XLib.Configs.Core {
	
	[ValidateAssetName("types_list")]
	public partial class TypesManifest : AssetManifest {

		[Serializable]
		public struct ScriptInfo : IEqualityComparer<ScriptInfo>{

			internal const string TypeName = nameof(type);
			internal const string GuidName = nameof(guid);
			public string type;
			public string guid;

			public bool Equals(ScriptInfo x, ScriptInfo y) => x.type == y.type && x.guid == y.guid;
			public int GetHashCode(ScriptInfo obj) => HashCode.Combine(obj.type, obj.guid);
		}

		[SerializeField, Title("Path to shared code with the server"), FolderPath] protected string[] _sources;
		[SerializeField, Title("Serializable types"), Searchable, TableList(NumberOfItemsPerPage = 25, ShowPaging = true)] 
		protected List<ScriptInfo> _scripts; // информация для сервера
		
		public static string ScriptsName => nameof(_scripts);

	}
}