using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Client.Cheats.Internal {

	public class CheatArgumentData {
		private List<object[]> _arguments;
		private readonly CheatPluginData _data;

		private readonly object[] _args;
		private readonly int _argsCount;
		private readonly int[] _argsCoef;
		private int _argsIterations;

		public CheatArgumentData(CheatPluginData data) {
			_data = data;

			_args = new object[_data.ArgumentType.Length];
			_argsCount = _args.Length;
			_argsCoef = new int[_argsCount + 1];
			_argsCoef[0] = 1;
			_argsIterations = 1;
		}

		public bool IsEmpty => _arguments.Any(v => v.Length == 0);

		public void Update(CheatDiResolver resolver) {
			_arguments = _data.ArgumentType.Select(resolver.ResolveAll).ToList();
			var i = 0;
			_argsIterations = 1;
			foreach (var el in _arguments) {
				_argsIterations *= el.Length;
				_argsCoef[++i] = _argsIterations;
			}
		}

		public int Priority => IsEmpty ? -1000 : _data.Priority;
		public int Iterations => _argsIterations;

		public void DrawCheat(string searchQuery) {
			if (IsEmpty) return;

			for (var i = 0; i < _argsIterations; ++i) {
				var c = i;
				for (var j = _argsCount - 1; j >= 0; --j) {
					_args[j] = _arguments[j][c / _argsCoef[j]];
					c %= _argsCoef[j];
				}

				if (searchQuery.IsNotNullOrEmpty() && _args.Any(o => o is Object)) {
					if (_args.OfType<Object>().All(o => o.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) == -1)) continue;
				}

				if (_argsIterations <= 1 && _data.IsMethod) {
					_data.InvokeMethod(_args);
					return;
				}

				_data.Invoke(_args);
			}
		}
	}

}