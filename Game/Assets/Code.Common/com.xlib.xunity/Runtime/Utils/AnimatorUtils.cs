using UnityEngine;

namespace XLib.Unity.Utils {

	public static class AnimatorUtils {
		public static bool SetTriggerIsPlaying(this Animator animator, string name) {
			if (animator.runtimeAnimatorController == null) return false;
			animator.SetTrigger(name);
			return true;
		}

		public static bool SetTriggerIsPlaying(this Animator animator, int id) {
			if (animator.runtimeAnimatorController == null) return false;
			animator.SetTrigger(id);
			return true;
		}

		public static bool SetIntegerIsPlaying(this Animator animator, int id, int value) {
			if (animator.runtimeAnimatorController == null) return false;
			animator.SetInteger(id, value);
			return true;
		}
	}

}