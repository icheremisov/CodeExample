namespace XLib.UI.DragDrop.Contracts {

	public delegate void DropHandler(object droppedObjectId, object targetId);

	public interface IDragDropRoot {

		bool DragEnabled { get; set; }
		
		event DropHandler ObjectDropped;

		void CancelDrag();

	}

}