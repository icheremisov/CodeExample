using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace XLib.Core.Runtime.Extensions {

	[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "CheckNamespace")]
	public class LambdaComparer<T> : IComparer<T> {

		private readonly Comparison<T> _comparision;

		public LambdaComparer(Comparison<T> comparision) {
			_comparision = comparision;
		}

		public int Compare(T x, T y) => _comparision(x, y);

	}

}