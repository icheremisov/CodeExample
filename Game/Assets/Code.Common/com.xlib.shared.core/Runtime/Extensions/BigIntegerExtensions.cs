using System;
using System.Numerics;

// ReSharper disable once CheckNamespace
public static class BigIntegerExtensions {

	public static double Divide(this BigInteger source, BigInteger target) => Math.Exp(BigInteger.Log(source) - BigInteger.Log(target));

	public static double ToDouble(this BigInteger source) => (double)source;

}