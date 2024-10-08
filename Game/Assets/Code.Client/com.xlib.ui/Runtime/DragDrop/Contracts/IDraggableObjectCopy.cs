using UnityEngine;

namespace XLib.UI.DragDrop.Contracts {

	public interface IDraggableObjectCopy {

		object ObjectId { get; }
		Transform Transform { get; }
		void Destroy();
	}

}