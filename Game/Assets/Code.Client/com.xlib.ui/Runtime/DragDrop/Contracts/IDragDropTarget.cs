namespace XLib.UI.DragDrop.Contracts {

	public interface IDragDropTarget {

		object TargetId { get; }
		bool CanDrop(object droppedObjectId, IDraggableObject droppedObject);

	}

}