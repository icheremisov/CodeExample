namespace XLib.Core.Utils {

	public class ValueAsRef<T> where T : struct {
		public T Value { get; set; }

		public ValueAsRef(T value) => Value = value;
	}

}