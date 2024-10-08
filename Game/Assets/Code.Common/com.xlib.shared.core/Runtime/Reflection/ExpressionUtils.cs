using System;
using System.Linq.Expressions;
using System.Reflection;

namespace XLib.Core.Reflection {

	public static class ExpressionUtils {

		/// <summary>
		///     get property for expression
		///     ExpressionUtils.GetProperty(x => x.name)
		/// </summary>
		public static PropertyInfo GetProperty<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression) {
			var member = GetMemberExpression(expression).Member;
			var property = member as PropertyInfo;
			if (property == null) throw new InvalidOperationException(string.Format("Member with Name '{0}' is not a property.", member.Name));

			return property;
		}

		private static MemberExpression GetMemberExpression<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression) {
			MemberExpression memberExpression = null;
			if (expression.Body.NodeType == ExpressionType.Convert) {
				var body = (UnaryExpression)expression.Body;
				memberExpression = body.Operand as MemberExpression;
			}
			else if (expression.Body.NodeType == ExpressionType.MemberAccess) memberExpression = expression.Body as MemberExpression;

			if (memberExpression == null) throw new ArgumentException("Not a member access", "expression");

			return memberExpression;
		}

		/// <summary>
		///     create setter for expression. ex.:
		///     ExpressionUtils.CreateSetter(x => x.name)
		/// </summary>
		public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property) {
			var propertyInfo = GetProperty(property);

			var instance = Expression.Parameter(typeof(TEntity), "instance");
			var parameter = Expression.Parameter(typeof(TProperty), "param");

			var body = Expression.Call(instance, propertyInfo.GetSetMethod(), parameter);
			var parameters = new[] { instance, parameter };

			return Expression.Lambda<Action<TEntity, TProperty>>(body, parameters).Compile();
		}

		/// <summary>
		///     create setter for expression. ex.:
		///     ExpressionUtils.CreateGetter(x => x.name)
		/// </summary>
		public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property) {
			var propertyInfo = GetProperty(property);

			var instance = Expression.Parameter(typeof(TEntity), "instance");

			var body = Expression.Call(instance, propertyInfo.GetGetMethod());
			var parameters = new[] { instance };

			return Expression.Lambda<Func<TEntity, TProperty>>(body, parameters).Compile();
		}

		public static Func<TEntity> CreateDefaultConstructor<TEntity>() {
			var body = Expression.New(typeof(TEntity));
			var lambda = Expression.Lambda<Func<TEntity>>(body);

			return lambda.Compile();
		}

	}

}