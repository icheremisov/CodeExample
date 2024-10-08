// ReSharper disable CheckNamespace

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class ExceptionExtensions {
	public static string ToLog(this Exception ex, string startsWith = null, bool showStackTrace = true) {
		var sb = new StringBuilder(1024);
		
		if (startsWith != null) sb.AppendLine(startsWith);
		else {
			var method = new StackTrace().GetFrame(1)?.GetMethod();
			if (method != null) sb.AppendLine($"{method.DeclaringType?.Name ?? "[NaN]"}.{method.Name}");
		}
		
		sb.AppendLine($"{ex.GetType().Name}: {ex.Message}");
		if (showStackTrace) sb.AppendLine(ex.StackTrace);
		else sb.AppendLine(ex.StackTrace?.Split(Environment.NewLine).FirstOrDefault());
		
		var innerException = ex.InnerException;
		var index = 1;
		while (innerException != null) {
			sb.AppendLine($"----- {index:00} {innerException.GetType().Name}: {innerException.Message}");

			if (showStackTrace) sb.AppendLine(innerException.StackTrace);
			else sb.AppendLine(innerException.StackTrace?.Split(Environment.NewLine).FirstOrDefault());

			innerException = innerException.InnerException;
			++index;
		}

		return sb.ToString();
	}

	public static void LogIfNotOperationCanceled(this Exception exception) {
		if(exception is OperationCanceledException) return;
		UnityEngine.Debug.LogError(exception.ToLog());
	}
}