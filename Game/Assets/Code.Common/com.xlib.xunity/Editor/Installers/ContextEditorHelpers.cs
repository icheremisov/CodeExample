using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XLib.Core.Reflection;
using XLib.Core.Utils;
using XLib.Unity.Installers.Attributes;
using XLib.Unity.Utils;
using Zenject;

namespace XLib.Unity.Installers
{
    public static class ContextEditorHelpers
    {
        public static bool NeedUpdateInstallers(Context context)
        {
            var installers = GetNewInstallers(context.gameObject);

            var decoratorContext = context as SceneDecoratorContext;
            if (decoratorContext != null)
                return installers.Length != decoratorContext.LateInstallers.Count() ||
                       !decoratorContext.LateInstallers.All(installer =>
                           installer != null && installers.Contains(installer.GetType()));

            if (context != null)
                return installers.Length != context.Installers.Count() || !context.Installers.All(installer =>
                    installer != null && installers.Contains(installer.GetType()));
            return true;
        }

        public static void BindAll(Context context)
        {
            var scene = context.gameObject.scene;

            var installerTypes = GetNewInstallers(context.gameObject);

            // clear invalid installers
            foreach (var existingInstaller in GetExistingInstallers(context.gameObject))
            {
                if (!installerTypes.Contains(existingInstaller.GetType()))
                {
                    GameObject.DestroyImmediate(existingInstaller);
                }
            }

            var installers = new List<MonoInstaller>(16);
            foreach (var type in installerTypes)
            {
                var installer = (MonoInstaller)context.gameObject.GetComponent(type);
                if (installer == null)
                {
                    installer = (MonoInstaller)context.gameObject.AddComponent(type);
                    Debug.Assert(installer != null, $"Error creating installer {type.FullName}");
                }

                if (installer != null) installers.Add(installer);
            }

            foreach (var monoInstaller in installers) EditorUtils.FindReferences(monoInstaller, true, scene);

            var decoratorContext = context as SceneDecoratorContext;
            if (decoratorContext != null)
            {
                decoratorContext.LateInstallers = installers;
                EditorUtility.SetDirty(decoratorContext);
                return;
            }

            context.Installers = installers;
            EditorUtility.SetDirty(context);
        }

        private static InstallerContainer? DetectContainerType(GameObject gameObject)
        {
            if (gameObject.GetComponent<ProjectContext>() != null) return null;

            var scene = gameObject.scene;
            if (scene.name == "Main") return InstallerContainer.Main;
            if (scene.name.EndsWith("_Logic") || scene.GetRootGameObjects().Any(x => x.name == "LevelRoot"))
                return InstallerContainer.Level;
            if (scene.GetRootGameObjects().Any(x => x.name == "BattlePersistentBinding"))
                return InstallerContainer.BattlePersistent;
            return null;
        }

        private static MonoInstaller[] GetExistingInstallers(GameObject gameObject)
        {
            if (gameObject.GetComponent<ProjectContext>() != null)
                return gameObject.GetComponentsInChildren<MonoInstaller>(true);

            var scene = gameObject.scene;
            return GameObject.FindObjectsOfType<MonoInstaller>(true)
                .Where(x => x.gameObject.scene == scene).ToArray();
        }

        private static Type[] GetNewInstallers(GameObject gameObject)
        {
            var result = GetExistingInstallers(gameObject)
                .SelectToList(x => x.GetType());

            if (gameObject.GetComponent<ProjectContext>() != null)
            {
                var baseType = typeof(ProjectContextInstaller<>);
                bool RemoveInvalidType(Type t) => !t.IsSubclassOfRawGeneric(baseType);
                result.RemoveAll(RemoveInvalidType);

                var newTypes = FindProjectContextInstallers();
                result.AddOnce(newTypes);
            }
            else
            {
                var container = DetectContainerType(gameObject);
                if (container != null)
                {
                    bool RemoveInvalidType(Type t)
                    {
                        var c = t.GetAttribute<InstallerContainerAttribute>();
                        if (c == null || c.Container == container) return false;
                        return true;
                    }

                    result.RemoveAll(RemoveInvalidType);

                    var newTypes = FindInstallers(container.Value);
                    result.AddOnce(newTypes);
                }
                else
                {
                    Debug.LogError("Unknown container type: " + gameObject, gameObject);
                }
            }

            return result.ToArray();
        }

        private static IEnumerable<Type> FindInstallers(InstallerContainer container)
        {
            var baseType = typeof(BaseInstaller<>);

            bool Filter(Type x)
            {
                var isValid = (!x.IsAbstract && x.IsSubclassOfRawGeneric(baseType));
                if (isValid) isValid = x.GetAttribute<InstallerContainerAttribute>()?.Container == container;

                return isValid;
            }

            return TypeUtils.EnumerateAll(Filter);
        }

        private static IEnumerable<Type> FindProjectContextInstallers()
        {
            var baseType = typeof(ProjectContextInstaller<>);

            bool Filter(Type x)
            {
                var isValid = (!x.IsAbstract && x.IsSubclassOfRawGeneric(baseType));
                return isValid;
            }

            return TypeUtils.EnumerateAll(Filter);
        }
    }
}