using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XLib.BuildSystem.GameDefines
{
    public partial class CustomDefineManager : EditorWindow
    {
        /*private static CustomDefineManagerData _dataFile = null;
        private static CustomDefineManagerData dataFile
        {
            get
            {
                if (_dataFile == null) _dataFile = Resources.Load<CustomDefineManagerData>("CustomDefineManagerData");

                return _dataFile;
            }
        }*/

        public static List<Directive> LoadDirectives()
        {
            var directives = new List<Directive>();

            foreach (CdmBuildTargetGroup platform in Enum.GetValues(typeof(CdmBuildTargetGroup)))
            {
                var platformSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform.ToBuildTargetGroup());

                if (!String.IsNullOrEmpty(platformSymbols))
                {
                    foreach (var symbol in platformSymbols.Split(';'))
                    {
                        var directive = directives.FirstOrDefault(d => d._name == symbol);

                        if (directive == null)
                        {
                            directive = new Directive { _name = symbol };
                            directives.Add(directive);
                        }

                        directive._targets |= platform;
                    }
                }
            }

            var dataFileDirectives = GetDirectivesFromXmlFile();

            if (directives.Any())
            {
                if (!dataFileDirectives.Any())
                {
                    SaveDirectives(directives);
                }
            }

            // Add any directives from the data file which weren't located in the configuration file
            directives.AddRange(dataFileDirectives.Where(df => !directives.Any(d => d._name == df._name)));

            foreach (var dataFileDirective in dataFileDirectives)
            {
                var directive = directives.First(d => d._name == dataFileDirective._name);

                directive._enabled = dataFileDirective._enabled;
                directive._sortOrder = dataFileDirective._sortOrder;
            }

            return directives.OrderBy(d => d._sortOrder).ToList();
        }

        public static void SaveDirectives(List<Directive> directives)
        {
            var targetGroups = new Dictionary<CdmBuildTargetGroup, List<Directive>>();

            foreach (var directive in directives)
            {
                foreach (CdmBuildTargetGroup targetGroup in Enum.GetValues(typeof(CdmBuildTargetGroup)))
                {
                    if (String.IsNullOrEmpty(directive._name) || !directive._enabled) continue;

                    if (directive._targets.HasFlag(targetGroup))
                    {
                        if (!targetGroups.ContainsKey(targetGroup))
                            targetGroups.Add(targetGroup, new List<Directive>());

                        targetGroups[targetGroup].Add(directive);
                    }
                }
            }

            foreach (CdmBuildTargetGroup targetGroup in Enum.GetValues(typeof(CdmBuildTargetGroup)))
            {
                var symbols = "";

                if (targetGroups.TryGetValue(targetGroup, out var group))
                    symbols = string.Join(";", group.Select(d => d._name).ToArray());

                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup.ToBuildTargetGroup(), symbols);
            }

            SaveDirectivesToDataFile(directives);
        }

        private static void SaveDirectivesToDataFile(List<Directive> directives)
        {
            var x = 0;
            directives.ForEach(d => d._sortOrder = x++);

            SaveDataToXmlFile(directives);
        }

        public static Directive GetDirective(string directiveName)
        {
            return GetDirectivesFromXmlFile()
                .FirstOrDefault(d => d._name.Equals(directiveName, StringComparison.OrdinalIgnoreCase));
        }

        public static void EnableDirective(string directiveName)
        {
            var directives = GetDirectivesFromXmlFile();
            var directive =
                directives.FirstOrDefault(d => d._name.Equals(directiveName, StringComparison.OrdinalIgnoreCase));

            if (directive == null)
            {
                Debug.LogErrorFormat("Directive '{0}' not found!", directiveName);
                return;
            }

            directive._enabled = true;
            CustomDefineManager.SaveDirectives(directives);

            // also update the editor window            
            var window = Resources.FindObjectsOfTypeAll<CustomDefineManager>().LastOrDefault();
            if (window != null)
            {
                var windowDirective = window._directives.FirstOrDefault(d => d._name == directive._name);
                if (windowDirective != null)
                {
                    windowDirective._enabled = true;
                    window.Repaint();
                }
            }
        }

        public static void DisableDirective(string directiveName)
        {
            var directives = GetDirectivesFromXmlFile();
            var directive =
                directives.FirstOrDefault(d => d._name.Equals(directiveName, StringComparison.OrdinalIgnoreCase));

            if (directive == null)
            {
                Debug.LogErrorFormat("Directive '{0}' not found!", directiveName);
                return;
            }

            directive._enabled = false;
            CustomDefineManager.SaveDirectives(directives);

            // also update the editor window            
            var window = Resources.FindObjectsOfTypeAll<CustomDefineManager>().LastOrDefault();
            if (window != null)
            {
                var windowDirective = window._directives.FirstOrDefault(d => d._name == directive._name);
                if (windowDirective != null)
                {
                    windowDirective._enabled = false;
                    window.Repaint();
                }
            }
        }
    }
}