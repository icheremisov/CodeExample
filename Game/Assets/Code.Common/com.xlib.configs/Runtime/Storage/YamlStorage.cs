#if !UNITY3D

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace XLib.Configs.Storage {

	public class YamlFileReferenceEntry {

		public long ReferenceId { get; set; }
		public Type Type { get; set; }
		public YamlNode YamlNode { get; set; }
		public object Obj { get; set; }

	}

	public class YamlFileEntry {

		public YamlNode YamlNode { get; set; }
		public object Obj { get; set; }
		public string FileName { get; set; }
		public string GUID { get; set; }
		public Type Type { get; set; }
		public Dictionary<long, YamlFileReferenceEntry> References { get; set; }

	}

	public interface IYamlEntryReference {

		public YamlFileEntry GetReference(YamlMappingNode reference, string defGUID);

	}

	public class YamlStorage : IYamlEntryReference {

		private readonly IUnityLogger _logger;

		private readonly Dictionary<string, YamlFileEntry> _entries = new(1024);
		public IReadOnlyDictionary<string, YamlFileEntry> Entries => _entries;

		public YamlStorage(IUnityLogger logger) {
			_logger = logger;
		}

		private string GetGuidFromMetaFile(string metafile) =>
			File.ReadLines(metafile)
				.First(s => s.StartsWith("guid: "))
				.Split(' ', 2)
				.Last();

		public YamlFileEntry GetReference(YamlMappingNode reference, string defGUID) {
			if (!reference.Children.TryGetValue("fileID", out var fileId)) return null;
			reference.Children.TryGetValue("guid", out var guid);
			return _entries.TryGetValue((guid != null ? (string)guid : defGUID) + (string)fileId, out var value) ? value : null;
		}

		public void PhaseLoading(IEnumerable<string> assets, string assetsDir) {
			foreach (var file in assets) {
				var relativePath = Path.GetRelativePath(assetsDir, file);
				var metafile = file + ".meta";
				if (!File.Exists(metafile)) continue;

				var guid = GetGuidFromMetaFile(metafile);
				_logger.LogVerbose($"Reading YAML file at {relativePath}");
				using var fs = new FileStream(file, FileMode.Open);
				var yamlStream = new YamlStream();
				yamlStream.Load(new StreamReader(fs));
				foreach (var document in yamlStream.Documents) {
					var fileId = document.RootNode.Anchor;
					_entries[guid + fileId] = new YamlFileEntry { YamlNode = document.RootNode, FileName = relativePath, GUID = guid };
				}
			}
		}

		public void PhaseConstruction(IConfigTypeProvider typeProvider) {
			Debug.Log("Building config structures: Second pass (create objects)");
			foreach (var entry in _entries.Values) {
				_logger.LogVerbose("Creating object for " + entry.FileName);
				var behaviour = (YamlMappingNode)entry.YamlNode["MonoBehaviour"];
				var script = behaviour["m_Script"] as YamlMappingNode;
				script.Children.TryGetValue("guid", out var guidNode);
				var type = typeProvider.GetTypeFromGuid((string)guidNode);
				if (type == null) throw new NotSupportedException($"Unrecognised config type: {(string)guidNode} in file {entry.FileName}");
				entry.Type = type;
				entry.Obj = Activator.CreateInstance(type);

				if (behaviour.Children.TryGetValue("references", out var references)) {
					entry.References = new Dictionary<long, YamlFileReferenceEntry>();
					var refIds = references["RefIds"] as YamlSequenceNode;
					foreach (var refIdNode in refIds) {
						var rid = long.Parse((string)refIdNode["rid"]);
						var typeInfo = refIdNode["type"] as YamlMappingNode;
						var data = refIdNode["data"];

						var referenceType = typeProvider.GetTypeByName((string)typeInfo["class"], (string)typeInfo["ns"], (string)typeInfo["asm"]);
						if (referenceType == null)
							throw new NotSupportedException($"Unrecognised config type: {(string)typeInfo["ns"]}.{(string)typeInfo["class"]} in file {entry.FileName}");
						entry.References.Add(rid, new YamlFileReferenceEntry() {
							ReferenceId = rid,
							YamlNode = data,
							Type = referenceType,
							Obj = Activator.CreateInstance(referenceType)
						});
					}
				}
			}
		}

		public YamlFileEntry GetEntryByName(string name) => _entries.Values.FirstOrDefault(entry => entry.FileName.Equals(name, StringComparison.InvariantCultureIgnoreCase));

	}

}
#endif