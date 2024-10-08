#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using XLib.BuildSystem.Exceptions;

namespace XLib.BuildSystem.Types {

	public class RunnerReport {

		public Logger Logger { get; }
		public bool HasErrors => _errors.Count > 0;

		public struct ErrorInfo {
			public string Message { get; }

			public ErrorInfo(string message) {
				Message = message;
			}

			public ErrorInfo(Exception ex) {
				Message = ex.ToString();
			}
		}

		private readonly List<ErrorInfo> _errors = new();

		public RunnerReport(Logger logger) {
			Logger = logger;
		}

		public void ReportError(string error, bool @throw = true) {
			_errors.Add(new ErrorInfo(error));
			
			if (@throw) ThrowOnError();
		}

		public void ReportError(Exception ex, bool @throw = true) {
			while (ex != null) {
				_errors.Add(new ErrorInfo(ex));
				ex = ex.InnerException;
			}
			
			if (@throw) ThrowOnError();
		}

		public void ThrowOnError() {
			if (_errors.Count > 0) throw new BuildErrorsException(this);
		}

		public void DumpErrors() {
			if (_errors.Count == 0) return;
			
			Logger.LogError($"Error count: {_errors.Count}");
			
			foreach (var errorInfo in _errors) {
				Logger.LogError(errorInfo.Message);
			}
		}
		
		public string GetErrorString() {
			if (_errors.Count == 0) return "";

			var sb = new StringBuilder(1024);
			sb.AppendLine($"Error count: {_errors.Count}");
			
			foreach (var errorInfo in _errors) {
				sb.AppendLine(errorInfo.Message);
			}

			return sb.ToString();
		}
		
	}

}

#endif