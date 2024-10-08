using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace XLib.Core.Reflection {

	// ReSharper disable once UnusedType.Global
	public static class ExpressionExtensions {

		/// <summary>
		///     Convert a lambda expression for a getter into a setter
		/// </summary>
		public static Action<T, TProperty> ToSetter<T, TProperty>(this Expression<Func<T, TProperty>> expression) {
			var memberExpression = (MemberExpression)expression.Body;

			if (memberExpression.Member is PropertyInfo property) {
				var setMethod = property.GetSetMethod();

				var parameterT = Expression.Parameter(typeof(T), "x");
				var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

				var newExpression =
					Expression.Lambda<Action<T, TProperty>>(Expression.Call(parameterT, setMethod, parameterTProperty),
						parameterT,
						parameterTProperty);

				return newExpression.Compile();
			}

			if (memberExpression.Member is FieldInfo field) {
				var parameterT = Expression.Parameter(typeof(T), "x");
				var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

				var fieldExp = Expression.Field(parameterT, field);

				var newExpression =
					Expression.Lambda<Action<T, TProperty>>(Expression.Assign(fieldExp, parameterTProperty),
						parameterT,
						parameterTProperty);

				return newExpression.Compile();
			}

			throw new Exception($"Unknown member type: {memberExpression.Member}");
		}

		/// <summary>
		///     Convert a lambda expression for a getter
		/// </summary>
		public static Func<T, TProperty> ToGetter<T, TProperty>(this Expression<Func<T, TProperty>> expression) => expression.Compile();

		/// <summary>
		///     invoke getter
		/// </summary>
		public static void InvokeSetter<T, TValue>(this Expression<Func<T, TValue>> expression, T x, TValue value) where T : class {
			var members = new List<MemberInfo>(4);

			var exp = expression.Body;
			ConstantExpression ce = null;

			object targetObject = null;

			while (exp != null) {
				if (exp is MemberExpression mi) {
					members.Add(mi.Member);
					exp = mi.Expression;
				}
				else {
					ce = exp as ConstantExpression;

					if (ce == null)
						targetObject = x;
					else
						targetObject = ce.Value;

					break;
				}
			}

			if (members.Count == 0 || targetObject == null) {
				// We need at least a getter
				throw new NotSupportedException();
			}

			// We have to walk the getters from last (most inner) to second
			// (the first one is the one we have to use as a setter)
			for (var i = members.Count - 1; i >= 1; i--) {
				var pi = members[i] as PropertyInfo;

				if (pi != null)
					targetObject = pi.GetValue(targetObject);
				else {
					var fi = (FieldInfo)members[i];
					targetObject = fi.GetValue(targetObject);
				}
			}

			// The first one is the getter we treat as a setter
			{
				var pi = members[0] as PropertyInfo;

				if (pi != null)
					pi.SetValue(targetObject, value);
				else {
					var fi = (FieldInfo)members[0];
					fi.SetValue(targetObject, value);
				}
			}
		}

		/// <summary>
		///     invoke getter
		/// </summary>
		public static TValue InvokeGetter<T, TValue>(this Expression<Func<T, TValue>> expression, T x) where T : class {
			var members = new List<MemberInfo>(4);

			var exp = expression.Body;
			ConstantExpression ce = null;

			object targetObject = null;

			while (exp != null) {
				var mi = exp as MemberExpression;

				if (mi != null) {
					members.Add(mi.Member);
					exp = mi.Expression;
				}
				else {
					ce = exp as ConstantExpression;

					if (ce == null)
						targetObject = x;
					else
						targetObject = ce.Value;

					break;
				}
			}

			if (members.Count == 0 || targetObject == null) {
				// We need at least a getter
				throw new NotSupportedException();
			}

			// We have to walk the getters from last (most inner) to second
			// (the first one is the one we have to use as a setter)
			for (var i = members.Count - 1; i >= 1; i--) {
				var pi = members[i] as PropertyInfo;

				if (pi != null)
					targetObject = pi.GetValue(targetObject);
				else {
					var fi = (FieldInfo)members[i];
					targetObject = fi.GetValue(targetObject);
				}
			}

			// The first one is the getter we treat as a setter
			{
				var pi = members[0] as PropertyInfo;

				if (pi != null) return (TValue)pi.GetValue(targetObject);
				var fi = (FieldInfo)members[0];
				return (TValue)fi.GetValue(targetObject);
			}
		}

	}

}