using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using XLib.Core.Utils;

namespace XLib.UI.Controls
{
	[RequireComponent(typeof(Image))]
	public class UISpriteAnimation : MonoBehaviour
	{
		public enum PlayMode
		{
			Single,
			Loop,
			PingPong,
		}

		[SerializeField] private PlayMode _mode = PlayMode.Loop;
		[SerializeField] private float _fps = 1.0f;

		[Space]
		[SerializeField, Required, ListDrawerSettings(DefaultExpandedState = true)] private Sprite[] _frames;


		private Image _image;

		private int _currentFrame = 0;
		private float _timer = 0.0f;

		private bool _isPlaying = false;

		private int _playDirection = 1;

		private void Awake()
		{
			_image = this.GetExistingComponent<Image>();
		}

		private void OnEnable()
		{
			_currentFrame = 0;
			_timer = 0.0f;

			_isPlaying = true;

			_playDirection = 1;

			SetSprite(_currentFrame);
		}

		private void SetSprite(int sprite)
		{
			_currentFrame = sprite;
			if (_image) _image.sprite = _frames.IsValidIndex(_currentFrame) ? _frames[_currentFrame] : null;
		}

		private void OnDisable()
		{
			_isPlaying = false;
		}

		private void Update()
		{
			if (!_isPlaying || _frames.Length == 0) return;

			_timer += Time.unscaledDeltaTime;

			var secPerFrame = 1.0f / _fps;

			var frameCount = (int)(_timer / secPerFrame);

			if (frameCount > 0)
			{
				_timer = Mathf.Max(0, _timer - frameCount);

				switch (_mode)
				{
					case PlayMode.Single:
						{
							var newFrame = MathEx.Clamp(_currentFrame + frameCount * _playDirection, 0, _frames.Length - 1);

							if (_currentFrame != newFrame)
							{
								SetSprite(newFrame);
							}
						}
						break;
					case PlayMode.Loop:
						{
							var newFrame = (_currentFrame + frameCount * _playDirection + _frames.Length) % _frames.Length;

							if (_currentFrame != newFrame)
							{
								SetSprite(newFrame);
							}
						}
						break;
					case PlayMode.PingPong:
						{
							var newFrame = _currentFrame + frameCount * _playDirection;

							for (int i = 0; i < 10; i++)
							{
								var changed = false;
								if (newFrame < 0)
								{
									_playDirection = -_playDirection;
									newFrame = -newFrame;
									changed = true;
								}
								if (newFrame >= _frames.Length)
								{
									_playDirection = -_playDirection;
									newFrame = 2 * _frames.Length - newFrame - 1;
									changed = true;
								}
								if (!changed) break;
							}

							if (_currentFrame != newFrame)
							{
								SetSprite(MathEx.Clamp(newFrame, 0, _frames.Length - 1));
							}
						}
						break;
				}
			}
		}
	}
}
