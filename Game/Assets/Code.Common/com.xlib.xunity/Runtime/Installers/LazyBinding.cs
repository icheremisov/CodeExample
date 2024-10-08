using System;
using XLib.Unity.Installers;
using Zenject;

namespace XLib.Unity.Installers {

	public interface ILazyBinding {

		void Resolve(DiContainer container);

	}

	public class LazyBinding<T> : ILazyBinding {
		private T _value;

		public T Value => _value ?? throw new NullReferenceException(typeof(T).Name);
		public T ValueOrNull => _value;
		public bool HasValue => _value != null;

		public void Set(T value) => _value = value;
		public void Resolve(DiContainer container) => _value = container != null ? container.Resolve<T>() : default;

	}

}


public static class LazyBindingExtensions {
	
	public static void InitLazyBindingsTo<T>(this DiContainer container) => container.Resolve<LazyBinding<T>>().Resolve(container);
	public static void ClearLazyBindingsTo<T>(this DiContainer container) => container.Resolve<LazyBinding<T>>().Resolve(null);
}
