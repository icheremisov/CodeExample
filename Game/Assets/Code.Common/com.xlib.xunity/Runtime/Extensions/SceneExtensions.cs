using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Extensions {
	public static IEnumerable<T> FindComponents<T>(this Scene scene) => 
		scene.GetRootGameObjects().SelectMany(rootObj => rootObj.GetComponentsInChildren<T>());

	public static IEnumerable<Component> FindComponents(this Scene scene, Type type, bool includeInactive) => 
		scene.GetRootGameObjects().SelectMany(rootObj => rootObj.GetComponentsInChildren(type, includeInactive));

	public static T FindComponent<T>(this Scene scene) => FindComponents<T>(scene).FirstOrDefault();
}