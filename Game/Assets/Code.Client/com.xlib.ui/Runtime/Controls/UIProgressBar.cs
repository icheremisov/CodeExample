using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLib.Core.CommonTypes;
using XLib.UI.Procedural;
using XLib.UI.Utils;

namespace XLib.UI.Controls {

	/// <summary>
	///     simple progress bar
	/// </summary>
	[Serializable]
	public class UIProgressBar {
		[SerializeField] private Image _barImage;
		[SerializeField] private UICircle _barCircle;
		[SerializeField, ShowIf("@_barImage == null && _barCircle == null")] private RectTransform _barRect;
		[SerializeField] private RectTransform _barRoot;
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private bool _remainMode = false;

		[Space, SerializeField, HideIf(nameof(IsFilled))]
		private float _minProgressWidth;

		private const float _defaultProgressDuration = 0.3f;

		public RectTransform BarRoot => _barRoot;

		public Color Color {
			set {
				if (_barImage) _barImage.color = value;
				if (_barCircle) _barCircle.ImageColor = value;
			}
		}

		public bool HasView => _barImage || _barCircle;

		public void SetActive(bool v) {
			if (_barImage) _barImage.SetActive(v);
			if (_barCircle) _barCircle.SetActive(v);
		}
		
		public enum ProgressBarFormat {
			Relative,
			Percentage,
			Timer,
			TimerNoSec,
			NoText,
			Countdown
		}

		[SerializeField]
		private ProgressBarFormat _format = ProgressBarFormat.Relative;

		private bool IsFilled => (_barImage != null && _barImage.type == Image.Type.Filled) || (_barCircle != null && _barCircle.FilledSector);

		/// <summary>
		///     set text value on bar
		/// </summary>
		public string Text {
			set {
				if (_text) _text.text = value;
			}
		}

		/// <summary>
		///     set value to bar: [0..1]
		/// </summary>
		protected void SetValue(float value, float duration = 0.3f, AnimDirection anim = AnimDirection.Default) {
			if (!IsFilled) {
				var rect = _barRect;
				if (rect == null && _barImage != null) rect = _barImage.GetRectTransform();
				if (rect == null) return;
				FillImageUtility.FillProgress(value, rect, _barRoot, anim, minWidth: _minProgressWidth, duration: duration);
			}
			else {
				if (_barImage != null) FillImageUtility.FillProgress(value, _barImage, anim, duration);
				else if (_barCircle != null) FillImageUtility.FillProgress(value, _barCircle, anim, duration);
			}
		}

		/// <summary>
		///		get transform value of bar's point: [0..1]
		/// </summary>
		public Vector2 GetPointTransform(float progress) {
			var rectTransform = _barRoot != null ? _barRoot.GetRectTransform() : null;

			if (rectTransform == null) return Vector2.zero;

			var minX = rectTransform.rect.xMin;
			var maxX = rectTransform.rect.xMax;
			var xPos = Mathf.Lerp(minX, maxX, progress);

			return new Vector2(xPos, rectTransform.rect.y);
		}

		public void SetProgress(Duration remain, Duration total, AnimDirection anim = AnimDirection.Default) {
			_format = ProgressBarFormat.Timer;
			SetProgress(total.Value - remain.Value, total.Value, anim);
		}

		public void SetProgress(TimeRange range, ITimeProvider timeProvider, AnimDirection anim = AnimDirection.Default) {
			if (range.IsInfinity)
				SetProgress(0, (int)range.TimeLeft(timeProvider), anim);
			else
				SetProgress((int)range.TimePassed(timeProvider), (int)range.Duration, anim);
		}

		public void SetProgress(
			float current,
			float total = 1f,
			AnimDirection anim = AnimDirection.Default,
			float progress = -1f,
			float duration = -1f) {
			if (progress == -1f) progress = Mathf.Clamp01(current / total);
			if (duration < 0f) duration = _defaultProgressDuration;

			if (float.IsNaN(progress)) progress = 0f;

			SetValue(progress, duration, anim);

			if (_text != null) {
				string locale = null;
				switch (_format) {
					case ProgressBarFormat.Percentage:
						locale = $"{Mathf.RoundToInt(100f * progress)}%";
						break;

					case ProgressBarFormat.Relative:
						locale = $"{(int)current}/{(int)total}";
						break;

					// case ProgressBarFormat.Timer:
					// 	locale = new Duration((int)(total - current)).ToTimer().ToString();
					// 	break;
					//
					// case ProgressBarFormat.TimerNoSec:
					// 	locale = new Duration((int)(total - current)).ToTimerNoSec().ToString();
					// 	break;
					//
					// case ProgressBarFormat.Countdown:
					// 	locale = new Duration((int)(total - current)).ToCountdown().ToString();
					// 	break;

					case ProgressBarFormat.NoText: return;
				}

				_text.SetText(locale);
			}
		}

	}

}