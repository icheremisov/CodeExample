using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using XLib.Core.Collections;


namespace Client.Core.Common.UI.SystemNotification {

	public class SystemNotificationView : MonoBehaviour {
		[SerializeField, Required] private GameObject _viewRoot;
		[SerializeField, Required] private CanvasGroup _canvasGroup;
		[SerializeField, Required] private TMP_Text _title;
		[SerializeField, Required] private TMP_Text _text;
		[SerializeField, Required] private float _duration = 2f;
		[SerializeField, Required] private float _fadeDuration = 0.3f;

		private int _currentHash;
		private readonly OrderedDictionary<int, (string, string)> _notificationsQueue = new();
		
		public void SetVisible(bool v) => _viewRoot.SetActive(v);

		public void Show(string text) {
			var hash = text.ToString().GetHashCode();
			
			if (_currentHash == hash || _notificationsQueue.ContainsKey(hash)) return;
			
			_notificationsQueue.Add(hash, (null, text));

			TryShowNextNotification();
		}
		
		public void Show(string title, string text) {
			var hash = HashCode.Combine(text.ToString().GetHashCode(), text.ToString().GetHashCode());
			
			if (_currentHash == hash || _notificationsQueue.ContainsKey(hash)) return;
			
			_notificationsQueue.Add(hash, (title, text));

			TryShowNextNotification();
		}

		private void TryShowNextNotification() {
			if (_viewRoot.activeSelf) return;
			if (_notificationsQueue.IsNullOrEmpty()) return;

			var (hash, (title, text)) = _notificationsQueue.First();
			_notificationsQueue.Remove(hash);

			_title.SetActive(title != null);
			if (title != null) _title.text = title;
			_text.text = text;

			_currentHash = hash;
			Show().Forget();
		}
		
		private async UniTask Show() {
			_canvasGroup.alpha = 0;
			
			SetVisible(true);
			
			await _canvasGroup.DOFade(1f, _fadeDuration);
			
			await UniEx.DelaySec(_duration - _fadeDuration * 2f);
			
			await _canvasGroup.DOFade(0, _fadeDuration);
			
			SetVisible(false);
			_currentHash = 0;
			
			TryShowNextNotification();
		}
	}

}