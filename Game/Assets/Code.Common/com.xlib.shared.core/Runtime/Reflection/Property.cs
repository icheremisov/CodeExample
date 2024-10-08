using System;
using System.Linq.Expressions;

namespace XLib.Core.Reflection {

	/// <summary>
	///     Dynamic property wrapper
	///     Usage:
	///     prop = new Property&lt;GameObject, string&gt;(x => x.name)
	///     var name = prop.Get(gameObject)
	///     prop.Set(gameObject, "test")
	/// </summary>
	public class Property<TInstance, TArg> {

		private readonly Func<TInstance, TArg> _getter;
		private readonly Action<TInstance, TArg> _setter;

		public Property(Expression<Func<TInstance, TArg>> expr) {
			_getter = ExpressionUtils.CreateGetter(expr);
			_setter = ExpressionUtils.CreateSetter(expr);
		}

		public TArg Get(TInstance instance) => _getter(instance);

		public void Set(TInstance instance, TArg arg) {
			_setter(instance, arg);
		}

	}

}