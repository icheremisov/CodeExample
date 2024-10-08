using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using XLib.Core.Reflection;

namespace XLib.Unity.Extensions {

	public static class OdinUtils {
		public static void AddMethod(
			this IList<InspectorPropertyInfo> infos,
			Action method, params Attribute[] attr) {
			infos.AddDelegate(method.Method.Name, method, method.Method
				.GetAttributes<Attribute>().Concat(attr).ToArray());
		}

		public static void SetValue<T>(this InspectorProperty property, T value) => 
			property.ValueEntry.WeakSmartValue = value;

		public static T GetValue<T>(this InspectorProperty property) => 
			property.ValueEntry.WeakSmartValue is T value ? value : default;
	
		public static void SetPropertyValue<T>(this InspectorProperty property, string name, T value) => 
			property.Children[name].SetValue(value);

		public static T GetPropertyValue<T>(this InspectorProperty property, string name) => 
			property.Children[name].GetValue<T>();


		public static void AddMenu(this GenericMenu menu, string name, Action apply, bool enable = true) {
			if (enable) 
				menu.AddItem(new GUIContent(name), false, () => apply());
			else menu.AddDisabledItem(new GUIContent(name), false);
		}
		
		private static MethodInfo _renderStaticPreview;

		public static Texture2D RenderStaticPreview(Sprite sprite, Color color, int width, int height) {
			// usign UnityEditor.SpriteUtility.RenderStaticPreview
			if (_renderStaticPreview == null) {
				var typeName = "UnityEditor.SpriteUtility";
				var t = Type.GetType(typeName);
				if (t == null) {
					var currentAssembly = Assembly.GetExecutingAssembly();
					var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
					foreach (var assemblyName in referencedAssemblies)
					{
						var assembly = Assembly.Load(assemblyName);
						if (assembly == null) continue;
						t = assembly.GetType(typeName);
						if (t != null) break;
					}
				}

				_renderStaticPreview = t.GetMethod("RenderStaticPreview", new[] {
					typeof(Sprite),
					typeof(Color),
					typeof(int),
					typeof(int)
				});
			}

			if (_renderStaticPreview == null) return null;
			return _renderStaticPreview.Invoke(null, new object[] {
				sprite,
				color,
				width,
				height
			}) as Texture2D;
		}
	}

}