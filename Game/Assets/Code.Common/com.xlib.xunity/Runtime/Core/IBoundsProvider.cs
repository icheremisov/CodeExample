using UnityEngine;

namespace XLib.Unity.Core {

	public interface IBoundsProvider {
		Bounds GetBounds(bool local);
	}

}