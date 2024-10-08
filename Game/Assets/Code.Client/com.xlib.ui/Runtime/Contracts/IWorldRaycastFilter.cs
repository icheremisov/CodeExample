using UnityEngine.EventSystems;
using XLib.UI.Controls;

namespace XLib.UI.Contracts {

	public interface IWorldRaycastFilter {
		bool CheckWorldRaycast<T>(T target) where T : class;
		bool CheckWorldEvent(UIInputEvent evt, ITapHandler target);
		bool CheckUIEvent(UIInputEvent evt, PointerEventData arg);
	}

	public interface IWorldMasterRaycastFilter {
		bool CheckWorldRayCast<T>(T target) where T : class;
		bool CheckWorldEvent(UIInputEvent evt, ITapHandler target);
		bool CheckUIEvent(UIInputEvent evt, PointerEventData arg);
	}

}