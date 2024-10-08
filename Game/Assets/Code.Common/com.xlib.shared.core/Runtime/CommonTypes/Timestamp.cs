using System;
using Newtonsoft.Json;
using XLib.Core.Json.Net;

namespace XLib.Core.CommonTypes {

	[Serializable, JsonConverter(typeof(TimestampConverter))]
	public struct Timestamp : IComparable<Timestamp> {
		public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// ReSharper disable once InconsistentNaming
		public int Value;

		public Timestamp(int value) {
			Value = value;
		}

		public Timestamp(DateTime value) {
			Value = (int)new DateTimeOffset(value).ToUnixTimeSeconds();
		}

		public static Timestamp operator +(Timestamp a, Duration b) => new(a.Value + b.Value);
		public static Timestamp operator -(Timestamp a, Duration b) => new(a.Value - b.Value);
		public static Duration operator -(Timestamp a, Timestamp b) => new(a.Value - b.Value);
		public static bool operator >(Timestamp a, Timestamp b) => a.Value > b.Value;
		public static bool operator >=(Timestamp a, Timestamp b) => a.Value >= b.Value;
		public static bool operator <(Timestamp a, Timestamp b) => a.Value < b.Value;
		public static bool operator <=(Timestamp a, Timestamp b) => a.Value <= b.Value;
		public static bool operator ==(Timestamp a, Timestamp b) => a.Value == b.Value;
		public static bool operator !=(Timestamp a, Timestamp b) => a.Value != b.Value;
		public static Duration operator %(Timestamp a, Duration b) => new(a.Value % b.Value);

		public static Timestamp DayStart(int day, Duration timezone = default) => new(day * Duration.DaySeconds - timezone.Value);

		public static Timestamp DeviceTime => new((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

		public static readonly Timestamp Null = new(0);
		public static readonly Timestamp Never = new(int.MaxValue);

		public int Day(Duration timezone = default) => (Value + timezone.Value) / Duration.DaySeconds;
		public bool Passed(ITimeProvider provider) => Value == 0 || this <= provider.CurrentTime;
		public Duration TimeLeft(ITimeProvider provider) => this - provider.CurrentTime;
		public bool IsNull => Value == 0;
		public bool IsNever => Value == int.MaxValue;
		public bool IsValid => !IsNull && !IsNever;
		public double AsJsTimestamp => (double)Value * 1000;

		public static explicit operator int(Timestamp ts) => ts.Value;

		public int CompareTo(Timestamp other) => Value.CompareTo(other.Value);
		public bool Equals(Timestamp other) => Value == other.Value;

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Timestamp other && Equals(other);
		}
		public override int GetHashCode() => Value;
		public DateTime ToDateTime => DateTimeOffset.FromUnixTimeSeconds(Value).UtcDateTime;
		public DateTime ToLocalDateTime => DateTimeOffset.FromUnixTimeSeconds(Value).LocalDateTime;
		public Timestamp Add(TimeSpan other) => new(Value + (int)other.TotalSeconds);

		public override string ToString() {
			if (Value == 0) return "-";
			if (Value == int.MaxValue) return "Never";

			return UnixEpoch.AddSeconds(Value).ToLocalTime().ToString("R");
		}

		public static Timestamp Min(Timestamp a, Timestamp b) => a.Value < b.Value ? a : b;

		public static Timestamp Max(Timestamp a, Timestamp b) => a.Value > b.Value ? a : b;
	}

}