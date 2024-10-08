using System.Collections.Generic;

namespace XLib.UI.Contracts {

	public interface IWorldRaycasterManager {
		public void AddWorldRayCaster(IWorldRaycaster raycaster);
		public void RemoveWorldRayCaster(IWorldRaycaster raycaster);
		IReadOnlyList<IWorldRaycaster> GetRayCasters();
		
		void AddRayCasterFilter(IWorldRaycastFilter filter);
		void RemoveRayCasterFilter(IWorldRaycastFilter filter);
	}

}