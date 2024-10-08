using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Screens;
using XLib.UI.Types;

namespace Client.Meta.UI
{
    public class DialogFullscreenScreen : UIScreen, IUIScreenSetup<DialogFullscreenScreen.Args>
    {
        [SerializeField, Required] private TMP_Text _header;
        [SerializeField, Required] private TMP_Text _desc;
        [SerializeField, Required] private Image _image;
        [SerializeField, Required] private Image _icon;
        [SerializeField, Required] private Transform _goRef;

        private Args _args;

        public struct Args : IScreenArgs
        {
            public string Header { get; set; }
            public string Desc { get; set; }
            public Sprite Sprite { get; set; }
            public Sprite SpriteIcon { get; set; }
            public Color SpriteIconTint { get; set; }
            public GameObject GameObject { get; set; }
        }

        public override ScreenStyle Style => ScreenStyle.Default | ScreenStyle.UnloadOnClose;

        public UniTask SetupScreen(Args args)
        {
            _args = args;
            return UniTask.CompletedTask;
        }

        protected override UniTask OnOpenAsync()
        {
            UpdateView();
            return base.OnOpenAsync();
        }

        private void UpdateView()
        {
            if (_args.Header != null)
            {
                _header.text = _args.Header;
                _header.SetActive(true);
            }
            else
            {
                _header.SetActive(false);
            }

            if (_args.Desc != null)
            {
                _desc.text = _args.Desc;
                _desc.SetActive(true);
            }
            else
            {
                _desc.SetActive(false);
            }

            _image.SetSprite(_args.Sprite);
            _icon.SetSprite(_args.SpriteIcon);
            _icon.color = _args.SpriteIconTint;

            _goRef.DestroyAllChildren();
            if (_args.GameObject != null)
            {
                Instantiate(_args.GameObject, _goRef);
                _goRef.SetActive(true);
            }
            else
            {
                _goRef.SetActive(false);
            }
        }
    }
}