using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Client.Utility {

	public class SolutionPreprocessor : AssetPostprocessor {
		private static readonly ProjectFolder[] Folders = {
			new("", "Assembly-CSharp*"), // unity сборки осталвяем в корне
			new("Library", "XLib.*"),
			new("Game", "Client*"),
			new("Thirdparty", "*"), // все остальные библиотеки добавляем в Thirdparty
		};

		private static MD5 _md5 = MD5.Create();
		private static string ComputeGuidHashFor(string value) => $"{{{new Guid(_md5.ComputeHash(Encoding.Default.GetBytes(value)))}}}".ToUpper();

		private class ProjectFolder {

			private string[] Filters { get; }
			public string Name { get; }

			public ProjectFolder(string name, params string[] filters) {
				Name = name;
				Filters = filters;
			}

			public bool IsMatch(string name) => Filters.Any(filter => name.IsMatch(filter));
		}

		private class Solution {

			internal class SolutionItem : ISolutionBlock {

				public string Name { get; set; }
				public string RelativePath { get; set; }
				public string Guid { get; set; }
				public string ParentGuid { get; set; }

				public override string ToString() => $"Project(\"{ParentGuid}\") = \"{Name}\", \"{RelativePath}\", \"{Guid}\"\nEndProject";

			}

			internal interface ISolutionBlock {

				string Name { get; set; }
				string ToString();

			}

			internal class SolutionSection : ISolutionBlock {

				public List<SolutionSection> Children { get; private set; }

				public SolutionSection(string line) {
					Name = line;
					for (Indent = 0; Indent < line.Length; ++Indent) {
						if (line[Indent] != '\t') break;
					}
				}

				public string Name { get; set; }
				public int Indent { get; }
				public override string ToString() => Children == default ? Name : $"{Name}\n{string.Join("\n", Children)}";

				public void Add(SolutionSection entry) {
					if (Children == null) Children = new List<SolutionSection>();

					if (entry.Indent > Indent + 1 && Children.Count > 0)
						Children.Last().Add(entry);
					else
						Children.Add(entry);
				}

			}

			private List<ISolutionBlock> _list;

			public void Load(string solution) {
				var projMatcher = new Regex("Project\\(\"({[A-Fa-f0-9-]+})\"\\) = \"(.*?)\", \"(.*?)\", \"({[A-Fa-f0-9-]+})\"", RegexOptions.Multiline);

				_list = new List<ISolutionBlock>();
				SolutionSection lastSection = null;
				foreach (var line in solution.Split("\n")) {
					if (line.StartsWith("EndProject")) continue;

					var match = projMatcher.Match(line);
					if (match.Success) {
						var groups = match.Groups;
						_list.Add(new SolutionItem {
							Name = groups[2].Value,
							RelativePath = groups[3].Value,
							Guid = groups[4].Value,
							ParentGuid = groups[1].Value
						});
					}
					else {
						var entry = new SolutionSection(line);
						if (lastSection != null && entry.Indent > lastSection.Indent)
							lastSection.Add(entry);
						else {
							lastSection = entry;
							_list.Add(entry);
						}
					}
				}
			}

			public IEnumerable<SolutionItem> Projects() => _list.OfType<SolutionItem>();

			public string Save() => string.Join("\n", _list.Select(o => o.ToString()));

			public void AddFolder(string folderName, IEnumerable<SolutionItem> projectItems) {
				var guid = ComputeGuidHashFor(folderName);
				var index = _list.FindLastIndex(block => block is SolutionItem);
				_list.Insert(index, new SolutionItem {
					Name = folderName,
					Guid = guid,
					RelativePath = folderName,
					ParentGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"
				});

				var globalSectionIndex = _list.FindIndex(block => block.Name.StartsWith("Global"));
				var globalSection = _list[globalSectionIndex] as SolutionSection;
				const string globalSectionNestedProjects = "GlobalSection(NestedProjects) = preSolution";
				var nestedProjects = globalSection.Children.FirstOrDefault(x => x.Name.Contains(globalSectionNestedProjects));
				if (nestedProjects == null) {
					var indent = Tab(globalSection.Indent + 1);
					nestedProjects = new SolutionSection(indent + globalSectionNestedProjects);
					globalSection.Children.Add(nestedProjects);
					globalSection.Children.Add(new SolutionSection($"{indent}EndGlobalSection"));
				}

				var itemIndent = Tab(nestedProjects.Indent + 1);
				foreach (var item in projectItems) {
					item.ParentGuid = guid;
					nestedProjects.Add(new SolutionSection($"{itemIndent}{item.Guid} = {guid}"));
				}
			}

			private static string Tab(int count) => new('\t', count);
		}

		public static string OnGeneratedSlnSolution(string path, string content) {

			var solution = new Solution();
			solution.Load(content);
			
			var groups = solution
				.Projects()
				.GroupBy(item => Folders.FirstOrDefault(folder => folder.IsMatch(item.Name)));

			foreach (var group in groups) {
				if (!string.IsNullOrEmpty(group.Key?.Name))
					solution.AddFolder(group.Key.Name, group);
			}

			Debug.Log("Regenerate Solution File " + path);
			return solution.Save();
		}

	}

}