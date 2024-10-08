using System;
using System.Collections.Generic;
using System.Linq;
using XLib.Core.RandGen;
#if UNITY_2020_3_OR_NEWER
using UnityEngine;
#endif

namespace XLib.Core.Utils {

	public static class MathEx {

		//
		// Summary:
		//     The infamous 3.14159265358979... value (Read Only).
		public const float PI = 3.14159274F;

		//
		// Summary:
		//     A representation of positive infinity (Read Only).
		public const float Infinity = float.PositiveInfinity;

		//
		// Summary:
		//     A representation of negative infinity (Read Only).
		public const float NegativeInfinity = float.NegativeInfinity;

		//
		// Summary:
		//     Degrees-to-radians conversion constant (Read Only).
		public const float Deg2Rad = 0.0174532924F;

		//
		// Summary:
		//     Radians-to-degrees conversion constant (Read Only).
		public const float Rad2Deg = 57.29578F;

		private const float floatEpsilon = 1e-6f;
		private const double doubleEpsilon = 1e-12;

		public static int Clamp(int x, int min, int max) => Math.Max(min, Math.Min(x, max));

		public static float Clamp(float x, float min, float max) => Math.Max(min, Math.Min(x, max));

		public static float Clamp01(float x) => Math.Max(0.0f, Math.Min(x, 1.0f));

		public static float Frac(float x) => x - (int)x;

		internal static int RoundToInt(float v) => (int)Math.Round(v);

		internal static long RoundToLong(float v) => (long)Math.Round(v);

		public static float Lerp(float min, float max, float k) => (max - min) * k + min;

		public static int FloorToInt(float k) => (int)k;

		public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

		public static T RandomWeight<T>(IList<T> items, IRandom random, Func<T, float> getWeight) where T : class {
			if (items == null) return null;

			var summ = items.Sum(getWeight);

			if (summ <= 0) return null;

			var randValue = random.NextInclusive(0.0f, summ);

			var totalWeight = 0.0f;
			foreach (var item in items) {
				totalWeight += getWeight(item);
				if (totalWeight >= randValue) return item;
			}

			return null;
		}

		public static bool EqualsEpsilon(float a, float b) => Math.Abs(a - b) < floatEpsilon;

		public static bool EqualsEpsilon(double a, double b) => Math.Abs(a - b) < doubleEpsilon;

		/// N-ый член арифметической прогессии
		public static double ArithmeticProgression(double first, double increaseNumber, int index) => first + (index - 1) * increaseNumber;

		/// Сумма n последовательных членов АП
		public static double ArithmeticProgressionSum(double first, double increaseNumber, int startIndex, int count) {
			var startNumber = ArithmeticProgression(first, increaseNumber, startIndex);
			return (startNumber * 2 + increaseNumber * (count - 1)) / 2 * count;
		}

		/// N-ый член геометрической прогессии
		public static double GeometricProgression(double first, double multiplierNumber, int index) => first * Math.Pow(multiplierNumber, index - 1);

		/// Сумма n последовательных членов ГП
		public static double GeometricProgressionSum(double first, double multiplierNumber, int startIndex, int count) {
			var startNumber = GeometricProgression(first, multiplierNumber, startIndex);
			double summ;
			if (multiplierNumber == 1.0)
				summ = startNumber * count;
			else
				summ = startNumber * ((Math.Pow(multiplierNumber, count) - 1) / (multiplierNumber - 1));

			return summ;
		}

		// https://stackoverflow.com/questions/109023/count-the-number-of-set-bits-in-a-32-bit-integer
		// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs#L458
		public static uint NumberOfSetBits(uint i)
		{ 
			i -= ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
		}

		public static ulong NumberOfSetBits(ulong i)
		{
			i -= (i >> 1) & 0x_55555555_55555555ul;
			i = (i & 0x_33333333_33333333ul) + ((i >> 2) & 0x_33333333_33333333ul);
			return (((i + (i >> 4)) & 0x_0F0F0F0F_0F0F0F0Ful) * 0x_01010101_01010101ul) >> 56;
		}
		
#if UNITY_2020_3_OR_NEWER
		public static float Parabola1D(float height, float t) => -4 * height * t * t + 4 * height * t;

		public static Vector2 Parabola2D(Vector2 start, Vector2 end, float height, float t) {
			var mid = Vector2.Lerp(start, end, t);
			return new Vector2(mid.x, Parabola1D(height, t) + Mathf.Lerp(start.y, end.y, t));
		}
#endif

	}

}