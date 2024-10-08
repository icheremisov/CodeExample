using System.Linq;
using System.Threading;
using Client.Core.Common.Contracts;
using Client.Core.Common.UI.SystemDialog;
using Client.Core.GameFlow.Contracts;
using Client.Definitions;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLib.BuildSystem;
using XLib.UI.Screens;
using XLib.UI.Types;
using Zenject;

namespace Client.Meta.UI
{
    public class MetaGameScreen : UIScreen
    {
        public override ScreenStyle Style => ScreenStyle.Default.With(ScreenStyle.HandleAndroidBackButton, false);
     
        [Inject] private ISystemDialog _systemDialog;
        [Inject] private ILevelFlowController _levelFlowController;
        
        [SerializeField, Required, ChildGameObjectsOnly] private Button _newGameButton;
        [SerializeField, Required, ChildGameObjectsOnly] private Button _continueButton;
        [SerializeField, Required, ChildGameObjectsOnly] private Button _settingsButton;
        [SerializeField, Required, ChildGameObjectsOnly] private Button _creditsButton;
        [SerializeField, Required, ChildGameObjectsOnly] private Button _exitButton;

        [SerializeField, Required, ChildGameObjectsOnly] private TMP_Text _versionInfo;

        protected override void InitializeView()
        {
            _newGameButton.onClick.AddListener(OnNewGameButtonClicked);
            _continueButton.onClick.AddListener(() => OnContinueButtonClicked().Forget());
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            _creditsButton.onClick.AddListener(OnCreditsButtonClicked);
            _exitButton.onClick.AddListener(OnExitButtonClicked);
            _versionInfo.text = VersionService.FullVersionString;
        }

        private void OnExitButtonClicked()
        {
            _systemDialog.ShowAsync("Exit", "Are you sure you want to exit?", "Yes", "No")
                .ContinueWith(result =>
                {
                    if (result == DialogResult.Ok)
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
            			 Application.Quit();
#endif
                    }
                }).Forget();
        }

        private void OnCreditsButtonClicked() => _systemDialog.ShowAsync("Credits", "Credits are not implemented yet", "Ok").Forget();

        private void OnSettingsButtonClicked() => _systemDialog.ShowAsync("Settings", "Settings are not implemented yet", "Ok").Forget();

        private async UniTaskVoid OnContinueButtonClicked()
        {
            Debug.Log("Continue");
            await _levelFlowController.EnterLevel(new LevelArgumentData(LevelsCatalogDefinition.Instance.Levels.First()), CancellationToken.None);
        }

        private void OnNewGameButtonClicked()
        {
            _systemDialog.ShowAsync("New Game", "Are you sure you want to start a new game?", "Yes", "No")
                .ContinueWith(async result =>
                {
                    if (result == DialogResult.Ok)
                    {
                        await UniTask.Delay(1000);
                        Debug.Log("New Game");
                        await _levelFlowController.EnterLevel(new LevelArgumentData(LevelsCatalogDefinition.Instance.Levels.First()), CancellationToken.None);
                    }
                }).Forget();
        }

        
        
        protected override UniTask OnOpenAsync()
        {
            return base.OnOpenAsync();
        }
    }
}