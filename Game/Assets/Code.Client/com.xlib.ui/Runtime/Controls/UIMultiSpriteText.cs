using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace XLib.UI.Controls {

	public class UIMultiSpriteText : MonoBehaviour {

		[SerializeField, Required] private UISpriteText[] _texts;
		[SerializeField, Required] private TMP_Text[] _textsTMP;

		public string Text { set { _texts.ForEach(x => x.Text = value); } }

		public TextAlignmentOptions Alignment { set { _textsTMP.ForEach(x => x.alignment = value); } }

	}

}