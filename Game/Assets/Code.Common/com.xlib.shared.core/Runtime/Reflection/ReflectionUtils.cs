using System.Reflection;

namespace XLib.Core.Reflection {

	public static class ReflectionUtils {

		public static T GetFieldOrPropertyValue<T>(string fieldName, object obj, bool includeAllBases = false,
			BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
			var field = obj.GetType().GetField(fieldName, bindings);
			if (field != null) return (T)field.GetValue(obj);

			var property = obj.GetType().GetProperty(fieldName, bindings);
			if (property != null) return (T)property.GetValue(obj, null);

			if (includeAllBases) {
				foreach (var type in obj.GetType().GetBaseClassesAndInterfaces()) {
					field = type.GetField(fieldName, bindings);
					if (field != null) return (T)field.GetValue(obj);

					property = type.GetProperty(fieldName, bindings);
					if (property != null) return (T)property.GetValue(obj, null);
				}
			}

			return default;
		}

		public static bool SetFieldOrPropertyValue(string fieldName, object obj, object value, bool includeAllBases = false,
			BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
			var field = obj.GetType().GetField(fieldName, bindings);
			if (field != null) {
				field.SetValue(obj, value);
				return true;
			}

			var property = obj.GetType().GetProperty(fieldName, bindings);
			if (property != null) {
				property.SetValue(obj, value, null);
				return true;
			}

			if (includeAllBases) {
				foreach (var type in obj.GetType().GetBaseClassesAndInterfaces()) {
					field = type.GetField(fieldName, bindings);
					if (field != null) {
						field.SetValue(obj, value);
						return true;
					}

					property = type.GetProperty(fieldName, bindings);
					if (property != null) {
						property.SetValue(obj, value, null);
						return true;
					}
				}
			}

			return false;
		}

	}

}