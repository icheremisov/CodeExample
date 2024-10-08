using Entitas;

namespace Client.Entitas.Global.Time {

	public class TimeSystem : IInitializeSystem, IExecuteSystem {
		private readonly GlobalContext _context;

		public TimeSystem(GlobalContext context) {
			_context = context;
		}
		
		public void Initialize() {
			_context.ReplaceTime(UnityEngine.Time.deltaTime, 0.0f);
		}
		
		public void Execute() {
			var time = _context.time;
			time.Delta = UnityEngine.Time.deltaTime;
			time.Time += time.Delta;
		}
	}

}