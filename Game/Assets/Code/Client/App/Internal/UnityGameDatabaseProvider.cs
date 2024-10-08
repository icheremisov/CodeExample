using System;
using System.Threading;
using System.Threading.Tasks;
using Client.Core.Common.Contracts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Configs;
using XLib.Configs.Contracts;
using Zenject;

namespace Client.App.Internal {

	public class UnityGameDatabaseProvider : IClientGameDatabaseProvider, IDisposable, IInitializable {
		private readonly IDataStorageProvider _dataStorageProvider;
		private IGameDatabase _gameDatabase;
		
		public UnityGameDatabaseProvider(IDataStorageProvider dataStorageProvider) {
			_dataStorageProvider = dataStorageProvider;
		}

		public void Initialize() => GameData.Reset();

		public void Dispose() {
			_gameDatabase?.Dispose();
			_gameDatabase = null;
			GameData.Reset();
		}
		
		public IGameDatabase Get() {
			if (_gameDatabase == null) throw new Exception($"[GameDatabase] No Database loaded while getting!");
			return _gameDatabase;
		}

		public async Task LoadGameDatabase() {
			Debug.Log($"[GameDatabase] Loading database");
			_gameDatabase?.Dispose();

			var isMainThread = PlayerLoopHelper.MainThreadId == Thread.CurrentThread.ManagedThreadId;
			
			try {
				if (!isMainThread) await UniTask.SwitchToMainThread();
				
				GameData.Reset();
				_gameDatabase ??= new GameDatabase();
				await _gameDatabase.LoadConfigs(_dataStorageProvider);
			}
			finally {
				if (!isMainThread) await UniTask.SwitchToThreadPool();
			}
		}

		public Task LocalServerPreload() => LoadGameDatabase();
	}

}