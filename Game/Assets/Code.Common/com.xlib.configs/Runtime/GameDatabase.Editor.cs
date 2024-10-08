#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Core;
using XLib.Core.Utils;

namespace XLib.Configs
{
    public class GameDatabase_Editor : IGameDatabase
    {
        public static IGameDatabase Instance => GetDatabase();

        private static IGameDatabase _database;

        private ConfigManifest _manifest;
        private int _version;
        private IGameItem[] _additional;

        public void Dispose() => _manifest = null;

        private void LoadManifest()
        {
            Debug.Assert(_manifest == null);

            _manifest = AssetDatabase.FindAssets($"t:{nameof(ConfigManifest)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<ConfigManifest>()
                .FirstOrDefault();

            _additional = _manifest != null
                ? _manifest.Configs.OfType<IGameItemContainer>()
                    .SelectMany(container => container.RawElements.OfType<IGameItem>()).ToArray()
                : Array.Empty<IGameItem>();

            if (_manifest != null)
            {
                _version = _manifest.Configs.OfType<IConfigRemapper>()
                    ?.MaxByOrDefault(remapper => remapper.MigrationVersion)?.MigrationVersion ?? 0;
            }
            else _version = 0;
        }

        public static IGameDatabase GetDatabase()
        {
            var instance = _database;
            if (instance != null) return instance;

            instance = new GameDatabase_Editor();
            ((GameDatabase_Editor)instance).LoadManifest();
            _database = instance;
            return instance;
        }

        private IEnumerable<IGameItem> Configs
        {
            get
            {
                // there is a problem with a project window - when assets are reimported, _manifest can be unloaded. Load it again in this case
                if (!_manifest) LoadManifest();
                return (_manifest != null && _manifest.Configs != null
                    ? _manifest.Configs.Concat(_additional)
                    : null) ?? Enumerable.Empty<IGameItem>();
            }
        }

        public Task LoadConfigs(IDataStorageProvider storageProvider)
        {
            foreach (var gameItem in Configs)
            {
                gameItem.Init(this);
            }

            return Task.CompletedTask;
        }

        public T Get<T>(ItemId id, bool throwOnNotFound = true) where T : class =>
            Configs.FirstOrDefault(item => item != null && item.Id == id) as T;

        public IEnumerable<T> All<T>() => Configs.OfType<T>();

        public IEnumerable<T> AllAsInterface<T>() => Configs.OfType<T>();

        public T Once<T>(bool throwOnNotFound = true)
        {
            try
            {
                return Configs.OfType<T>().First();
            }
            catch (Exception e)
            {
                if (throwOnNotFound)
                    throw new Exception($"Got '{e.Message}' while getting Once<{TypeOf<T>.Name}>()");
                else
                {
                    Debug.LogError($"Got '{e.Message}' while getting Once<{TypeOf<T>.Name}>()");
                    return default;
                }
            }
        }

        public int ShortVersion => _version;
        public string ConfigHash => null;
    }
}
#endif