using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Utils;
using Object = UnityEngine.Object;

namespace XLib.Configs {

	[CustomEditor(typeof(ScriptableObjectContainer), true), CanEditMultipleObjects]
	public class ScriptableObjectContainerEditor : ContainerEditor {

		protected override Type GetElementType(Type container) {
			var parent = typeof(ScriptableObjectContainer<>);

			var elementType = container;
			while (!elementType.IsGenericType || elementType.GetGenericTypeDefinition() != parent) elementType = elementType.BaseType;

			return elementType.GetGenericArguments()[0];
		}

		protected override UniTask<bool> AddChild(Object asset) {
			asset.name = $"{target.name} - {asset.GetType()}";
			AssetDatabase.AddObjectToAsset(asset, target);
			AssetDatabase.Refresh();
			return UniTask.FromResult(true);
		}

		protected override string GetHeaderLabel(Object obj) => $"{base.GetHeaderLabel(obj)} [{((ScriptableObject)obj).name}]";
	}

}