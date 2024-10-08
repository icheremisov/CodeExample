namespace XLib.Core.CommonTypes {

	public struct TimeRange {
		public Timestamp Start { get; set; }
		public Timestamp End { get; set; }

		public Duration Duration => End - Start;

		public static TimeRange Never => new(Timestamp.Never, Timestamp.Never);
		public static TimeRange Infinite => new(Timestamp.Null, Timestamp.Never);
		public static TimeRange Null => new(Timestamp.Null, Timestamp.Null);

		public bool Passed(ITimeProvider provider) => End.Passed(provider);

		public Duration TimePassed(ITimeProvider provider) {
			var now = provider.CurrentTime;
			if (now < Start) return Duration.Zero;
			if (now > End) return End - Start;
			return now - Start;
		}

		public Duration TimeLeft(ITimeProvider provider) {
			var now = provider.CurrentTime;
			if (now > End) return Duration.Zero;
			return End - now;
		}

		public bool IsInfinity => Start.IsNull || Start.IsNever || End.IsNull || End.IsNever;

		public TimeRange(Timestamp start, Timestamp end) {
			Start = start;
			End = end;
		}

		public static TimeRange FromStart(Timestamp start, Duration duration) => new(start, start + duration);
		public static TimeRange FromEnd(Timestamp end, Duration duration) => new(end - duration, end);
		public static TimeRange FromNowTo(Timestamp end, ITimeProvider timeProvider) => new(timeProvider.CurrentTime, end);

		public static TimeRange FromNow(Duration duration, ITimeProvider timeProvider) {
			var now = timeProvider.CurrentTime;
			return new TimeRange(now, now + duration);
		}

		public override string ToString() => $"start: {Start}; end: {End}";
		public bool Contains(Timestamp time) => Start <= time && End >= time;
		public bool Overlaps(TimeRange range) => range.End > Start && range.Start < End;
		public bool Overlaps(Timestamp start, Timestamp end) => end > Start && start < End;
		public TimeRange IntersectWith(TimeRange range) => new(Timestamp.Max(Start, range.Start), Timestamp.Min(End, range.End));
		public TimeRange IntersectWith(Timestamp start, Timestamp end) => new(Timestamp.Max(Start, start), Timestamp.Min(End, end));
	}

}