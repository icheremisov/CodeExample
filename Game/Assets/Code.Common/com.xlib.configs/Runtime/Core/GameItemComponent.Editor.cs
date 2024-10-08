#if UNITY_EDITOR
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Unity.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs.Core {

	public partial class GameItemComponent : IItemContainerEditor {
		public virtual void AddToContainer(Object container, FileId fileId) {
			_owner = (GameItemBase)container;
			_fileId = fileId;
			this.SetObjectDirty();
		}

		public bool Rename(string newName) {
			Debug.Assert(!Application.isPlaying);
			if (base.name != newName) {
				base.name = newName;
				_assetName = newName;
				this.SetObjectDirty();
				if (_owner) _owner.SetObjectDirty();
				return true;
			}

			return false;
		}

		public virtual string EditorGenerateName(int index) {
			return $"{name}";
		}

		public virtual string GetInspectorName => name;

		public virtual bool EditorUpdateName(int index) => Rename(EditorGenerateName(index));
	}

}
#endif