using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using XLib.Core.Utils;

namespace XLib.Core.Parsers {

	public abstract class DynamicOptions<TDerived> where TDerived : DynamicOptions<TDerived> {
		
		private readonly Dictionary<string, object> _options = new(StringComparer.InvariantCultureIgnoreCase);
		public IReadOnlyDictionary<string, object> Options => _options;

		public DynamicOptions() {
		}
		
		public DynamicOptions(Dictionary<string, object> data) {
			foreach (var v in data) _options.Add(v.Key, v.Value);
		}

		public DynamicOptions(params KeyValuePair<string, object>[] data) {
			foreach (var v in data) _options.Add(v.Key, v.Value);
		}

		public DynamicOptions(Dictionary<string, string> data) {
			foreach (var v in data) _options.Add(v.Key, v.Value);
		}

		public DynamicOptions(params KeyValuePair<string, string>[] data) {
			foreach (var v in data) _options.Add(v.Key, v.Value);
		}

		public T Get<T>(string key, T defValue) {

			if (!_options.TryGetValue(key, out var value)) return defValue;

			try {
				return value is string valueStr ? valueStr.Parse<T>() : (T)value;
			}
			catch (Exception) {
				return defValue;
			}
		}

		public T Get<T>(string key) {

			if (!_options.TryGetValue(key, out var value)) throw new Exception($"Option is not found: '{key}'");

			try {
				return value is string valueStr ? valueStr.Parse<T>() : (T)value;
			}
			catch (Exception ex) {
				throw new Exception($"Cannot convert option's '{key}' value to type {TypeOf<T>.FullName} (value='{value}', type={value?.GetType().FullName}), ex = {ex.Message}");
			}
		}

		public TDerived Set(string key, object value) {
			_options[key] = value;
			return (TDerived)this;
		}

		public bool Has(string key) => _options.ContainsKey(key);

		public TDerived OverrideFrom(TDerived other) {
			foreach (var arg in other._options) _options[arg.Key] = arg.Value;
			return (TDerived)this;
		}
		
		public string ToJson() => JsonConvert.SerializeObject(_options, Formatting.None);
		
		public override string ToString() => _options.OrderBy(x => x.Key).Select(x => $"'{x.Key}'='{x.Value}'").JoinToString("\n");
	}

}