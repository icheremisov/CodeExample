using System;
using UnityEditor;

namespace XLib.BuildSystem.GameDefines {

	public partial class CustomDefineManager {
		[Flags]
		public enum CdmBuildTargetGroup {
			Standalone = 1,
			iOS = 2,
			Android = 4,
			//WebGL = 8,
			//Metro = 16,
			//Tizen = 32,
			//PSP2 = 64,
			//PS4 = 128,
			//XBOX360 = 256,
			//XboxOne = 512,
			//SamsungTV = 1024,
			//N3DS = 2048,
			//WiiU = 4096,
			//tvOS = 8192
		}
	}

	public static class BuildTargetGroupExtensions {
		public static BuildTargetGroup ToBuildTargetGroup(this CustomDefineManager.CdmBuildTargetGroup tg) {
			//Debug.Log(tg.ToString());
			return (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), tg.ToString());
		}

		public static string ToIconName(this CustomDefineManager.CdmBuildTargetGroup tg) {
			switch (tg) {
				case CustomDefineManager.CdmBuildTargetGroup.iOS: return "iPhone";
			}

			return tg.ToString();
		}

		/// <summary>
		/// A FX 3.5 way to mimic the FX4 "HasFlag" method.
		/// Frm: http://www.sambeauvois.be/blog/2011/08/enum-hasflag-method-extension-for-4-0-framework/ thanks Sam!
		/// </summary>
		/// <param name="variable">The tested enum.</param>
		/// <param name="value">The value to test.</param>
		/// <returns>True if the flag is set. Otherwise false.</returns>
		public static bool HasFlag(this Enum variable, Enum value) {
			// check if from the same type.
			if (variable.GetType() != value.GetType()) {
				throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
			}

			var num = Convert.ToUInt64(value);
			var num2 = Convert.ToUInt64(variable);

			return (num2 & num) == num;
		}
	}

}