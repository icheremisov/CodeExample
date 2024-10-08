using Sirenix.OdinInspector;
using UnityEngine;

namespace ThirdParty.Editor.CleanEmptyDir.Editor {

	public class CleanEmptyDirSettings : ScriptableObject {
		[Space, InfoBox("You can set one or more Wildcards for type names (with * and ? symbols)"),
		 ListDrawerSettings(DefaultExpandedState = true, ShowItemCount = true, NumberOfItemsPerPage = 32), SerializeField, InlineProperty, Required]
		private string[] _ignoreWildcards;
		public string[] IgnoreWildcards => _ignoreWildcards;
	}

}