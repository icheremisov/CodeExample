using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLib.Assets.Utils;
using XLib.Configs.Contracts;
using XLib.Core.Utils;
using XLib.Unity.Extensions;
using XLib.Unity.Utils;

namespace Client.Definitions
{
    [UsedImplicitly]
    public class SceneFilterAttributeProcessor : OdinAttributeProcessor
    {
        public override bool CanProcessSelfAttributes(InspectorProperty property)
            => property.GetAttribute<SceneFilterAttribute>() != null;

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            var sceneFilter = property.GetAttribute<SceneFilterAttribute>();
            attributes.AddOnce(new ValueDropdownAttribute(
                $"@{TypeOf<SceneFilterAttributeProcessor>.FullName}.SceneList($property)"));

            if (sceneFilter.BuildMode == SceneFilterAttribute.SceneBuildModes.AddInBuildSettings)
            {
                attributes.AddOnce(new OnValueChangedAttribute(
                    $"@{TypeOf<SceneFilterAttributeProcessor>.FullName}.AddScenesToBuildSettings($property)"));
            }
            if (sceneFilter.BuildMode == SceneFilterAttribute.SceneBuildModes.AddInAddressable)
            {
                attributes.AddOnce(new OnValueChangedAttribute(
                    $"@{TypeOf<SceneFilterAttributeProcessor>.FullName}.AddScenesToAddressable($property)"));
            }
        }

        [UsedImplicitly]
        public static IEnumerable SceneList(InspectorProperty property)
        {
            var sceneFilter = property.GetAttribute<SceneFilterAttribute>();
            var allScenes = EditorUtils.GetAssetPaths(typeof(Scene)).ToArray();
            var paths = allScenes.Where(x => x.IsMatch(sceneFilter.Filter)).OrderBy(x => x).ToArray();
            var scenes = paths.SelectToArray(Path.GetFileNameWithoutExtension);

            return scenes.GroupBy(s => s)
                .Select(group => new ValueDropdownItem(group.Key, group.Key));
        }

        public static void AddScenesToBuildSettings(InspectorProperty property)
        {
            var oldScenes = EditorBuildSettings.scenes;
            var sceneName = property.GetValue<string>();
            if (sceneName.IsNullOrEmpty()) return;

            var newScenes = oldScenes.ToList();
            if (oldScenes.All(x => !x.path.Contains(sceneName)))
                newScenes.Add(new EditorBuildSettingsScene(sceneName, true));
            
            EditorBuildSettings.scenes = newScenes.ToArray();
        }
        
        public static void AddScenesToAddressable(InspectorProperty property)
        {
            var sceneName = property.GetValue<string>();
            if (sceneName.IsNullOrEmpty()) return;

            var path = EditorUtils.GetAssetPaths<Scene>($"/{sceneName}.unity").FirstOrDefault();
            if (path.IsNullOrEmpty()) return;

            var guid = AssetDatabase.AssetPathToGUID(path);
            if (guid.IsNullOrEmpty()) return;
            AddressableUtils.AddToAddressables(guid, "Scenes");
        }
    }
}