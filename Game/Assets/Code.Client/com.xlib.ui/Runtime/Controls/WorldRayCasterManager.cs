using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using XLib.UI.Contracts;

namespace XLib.UI.Controls {

	public class WorldRayCasterManager : IWorldRaycasterManager, IWorldMasterRaycastFilter {
		private readonly List<IWorldRaycaster> _rayCasters = new(4);
		private readonly List<IWorldRaycastFilter> _filters = new(4);
		private static IWorldMasterRaycastFilter MasterFilter { get; set; }
		public static bool CheckUIEvent(UIInputEvent evt, PointerEventData arg) => MasterFilter.CheckUIEvent(evt, arg);

		public WorldRayCasterManager() {
			MasterFilter = this;
		}

		IReadOnlyList<IWorldRaycaster> IWorldRaycasterManager.GetRayCasters() => _rayCasters;
		void IWorldRaycasterManager.AddRayCasterFilter(IWorldRaycastFilter filter) => _filters.Add(filter);
		void IWorldRaycasterManager.RemoveRayCasterFilter(IWorldRaycastFilter filter) => _filters.Remove(filter);

		void IWorldRaycasterManager.AddWorldRayCaster(IWorldRaycaster rayCaster) {
			rayCaster.SetRaycasterFilter(this);
			_rayCasters.Add(rayCaster);
		}

		void IWorldRaycasterManager.RemoveWorldRayCaster(IWorldRaycaster rayCaster) {
			rayCaster.SetRaycasterFilter(null);
			_rayCasters.Remove(rayCaster);
		}

		bool IWorldMasterRaycastFilter.CheckWorldRayCast<T>(T target) => _filters.All(filter => filter.CheckWorldRaycast(target));

		bool IWorldMasterRaycastFilter.CheckWorldEvent(UIInputEvent evt, ITapHandler target) => _filters.All(filter => filter.CheckWorldEvent(evt, target));

		bool IWorldMasterRaycastFilter.CheckUIEvent(UIInputEvent evt, PointerEventData arg) => _filters.All(filter => filter.CheckUIEvent(evt, arg));
	}

}