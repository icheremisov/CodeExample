using System;
using UnityEngine;

namespace XLib.Configs.Contracts {

	[Serializable]
	public struct SpreadsheetId : IComparable<SpreadsheetId> {
		[SerializeField] private string _id;

		public SpreadsheetId(string id) => _id = id;

		public bool IsEnable => !string.IsNullOrEmpty(_id);

		public int CompareTo(SpreadsheetId other) => string.Compare(_id, other._id, StringComparison.Ordinal);

		public bool Equals(string id) => _id == id;

		public bool Equals(SpreadsheetId other) => _id == other._id;

		public override bool Equals(object obj) => obj is SpreadsheetId other && Equals(other);

		public override int GetHashCode() => _id != null ? _id.GetHashCode() : 0;

		public static bool operator ==(SpreadsheetId a, SpreadsheetId b) => a._id == b._id;

		public static bool operator !=(SpreadsheetId a, SpreadsheetId b) => !(a == b);
		public static implicit operator string(SpreadsheetId id) => id.ToString();
		public override string ToString() => _id;
	}

}