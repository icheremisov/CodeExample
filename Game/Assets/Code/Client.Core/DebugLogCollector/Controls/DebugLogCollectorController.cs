#if FEATURE_CONSOLE || FEATURE_CHEATS || UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Core.DebugLogCollector.Contracts;
using UnityEngine;
using XLib.Core.Utils;
using Zenject;

namespace Client.Core.DebugLogCollector.Controls {

	public class DebugLogCollectorController : IDebugLogCollector, IInitializable {
		private const string StrRemove = "Cysharp.";
		private static StringBuilder _sb = new();

		private struct LogMessageInfo {
			public DateTime LastTime { get; set; }
			public int CollapseCount { get; set; }
		}

		private readonly Dictionary<string, LogMessageInfo> _logErrors = new();
		private readonly List<string> _logAll = new();

		public int TotalErrors {
			get {
				lock (_logErrors) return _logErrors.Count;
			}
		}

		public string FirstError {
			get {
				lock (_logErrors) {
					if (_logErrors == null || _logErrors.Count == 0) return string.Empty;
					var error = _logErrors.MinBy(x => x.Value.LastTime);
					var eol = error.Key.IndexOfAny(new[] { '\r', '\n' });
					var errorFirstString = eol < 0 ? error.Key : error.Key[..eol];
					return $"[{error.Value.LastTime:o}] Count: {error.Value.CollapseCount}\n{errorFirstString}";
				}
			}
		}

		public void Initialize() {
			Application.logMessageReceivedThreaded += OnLogCallback;
		}

		private void OnLogCallback(string logString, string trace, LogType type) {
			int? collapseCount = null;

			if (type is LogType.Error or LogType.Exception or LogType.Assert) {
				lock (_logErrors) {
					if (!trace.IsNullOrEmpty()) {
						trace = GetClearTrace(trace);
					}

					var logMessage = $"<color=red>{logString}</color>\n{trace}";

					if (_logErrors.TryGetValue(logMessage, out var duplicate)) {
						duplicate.CollapseCount++;
						collapseCount = duplicate.CollapseCount;
						_logErrors[logMessage] = duplicate;
					}
					else
						_logErrors.TryAdd(logMessage, new LogMessageInfo { LastTime = DateTime.Now, CollapseCount = 1 });
				}
			}

			if (collapseCount is >= 10) return;

			var logTypeStr = type switch {
				LogType.Error     => "[E]",
				LogType.Assert    => "[E]",
				LogType.Warning   => "[W]",
				LogType.Log       => "[D]",
				LogType.Exception => "[E]",
				_                 => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};

			lock (_logAll) _logAll.Add($"{logTypeStr}[{DateTime.Now:o}]:{RichTag.StripTags(logString)}");
		}

		
		private static string GetClearTrace(string trace) {
			_sb.Clear();
			var charIdx = 0;

			while (charIdx < trace.Length) {
				var lineStartIdx = charIdx;
				if (trace[charIdx] == StrRemove[0]) {
					charIdx++;
					var contains = true;
					for (var i = 1; i < StrRemove.Length; i++) {
						if (trace[charIdx] == StrRemove[i]) {
							charIdx++;
							continue;
						}

						contains = false;
						break;
					}

					if (contains) {
						charIdx = trace.IndexOf('\n', charIdx) + 1;
						continue;
					}
				}

				var lineEndIdx = trace.IndexOf('\n', charIdx) + 1;

				for (var i = lineStartIdx; i < lineEndIdx; i++) _sb.Append(trace[i]);

				charIdx = lineEndIdx;
			}

			return _sb.ToString();
		}

		public void ClearLog() {
			lock (_logErrors) _logErrors.Clear();
			lock (_logAll) _logAll.Clear();
		}

		public string GetLog() {
			lock (_logAll) return string.Join("\n", _logAll);
		}

		public string GetErrorLog() {
			lock (_logErrors) return string.Join("\n", _logErrors.OrderBy(x => x.Value.LastTime).Select(ToCombinedString));
		}

		private string ToCombinedString(KeyValuePair<string, LogMessageInfo> logMessagePair) {
			return $"[{logMessagePair.Value.LastTime:o}] Count: {logMessagePair.Value.CollapseCount}\n{logMessagePair.Key}";
		}
	}

}

#endif