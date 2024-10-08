using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

namespace XLib.Core.Utils {

	public static class TraceLog {
		public static string MethodName(int skipFrame = 0) {
			var frame = new StackTrace().GetFrame(skipFrame + 1);
			var method = frame.GetMethod();
			if (method.ReflectedType != null) {
				if (typeof(System.Runtime.CompilerServices.IAsyncStateMachine).IsAssignableFrom(method.ReflectedType)) {
					var name = method.ReflectedType.Name.Split(new[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
					return $"{method.ReflectedType.DeclaringType?.Name}.{name}";
				}

				return $"{method.ReflectedType.Name}.{method}";
			}

			return method.Name;
		}

		public static string Method => MethodName(0);

		public readonly struct TraceLogUsage : IDisposable {
#if DEVELOPMENT_BUILD
			private readonly Stopwatch _watcher;
			private readonly string _message;
			private readonly bool _verbose;
#endif

			public TraceLogUsage(string msg, bool verbose) {
#if DEVELOPMENT_BUILD
				_watcher = Stopwatch.StartNew();
				_message = msg ?? $"[{MethodName(3)}]";
				_verbose = verbose;

				if (_verbose) Debug.Log($">>>>> {_message} : {DateTime.UtcNow:O}");
#endif
			}

			[Conditional("DEVELOPMENT_BUILD")]
			public void Pause() {
#if DEVELOPMENT_BUILD
				_watcher.Stop();
				if (_verbose) Debug.Log($"   << PAUSE {_message} : {_watcher.ElapsedMilliseconds}ms {DateTime.UtcNow:O}");
#endif
			}

			[Conditional("DEVELOPMENT_BUILD")]
			public void Unpause() {
#if DEVELOPMENT_BUILD
				_watcher.Start();
				if (_verbose) Debug.Log($"   >> UNPAUSE {_message} : {_watcher.ElapsedMilliseconds}ms {DateTime.UtcNow:O}");
#endif
			}

			public void Dispose() {
#if DEVELOPMENT_BUILD
				_watcher.Stop();
				Debug.Log(_verbose
					? $"<<<<< {_message} : {_watcher.ElapsedMilliseconds}ms {DateTime.UtcNow:O}"
					: $"{_message} : {_watcher.ElapsedMilliseconds}ms");
#endif
			}
		}

		public static TraceLogUsage Usage(string message) => new TraceLogUsage(message, false);
		public static TraceLogUsage Usage<T>(string message) => Usage($"[{typeof(T).Name}] {message}");
		public static TraceLogUsage Usage() => Usage((string)null);
		public static TraceLogUsage Usage(params string[] messages) => Usage(string.Join(" ", messages));

		public static TraceLogUsage Verbose(string message) => new TraceLogUsage(message, true);
		public static TraceLogUsage Verbose<T>(string message) => Verbose($"[{typeof(T).Name}] {message}");
		public static TraceLogUsage Verbose() => Verbose((string)null);
		public static TraceLogUsage Verbose(params string[] messages) => Verbose(string.Join(" ", messages));

		public static string Print<T>(this IEnumerable<T> source, Func<T, string> print = null) {
			var stringBuilder = new StringBuilder(1024);
			stringBuilder.Append("[");
			stringBuilder.Append(print != null ? source.Select(print).JoinToString() : source.JoinToString());
			stringBuilder.Append("]");

			return stringBuilder.ToString();
		}

		public static string Print(this IEnumerable source, Func<object, string> print = null) {
			var stringBuilder = new StringBuilder(1024);
			stringBuilder.Append("[");

			foreach (var v in source) {
				if (print != null)
					stringBuilder.Append(print(v));
				else
					stringBuilder.Append(v);

				stringBuilder.Append(", ");
			}

			stringBuilder.Append("]");
			return stringBuilder.ToString().Replace(", ]", "]");
		}
	}

}