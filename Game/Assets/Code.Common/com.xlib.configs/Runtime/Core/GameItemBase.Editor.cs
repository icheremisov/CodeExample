#if UNITY_EDITOR
using UnityEditor;
using XLib.Configs.Contracts;

namespace XLib.Configs.Core {

	public partial class GameItemBase : IGameItemEditor {
		public static string CreateMethodName => nameof(OnCreateEditorOnly);
		public bool CanPaste => !string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer) && EditorGUIUtility.systemCopyBuffer.StartsWith("{");
		
		void IGameItemEditor.SetId(ItemId itemId, string fileName) {
			_id = itemId;
			if (!fileName.IsNullOrEmpty()) FileName = fileName;
		}

		protected virtual void OnCreateEditorOnly() { }
	}

}
#endif