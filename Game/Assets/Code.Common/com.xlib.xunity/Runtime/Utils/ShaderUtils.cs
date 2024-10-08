using UnityEngine;

namespace XLib.Unity.Utils {

	public static class ShaderUtils {

		public static void CopyColorFrom(this Material target, int propertyId, Material src) {
			if (target.HasProperty(propertyId) && src.HasProperty(propertyId)) target.SetColor(propertyId, src.GetColor(propertyId));
		}

		public static void CopyFloatFrom(this Material target, int propertyId, Material src) {
			if (target.HasProperty(propertyId) && src.HasProperty(propertyId)) target.SetFloat(propertyId, src.GetFloat(propertyId));
		}

	}

}