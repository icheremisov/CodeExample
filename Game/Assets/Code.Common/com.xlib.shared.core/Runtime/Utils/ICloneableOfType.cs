using System;

namespace XLib.Core.Utils {

	public interface ICloneableOfType<out T> : ICloneable {

		new T Clone();

		object ICloneable.Clone() => Clone();

	}

}