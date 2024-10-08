using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using XLib.Unity.Utils;

namespace XLib.UI.Controls {

	public class UIMaterialReplace : MonoBehaviour {

		private static readonly int ColorId = Shader.PropertyToID("_Color");
		private static readonly int SaturationId = Shader.PropertyToID("_Saturation");

		[SerializeField] private ReplaceData[] _objects;
		[SerializeField] private GameObject[] _excludeObjects;

		[Header("Replace With"), SerializeField] private Material _material;
		[SerializeField] private Material _textMaterial;
		[SerializeField] private TMP_SpriteAsset _textSprites;
		[SerializeField] private TextReplaceMode _textReplaceMode = TextReplaceMode.OnlyShader;
		private readonly List<Graphic> _cache = new(32);
		private readonly List<GameObject> _extraExcludedCache = new(16);

		private readonly List<GameObject> _extraExcludedObjects = new(4);

		private readonly List<ReplacedInfo> _replacedObjects = new(32);

		private bool IsReplaced => _replacedObjects.Count > 0;

		private void OnDestroy() {
			SetNormal();
		}

		public void SetObjectExcluded(GameObject obj, bool excludeFromFx) {
			var wasReplaced = IsReplaced;

			if (wasReplaced) SetNormal();

			if (excludeFromFx)
				_extraExcludedObjects.AddOnce(obj);
			else
				_extraExcludedObjects.Remove(obj);

			if (wasReplaced) SetReplaced();
		}

		public void SetReplaced(bool replace) {
			if (replace)
				SetReplaced();
			else
				SetNormal();
		}

		public void SetReplaced() {
			if (IsReplaced) SetNormal();

			_extraExcludedCache.Clear();

			if (_extraExcludedObjects.Count > 0) {
				foreach (var obj in _extraExcludedObjects) {
					obj.GetComponentsInChildren(true, _cache);
					_extraExcludedCache.AddRange(_cache.Select(x => x.gameObject));
				}
			}

			foreach (var obj in IterateObjects()) {
				switch (obj) {
					case TMP_SubMeshUI _: break;

					case TextMeshProUGUI text: {
						Material mat = null;

						if (_textMaterial != null) {
							if (_textReplaceMode == TextReplaceMode.OnlyShader) {
								var srcMaterial = text.fontMaterial;
								mat = new Material(srcMaterial) { shader = _textMaterial.shader };

								mat.CopyColorFrom(ColorId, _textMaterial);
								mat.CopyFloatFrom(SaturationId, _textMaterial);
							}
							else
								mat = _textMaterial;
						}

						if (mat != null) {
							var origAsset = text.spriteAsset;

							if (_textSprites != null) text.spriteAsset = _textSprites;

							_replacedObjects.Add(new ReplacedInfo { obj = obj, originalMat = text.fontMaterial, originalSprites = origAsset });
							text.fontMaterial = mat;
						}

						break;
					}

					default: {
						if (_material != null) {
							_replacedObjects.Add(new ReplacedInfo { obj = obj, originalMat = obj.material });
							obj.material = _material;
						}

						break;
					}
				}
			}
		}

		public void SetNormal() {
			if (_replacedObjects.Count == 0) return;

			foreach (var info in _replacedObjects) {
				if (!info.obj) continue;

				switch (info.obj) {
					case TMP_SubMeshUI _: break;

					case TextMeshProUGUI text:
						text.fontMaterial = info.originalMat;
						text.spriteAsset = info.originalSprites;
						break;

					default:
						info.obj.material = info.originalMat;
						break;
				}
			}

			_replacedObjects.Clear();
		}

		private IEnumerable<Graphic> IterateObjects() {
			foreach (var replaceData in _objects) {
				if (!replaceData.root || IsExcluded(replaceData.root)) continue;

				if (!replaceData.includeChildren) {
					var g = replaceData.root.GetComponent<Graphic>();

					if (g != null) yield return g;
				}
				else {
					replaceData.root.GetComponentsInChildren(true, _cache);

					foreach (var g in _cache)
						if (!IsExcluded(g.gameObject))
							yield return g;
				}
			}
		}

		private bool IsExcluded(GameObject obj) {
			if (_excludeObjects.Contains(obj)) return true;

			if (_extraExcludedCache.Contains(obj)) return true;

			return false;
		}

		private enum TextReplaceMode {

			OnlyShader = 0,
			FullReplace

		}

		[Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
		private struct ReplaceData {

			[Required] public GameObject root;
			public bool includeChildren;

		}

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private struct ReplacedInfo {

			public Graphic obj;
			public Material originalMat;
			public TMP_SpriteAsset originalSprites;

		}

#if UNITY_EDITOR
		[Button]
		private void SetReplacedBtn() {
			if (!Application.isPlaying) {
				EditorUtility.DisplayDialog("Disabled", "This function works only in play mode!", "OK");
				return;
			}

			SetReplaced();
		}

		[Button]
		private void SetNormalBtn() {
			if (!Application.isPlaying) {
				EditorUtility.DisplayDialog("Disabled", "This function works only in play mode!", "OK");
				return;
			}

			SetNormal();
		}
#endif

	}

}