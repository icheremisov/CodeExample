using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XLib.Configs {

	[Serializable]
	public class UserDataElement {
		public long Id;
		public Color Color;
	}

	[Serializable]
	public class AssetUserData {
		public List<UserDataElement> Elements = new(4);

		public Color? GetColor(Object obj) {
			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var _, out long localeId)) return null;
			return Elements.FirstOrDefault(el => el.Id == localeId)?.Color;
		}

		public void SetColor(Object obj, Color color) {
			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var _, out long localeId)) return;

			var el = Elements.FirstOrDefault(el => el.Id == localeId);
			if (el == null)
				Elements.Add(new UserDataElement() { Id = localeId, Color = color });
			else
				el.Color = color;
		}
	}

}