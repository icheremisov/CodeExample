using System;
using System.Threading;
using UnityEngine;
using XLib.Core.Utils;
using Object = UnityEngine.Object;

/// <summary>
/// log output with tag: "[tag] message"
/// </summary>
// ReSharper disable once CheckNamespace
public class Logger<T> : Logger {
	public Logger(RichLog.Color? color = null, LogLevel level = LogLevel.Info) : base(typeof(T).Name, color, level) { }
}

public class Logger {

	public enum LogLevel {
		Info = 0, 
		Warning = 1,
		Error = 2,
		None = 3
	}

	protected readonly string _tag;
	private LogLevel _level;
	private readonly RichLog.Color _color;
	

	private static int _nextColor = -1;

	public Logger(string tag, RichLog.Color? color = null, LogLevel level = LogLevel.Info) {
		_tag = tag;
		_level = level;
		_color = color ?? NextColor();
	}

	private static RichLog.Color NextColor() {
		var colorIndex = Interlocked.Increment(ref _nextColor);
		var colors = Enums.Values<RichLog.Color>();
		return colors[colorIndex % colors.Length];
	}

	public void Log(string message, Object obj = null) {
		if (_level <= LogLevel.Info) Debug.Log($"[{_tag.WithColor(_color)}] {message}", obj);
	}

	public void LogWarning(string message, Object obj = null) {
		if(_level <= LogLevel.Warning) Debug.LogWarning($"[{_tag.WithColor(_color)}] {message}", obj);
	}

	public void LogError(string message, Object obj = null) {
		if(_level <= LogLevel.Error) Debug.LogError($"[{_tag.WithColor(_color)}] {message}", obj);
	}

	public void Log(FormattableString message, Object obj = null) {
		if(_level <= LogLevel.Info) Debug.Log($"[{_tag.WithColor(_color)}] {message.ToString(RichLog.Default)}", obj);
	}

	public void LogWarning(FormattableString message, Object obj = null) {
		if(_level <= LogLevel.Warning) Debug.LogWarning($"[{_tag.WithColor(_color)}] {message.ToString(RichLog.Default)}", obj);
	}

	public void LogError(FormattableString message, Object obj = null) {
		if(_level <= LogLevel.Error) Debug.LogError($"[{_tag.WithColor(_color)}] {message.ToString(RichLog.Default)}", obj);
	}

	public void LogException(Exception ex, Object obj = null) {
		if (_level <= LogLevel.Error) {
			if (ex.InnerException != null)
				Debug.LogError($"[{_tag.WithColor(_color)}] {ex.GetType().Name} {ex.Message.WithColor(RichLog.Color.red)}\n{ex.StackTrace}\n----------INNER----------\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}", obj);
			else Debug.LogError($"[{_tag.WithColor(_color)}] {ex.GetType().Name} {ex.Message.WithColor(RichLog.Color.red)}\n{ex.StackTrace}", obj);
		}
	}

	public virtual void SetLevel(LogLevel level) => _level = level;
}