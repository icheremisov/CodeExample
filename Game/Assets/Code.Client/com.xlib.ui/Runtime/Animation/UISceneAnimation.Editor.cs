#if UNITY_EDITOR

using System.Collections.Generic;

namespace XLib.UI.Animation {

	public partial class UISceneAnimation {
		public IEnumerable<UINamedAnimation> Animations => _animations;
	}

}

#endif