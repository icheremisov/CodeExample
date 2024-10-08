using System;
using UnityEngine;

namespace Client.Core.Ecs.Types {

	/// <summary>
	///     ID for ECS systems container (Feature)
	/// </summary>
	[Serializable]
	public struct FeatureId : IComparable<FeatureId> {

		[SerializeField] private string _id;

		public FeatureId(string id) {
			if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

			_id = id;
		}

		public int CompareTo(FeatureId other) => string.Compare(_id, other._id, StringComparison.Ordinal);

		public bool Equals(string id) => _id == id;

		public bool Equals(FeatureId other) => _id == other._id;

		public override bool Equals(object obj) => obj is FeatureId other && Equals(other);

		public override int GetHashCode() => _id != null ? _id.GetHashCode() : 0;

		public static bool operator ==(FeatureId a, FeatureId b) => a._id == b._id;

		public static bool operator !=(FeatureId a, FeatureId b) => !(a == b);

		public override string ToString() => _id;

	}

}