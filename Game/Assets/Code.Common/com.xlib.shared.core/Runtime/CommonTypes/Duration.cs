using System;
using Newtonsoft.Json;
using UnityEngine.Serialization;
using XLib.Core.Json.Net;
using XLib.Core.Utils;

namespace XLib.Core.CommonTypes {

	[Serializable, JsonConverter(typeof(DurationConverter))]
	public struct Duration {
        public const int MinuteSeconds = 60;
        public const int HourSeconds = 3600;
        public const int DaySeconds = 86400;

		[FormerlySerializedAs("value")] public int Value;
        public Duration(int value) => Value = value;
		public int S => Value % 60;
		public int M => (Value / MinuteSeconds) % 60;
		public int H => (Value / HourSeconds) % 24;
		public int D => Value / DaySeconds;
		public int TotalSeconds => Value;
		public string SS => S.ToString("00");
		public string MM => M.ToString("00");
		public string HH => H.ToString("00");
		public string HHH => ((int) (Value / HourSeconds)).ToString("00");

        public static Duration operator +(Duration a, Duration b) => new(a.Value + b.Value);
        public static Duration operator -(Duration a, Duration b) => new(a.Value - b.Value);
        public static Duration operator *(Duration a, int b) => new(a.Value * b);
        public static Duration operator *(int a, Duration b) => new(a * b.Value);
        public static Duration operator *(Duration a, float b) => new((int)(a.Value * b));
        public static Duration operator *(float a, Duration b) => new((int)(a * b.Value));
        public static int operator /(Duration a, Duration b) => a.Value <= 0 ? 0 : a.Value / b.Value;
        public static Duration operator /(Duration a, int b) => new(a.Value / b);
        public static Duration operator %(Duration a, Duration b) => new(a.Value % b.Value);
        public static Duration operator -(Duration a) => new(-a.Value);
        public int DivideRoundUp(Duration b) => Value <= 0 ? 0 : (Value - 1) / b.Value + 1;
        
        public static bool operator >(Duration a, Duration b) => a.Value > b.Value;
        public static bool operator >=(Duration a, Duration b) => a.Value >= b.Value;
        public static bool operator <(Duration a, Duration b) => a.Value < b.Value;
        public static bool operator <=(Duration a, Duration b) => a.Value <= b.Value;
        public static bool operator ==(Duration a, Duration b) => a.Value == b.Value;
        public static bool operator !=(Duration a, Duration b) => a.Value != b.Value;
        
        public static readonly Duration Zero = new(0);
        public static readonly Duration OneSecond = new(1);
        public static readonly Duration OneMinute = new(MinuteSeconds);
        public static readonly Duration OneHour = new(HourSeconds);
        public static readonly Duration OneDay = new(DaySeconds);

        public static Duration Seconds(int amount) => new(amount);
        public static Duration Minutes(int amount) => new(amount * MinuteSeconds);
        public static Duration Hours(int amount) => new(amount * HourSeconds);
        public static Duration Days(int amount) => new(amount * DaySeconds);

        public static Duration Lerp(Duration a, Duration b, float p) => (a * (1 - p) + b * p);
		public static Duration Clamp(Duration v, Duration a, Duration b) => new (Math.Clamp(v.Value, a.Value, b.Value));
		public Duration Clamp0Max() => new (Math.Clamp(Value, 0, int.MaxValue));

        public static explicit operator int(Duration dur) => dur.Value;
        
        public TimeSpan ToTimeSpan => TimeSpan.FromSeconds(Value);
        
        public bool Equals(Duration other) => Value == other.Value;

        public override bool Equals(object obj) => 
			!ReferenceEquals(null, obj) && obj is Duration other && Equals(other);

        public override int GetHashCode() => Value;

        public override string ToString() => ToString(true);
		
		public static Duration Min(Duration a, Duration b) => a.Value < b.Value ? a : b;
		
		public static Duration Max(Duration a, Duration b) => a.Value > b.Value ? a : b;

		public string ToString(bool removeZeros, string zero = null, string template = null) {
				var seconds = Value;
				if (seconds <= 0) return zero ?? "0s";
				var days = seconds / DaySeconds;
				seconds -= days * DaySeconds;
				var hours = seconds / HourSeconds;
				if (days > 0) {
					if (hours == 0 && removeZeros) return $"{days}d";
					return $"{days}d {hours}h";
				}

				seconds -= hours * HourSeconds;
				var minutes = seconds / MinuteSeconds;
				if (hours > 0) {
					if (minutes == 0 && removeZeros) return $"{hours}h";
					return !string.IsNullOrEmpty(template) ? string.Format(template, hours / 10, hours % 10, minutes / 10, minutes % 10) : $"{hours}h {minutes}m";
				}

				seconds -= minutes * MinuteSeconds;
				if (seconds == 0 && removeZeros) return $"{minutes}m";
				return !string.IsNullOrEmpty(template) ? string.Format(template, minutes / 10, minutes % 10, seconds / 10, seconds % 10) : $"{minutes}m {seconds}s";
		}
	}

}