using Client.Core.Common.Contracts;
using Client.Core.GameFlow.Contracts;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using XLib.UI.Screens;
using XLib.UI.Types;
using Zenject;

namespace Client.Levels.UI
{
    public class LevelScreen : UIScreen
    {
        public override ScreenStyle Style => ScreenStyle.Default;

        [SerializeField, Required, ChildGameObjectsOnly] private Button _closeButton;

        [Inject] private LevelContext _levelContext;
        [Inject] private ILevelFlowController _levelFlowController;
        [Inject] private ISystemDialog _systemDialog;
        protected override void InitializeView()
        {
            base.InitializeView();
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnCloseButtonClicked() => _levelContext.ExitClick(this, _levelFlowController, _systemDialog).Forget();
    }
}