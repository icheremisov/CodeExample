using Client.Core.Ecs.Types;
using Entitas;

namespace Client.Ecs.Core.Contracts {

	/// <summary>
	///     List of all features.
	///     Can add/remove features.
	///     Execute all started features each frame.
	/// </summary>
	public interface IEcsRunner {

		/// <summary>
		///     pause all updates of all systems until it resumed.
		///     IEcsRunner started in paused state and unpaused when level is loaded
		/// </summary>
		bool IsPaused { get; set; }

		/// <summary>
		///     add systems to feature. Create feature if it not exists
		///     if feature already started, initialize systems
		/// </summary>
		void Add(FeatureId id, params ISystem[] systems);

		/// <summary>
		///     register feature
		/// </summary>
		void Set(FeatureId id, Feature systems);

		/// <summary>
		///     call Stop for this feature and remove all systems from it
		/// </summary>
		void Destroy(FeatureId id);

		/// <summary>
		///     start all systems in feature
		/// </summary>
		void Start(FeatureId id);

		/// <summary>
		///     manually update all systems
		/// </summary>
		void ForceUpdate(FeatureId id);

		/// <summary>
		///     stop all systems in feature
		/// </summary>
		void Stop(FeatureId id);

	}

}