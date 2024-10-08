using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
public static class AnimatorExtensions {

	public static void SetFloat(this Animator anim, int id, float v, bool optional) {
		if (optional && anim.parameters.All(x => x.nameHash != id)) return;

		anim.SetFloat(id, v);
	}

	public static void SetInteger(this Animator anim, int id, int v, bool optional) {
		if (optional && anim.parameters.All(x => x.nameHash != id)) return;

		anim.SetInteger(id, v);
	}

	public static void SetTrigger(this Animator anim, int id, bool optional) {
		if (optional && anim.parameters.All(x => x.nameHash != id)) return;

		anim.SetTrigger(id);
	}

}