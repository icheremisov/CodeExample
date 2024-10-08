// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using XLib.BuildSystem;
// using Client.Core.Common.Scene;
// using Client.Core.Common.UI.SystemDialog;
// using Client.Core.Crashlitycs;
// using DG.Tweening;
// using XLib.Core.CommonTypes;
// using XLib.Unity.LocalStorage;
// using SystemInfo = UnityEngine.Device.SystemInfo;
//
// #if UNITY_EDITOR && DEVELOPMENT_BUILD
// using Client.Core.Dialogs.Helpers;
// using Shared.Definitions.Dialogs;
// using XLib.Unity.Core;
// #endif
//
// namespace Client.App.Internal {
//
// 	internal partial class UnityApplication : INetworkActionHandler {
// 		private class ConnectionInfo {
// 			public NetworkInfo NetworkInfo { get; set; }
// 			public ConnectOptions ConnectOptions { get; set; }
// 			public TransportEndpoint Endpoint { get; set; }
// 		}
//
// 		public class PrevNetworkExceptionData {
// 			public ErrorCode ExceptionCode;
// 			public Timestamp ExceptionTimeStamp;
// 		}
//
// #if PLATFORM_STANDALONE && !UNITY_EDITOR
// 		private static readonly StoredValue<StartupGameParams> StartupParams = new("Standalone.StartupGameParams", new StartupGameParams());
// #else
// 		private static readonly StoredValue<StartupGameParams> StartupParams = new("StartupGameParams", new StartupGameParams());
// #endif
//
// 		public static readonly StoredValue<bool> IsDeveloper = new("IsDeveloper", false);
//
// 		private static bool _forceShowConnectionScreen;
// 		private readonly HashSet<ChannelKind> _waitingScreenChannels = new();
//
// 		private NetworkException _networkException;
// 		private static readonly StoredValue<PrevNetworkExceptionData> PrevNetworkException = new("UnityApplication_PrevNetworkError", null);
//
// 		private async UniTask TryDownloadEndpoints(CancellationToken ct) {
// 			try {
// 				await _announceService.DownloadInfo(StartupGameParams.TargetGatewayInfo, ct);
// 				var info = _announceService.GetGatewayInfo(StartupGameParams.TargetGatewayInfo);
// 				_networkConfig.ChannelConfig.SetEndpoints(info);
// 			}
// 			catch (OperationCanceledException) {
// 				throw;
// 			}
// 			catch (Exception ex) {
// 				_appAnalyticsEvents.OnServerAnnounceFail(Network.HasValue ? Network.Value.ServerEnvironment : string.Empty, ex.Message);
// 				NetLogger.LogWarning(
// 					$"Can't download gateway info from internet! Target gateway info {StartupGameParams.TargetGatewayInfo}. So.. playing with defaults. ServerEnv:'{Network}'. Default:'{StartupGameParams.DefaultServerEnvironment}' {ex.ToLog()}");
// 			}
// 		}
//
// 		private async UniTask<bool> SelectServerEnvironment(CancellationToken ct) {
// 			try {
// #if UNITY_EDITOR && DEVELOPMENT_BUILD
// 				if (GameLoader.Params != null) Network = NetworkInfo.BuiltIn;
//
// 				if (GameLoader.HasParams<TestDialogStartupParams>()) {
// 					_gameStateMachine.EnterAsync<TestDialogState>(ct).Forget();
// 					await UniTask.WaitUntilCanceled(ct);
// 					ct.ThrowIfCancellationRequested();
// 				}
// #endif
//
// 				// select default server
// #if FEATURE_STARTUPCONFIG
// 				if (Network == null) {
// 					Network = new NetworkInfo(StartupGameParams.DefaultServerEnvironment);
//
// 					var args = new SetupConnectionScreen.Args() { Startup = StartupParams.Value, ForceShow = _forceShowConnectionScreen };
// 					_forceShowConnectionScreen = false;
//
// 					await _screenManager.Screen<SetupConnectionScreen>().OpenAndWait(args);
// 					StartupParams.Save();
//
// 					Network = new NetworkInfo(StartupParams.Value.ServerEnvironment);
//
// 					Debug.Log($"[Network] Select ServerEnvironment => [{StartupParams.Value.ServerEnvironment}]");
// #if FEATURE_DISABLESERVER
// 					Network = NetworkInfo.BuiltIn;
// #endif
//
// 					if (StartupParams.Value.UseCustomBuildVersion) VersionService.VersionCode = StartupParams.Value.CustomBuildVersion;
// 				}
// #else
// 				Network = new NetworkInfo(_announceService.GetTargetServer(StartupGameParams.TargetGatewayInfo, VersionService.VersionCode));
// #endif
//
// #if FEATURE_DEMO
// 				{
// 					var result = await _screenManager.Screen<SelectDemoModeScreen, SelectDemoModeScreen.Result>().ShowAndWaitResult();
// 					StartupParams.Value.ResetPlayerProfile = result.StartNew;
// 				}
// #endif
//
// #if !FEATURE_DISABLESERVER
// 				if (!IsServerEnvironmentExists()) {
// 					Network = new NetworkInfo(_announceService.GetTargetServer(StartupGameParams.TargetGatewayInfo, VersionService.VersionCode));
// 				}
//
// 				if (!IsCanPlayOnChosenServerEnvironment()) {
// 					await HandleNetworkError(new NetworkException(ErrorCodes.ServerMaintenanceModeError), CancellationToken.None);
// 					return false;
// 				}
// #endif
// 				Debug.Assert(Network != null);
// 				NetLogger.Log($"NetworkInfo={Network} VersionCode={VersionService.VersionCode}");
// 				return true;
// 			}
// 			catch (OperationCanceledException) {
// 				throw;
// 			}
// 			catch (NetworkException ex) {
// 				await HandleNetworkError(ex, ct);
// 				return false;
// 			}
// 			catch (Exception ex) {
// 				await HandleNetworkError(new ClientErrorException(ex), ct);
// 				return false;
// 			}
// 		}
//
// #if !FEATURE_DISABLESERVER
// 		private bool IsServerEnvironmentExists() {
// 			Debug.Assert(Network != null);
// 			if (Network.Value.SkipChecks) return true;
//
// 			return _announceService.IsEnvironmentExists(StartupGameParams.TargetGatewayInfo, Network.Value.ServerEnvironment);
// 		}
//
// 		private bool IsCanPlayOnChosenServerEnvironment() {
// 			Debug.Assert(Network != null);
// 			if (Network.Value.SkipChecks) return true;
// 			if (!IsServerEnvironmentExists()) {
// 				_appAnalyticsEvents.OnServerAnnounceFail(Network.Value.ServerEnvironment);
// 				return false;
// 			}
//
// 			var isMaintenanceModeOff = !_announceService.IsMaintenanceModeOn(StartupGameParams.TargetGatewayInfo, Network.Value.ServerEnvironment);
// 			return isMaintenanceModeOff;
// 		}
// #endif
//
// 		private async UniTask<ConnectionInfo> GetConnectionInfo(CancellationToken ct) {
// 			Debug.Assert(Network != null);
// 			Debug.Assert(!DeviceId.IsNullOrEmpty());
//
// 			var tokens = new List<AuthToken>();
//
// 			var gameParams = StartupParams.Value;
// 			if (gameParams.UseCustomProfileId) {
// 				if (long.TryParse(gameParams.CustomProfileId, out _))
// 					tokens.Add(AuthToken.ProfileId(gameParams.CustomProfileId));
// 				else if (gameParams.CustomProfileId.StartsWith("#")) {
// 					tokens.Add(AuthToken.ProfileId(gameParams.CustomProfileId[1..]));
// 				} else tokens.Add(AuthToken.DeviceId(gameParams.CustomProfileId));
// 			}
// 			else {
// 				tokens.Add(AuthToken.DeviceId(DeviceId));
// 			}
//
// 			var loginData = await _loginController.TryDoLogin(ct);
// 			if (loginData != null) tokens.AddRange(loginData);
// 			_appAnalyticsEvents.SendLoadingState("60");
//
// 			var version = SharedVersion.Parse($"{VersionService.ShortVersionString} #{VersionService.VersionCode}");
// 			var skipTutorial = SkipTutorialMode.None
// 				.With(SkipTutorialMode.Main, gameParams.SkipTutorial || gameParams.FullProfile)
// 				.With(SkipTutorialMode.Soft, gameParams.SkipSoftTutorial);
//
// 			var options = new ConnectOptions(version, tokens) {
// 				SkipTutorial = skipTutorial, OnAuthSuccess = OnAuthSuccess, 
// 				RealDeviceId = SystemInfo.deviceUniqueIdentifier
// 			};
//
// 			TransportEndpoint endpoint;
// 			if (!Network.Value.IsCustom) {
// 				endpoint = _networkConfig.ChannelConfig.GetEndpoint(Network.Value.ServerEnvironment);
// 			}
// 			else {
// 				if (gameParams.CustomServerURL.IsNullOrEmpty()) throw new Exception($"CustomServerURL not set for {Network}");
// 				endpoint = new TransportEndpoint(TransportType.WebSocket, gameParams.CustomServerURL);
// 				Debug.Log($"Using custom server url {gameParams.CustomServerURL}");
// 			}
//
// 			return new ConnectionInfo() { ConnectOptions = options, Endpoint = endpoint, NetworkInfo = Network.Value };
// 		}
//
// 		private async Task OnAuthSuccess(Status.AuthResponse authResponse) {
// 			var gameParams = StartupParams.Value;
// 			Debug.Log($"Server config hash: {authResponse.ConfigHash}");
//
// 			var bundleInfo = gameParams.UseCustomBundleVersion
// 				? new BundleVersionInfo { 
// 					version = $"{gameParams.CustomBundleClientVersion}_{gameParams.CustomBundleBuildNumber}", 
// 					config_hash = authResponse.ConfigHash, rev = gameParams.CustomBundleBuildNumber }
// 				: _bundlesCatalogService.GetBundles().Where(info => info.config_hash == authResponse.ConfigHash).MaxByOrDefault(info => info.rev);
//
// 			if (bundleInfo != null) {
// 				await _assetProvider.InitializeCatalogAsync(bundleInfo.version, bundleInfo.config_hash, CancellationToken.None);
// 				await _gameDatabaseProvider.LoadGameDatabase();
// 			}
// 			else
// 				Debug.Log($"Not found bundles: {authResponse.ConfigHash}");
//
// 			_loginController.OnAuthSuccess(authResponse);
// 		}
//
// 		private async UniTask<bool> ConnectToServer(ConnectionInfo connectionInfo, CancellationToken ct) {
// 			Tween barAnimTween = null;
// 			try {
// 				FirebaseWrapper.SetConnectionInfo(connectionInfo.ConnectOptions, DeviceId, connectionInfo.NetworkInfo.ServerEnvironment);
//
// 				barAnimTween = DOVirtual.Float(0.3f, 0.5f, NetConstants.StartupWaitConnectionSec, x => _loadingScreen.Report(x))
// 					.SetUpdate(false)
// 					.SetEase(Ease.Linear);
//
// 				_appAnalyticsEvents.SendLoginStart(connectionInfo.NetworkInfo.ServerEnvironment);
//
// 				_networkController.ActionHandler = this;
//
// 				_networkController.SetNetworkInfo(connectionInfo.NetworkInfo);
// 				_networkController.SetConnectOptions(connectionInfo.ConnectOptions);
//
// 				_appAnalyticsEvents.SendLoadingState("80");
//
// 				await _networkController.Start(connectionInfo.Endpoint, ct);
//
// 				_appAnalyticsEvents.SendLoadingState("90");
//
// 				var metaHost = _sharedLogicService.GetHost<IMetaHost>();
// 				_globalContext.ReplaceMetaHost(metaHost);
//
// #if FEATURE_DEMO
// 				LocalProfileStorage.LoadProfile(metaHost.Id, $"{LocalProfileStorage.ProfileKey}@demo");
// #else
// 				LocalProfileStorage.LoadProfile(metaHost.Id);
// #endif
//
// #if FEATURE_STARTUPCONFIG || FEATURE_DEMO
// 				await ApplyAfterConnectionCheats(ct);
// #endif
//
// 				await TryStartAnalyticsService(metaHost, ct);
//
// 				_appAnalyticsEvents.SendFirstLoginEvents(metaHost);
// 				_appAnalyticsEvents.TrySendEndBattleEvent(true);
//
// 				if (metaHost.GetModule<Status>().NotificationsEnabled) {
// 					_deviceNotificationsController.SetEnabled(true);
// 					await _deviceNotificationsController.RequestNotificationPermissions();
// 				}
// 				else {
// 					_deviceNotificationsController.SetEnabled(false);
// 					_appAnalyticsEvents.SetPushEnabled(false);
// 				}
//
// 				_appAnalyticsEvents.SendLogin(metaHost, connectionInfo.NetworkInfo.ServerEnvironment, DetectLoginSourceData());
//
// 				return true;
// 			}
// 			catch (OperationCanceledException) {
// 				throw;
// 			}
// 			catch (NetworkException) {
// 				// handled in HandleNetworkError
// 				return false;
// 			}
// 			catch (Exception ex) {
// 				await HandleNetworkError(new ClientErrorException(ex), ct);
// 				return false;
// 			}
// 			finally {
// 				barAnimTween?.Kill();
// 			}
// 		}
//
// 		private async UniTask TryStartAnalyticsService(IMetaHost metaHost, CancellationToken ct) {
// 			var isDeveloper = metaHost.GetModule<Status>(true).IsDeveloper;
//
// 			if (isDeveloper != IsDeveloper.Value) {
// 				IsDeveloper.Value = isDeveloper;
//
// 				if (isDeveloper)
// 					await _analyticsService.Stop(ct);
// 				else
// 					await _analyticsService.Start(ct);
// 			}
// 		}
//
// 		private LoginSourceData DetectLoginSourceData() {
// 			var result = new LoginSourceData();
// 			if (PrevNetworkException.Value != null) {
// 				result.ExceptionTimeStamp = PrevNetworkException.Value.ExceptionTimeStamp;
// 				result.LoginSourceType = PrevNetworkException.Value.ExceptionCode == ClientErrorCodes.RequestTimeout ||
// 					PrevNetworkException.Value.ExceptionCode == ClientErrorCodes.InactiveTimeout
// 						? LoginSourceType.Timeout
// 						: LoginSourceType.Error;
// 				PrevNetworkException.Value = null;
// 				return result;
// 			}
//
// 			result.LoginSourceType = LoginSourceType.AppStart;
// 			return result;
// 		}
//
// 		public async Task HandleResetProfile(SharedVersion fromVersion, IEnumerable<InventoryItemStack> rewards, CancellationToken ct) {
// 			PlayerPrefs.DeleteKey("OffersShown");
// 			PlayerPrefs.DeleteKey("UnityApplication_PrevNetworkError");
// 			LocalProfileStorage.DeleteAllKeys();
// 			
// 			await _screenManager.Screen<MigrationScreen>().OpenAndWait(new MigrationScreen.Args() {
// 				Items = rewards,
// 				FromVersion = fromVersion,
// 			});
// 		}
//
// 		public Task HandleWaitConnection(ChannelKind from, bool isWaiting, CancellationToken ct) {
// 			if (isWaiting)
// 				_waitingScreenChannels.Add(from);
// 			else
// 				_waitingScreenChannels.Remove(from);
//
// 			_blockerView.SetVisible(NetworkLockTag, _waitingScreenChannels.Count > 0);
//
// 			return Task.CompletedTask;
// 		}
//
// 		public Task HandleNetworkError(NetworkException exception, CancellationToken ct) {
// 			if (_networkException == null) {
// 				_networkException = exception;
// 				PrevNetworkException.Value = new PrevNetworkExceptionData { ExceptionCode = _networkException.Code, ExceptionTimeStamp = Timestamp.DeviceTime };
// 				_evRestartGame.FireEvent();
// 			}
//
// 			return Task.CompletedTask;
// 		}
//
// 		private async UniTask ShowNetworkErrorToUser(CancellationToken ct) {
// 			if (_networkException == null) return;
// 			var error = _networkException;
//
// 			_blockerView.Close(NetworkLockTag);
// 			_waitingScreenChannels.Clear();
//
// 			switch (error) {
// 				case InactiveTimeoutException:
// 					// skip with no dialog - go to reconnect instantly
// 					break;
//
// 				case TransportException ex 
// 					when ex.OperationCode is NetworkOperationCode.InvalidProtocolVersion
// 					or NetworkOperationCode.UnsupportedProtocol 
// 					or NetworkOperationCode.UnsupportedVersion: {
// 					AppLogger.LogWarning(error.Dump(NetworkException.Details.Full));
// 					await UpdateClientDialog(error, ct); 
// 					break;
// 				}
//
// 				case ConnectionFailedException:
// 				case RequestTimeoutException:
// 				case TransportException: {
// 					AppLogger.LogWarning(error.Dump(NetworkException.Details.Full));
// 					await ShowErrorDialog(error, SystemKeys.ConnectionErrorTitle, SystemKeys.ConnectionErrorText, SystemKeys.ConnectionErrorButton, ct);
// 					PrevNetworkException.Value = new PrevNetworkExceptionData { ExceptionCode = _networkException.Code, ExceptionTimeStamp = Timestamp.DeviceTime };
// 					break;
// 				}
//
// 				default: {
// 					if (error.Code == ErrorCodes.ServerVersionError || error.Code == ErrorCodes.ServerUpdatedContractsError ||
// 						error.Code == ErrorCodes.ServerProtocolVersionError) {
// 						AppLogger.LogWarning(error.Dump(NetworkException.Details.Full));
// 						await UpdateClientDialog(error, ct);
// 					}
// 					else if (error.Code == ErrorCodes.ServerMaintenanceModeError) {
// 						AppLogger.LogWarning(error.Dump(NetworkException.Details.Full));
// 						await ShowErrorDialog(error, SystemKeys.MaintenanceErrorTitle, SystemKeys.MaintenanceErrorText, SystemKeys.MaintenanceErrorButton, ct);
// 					}
// 					else if (error.Code == ErrorCodes.ServerSecondDeviceError) {
// 						AppLogger.LogWarning(error.Dump(NetworkException.Details.Full));
// 						await ShowErrorDialog(error, SystemKeys.AnotherSessionErrorTitle, SystemKeys.AnotherSessionErrorText, SystemKeys.AnotherSessionErrorButton, ct);
// 					}
// 					else if (error.Code == ClientErrorCodes.ConnectionFailed || error.Code == ClientErrorCodes.GatewayInfoNotFound) {
// 						AppLogger.LogWarning(error.Dump(NetworkException.Details.Full));
// 						await ShowErrorDialog(error, SystemKeys.ConnectionErrorTitle, SystemKeys.ConnectionErrorText, SystemKeys.ConnectionErrorButton, ct);
// 					}
// 					else {
// 						PrevNetworkException.Value = new PrevNetworkExceptionData { ExceptionCode = _networkException.Code, ExceptionTimeStamp = Timestamp.DeviceTime };
// 						LogNetworkError(error);
//
// 						_appAnalyticsEvents.OnDisconnect(error.Code, error.Message);
// 						await ShowErrorDialog(error, SystemKeys.UnknownErrorTitle, SystemKeys.UnknownErrorText, SystemKeys.UnknownErrorButton, ct);
// 					}
//
// 					break;
// 				}
// 			}
//
// 			_networkException = null;
// 		}
//
// 		private async UniTask UpdateClientDialog(NetworkException error, CancellationToken ct) {
// 			await ShowErrorDialog(error, SystemKeys.UpdateClientErrorTitle, SystemKeys.UpdateClientErrorText, SystemKeys.UpdateClientErrorButton, ct);
// 			await UniEx.DelaySec(1, ct);
// 			Application.Quit();
// 		}
//
// 		private static void LogNetworkError(Exception error) {
// 			if (error is ClientErrorException && error.InnerException != null) error = error.InnerException;
//
// 			AppLogger.LogError((error as NetworkException)?.Dump(NetworkException.Details.Full) ?? error.ToString());
// 			FirebaseWrapper.LogException(error);
// 		}
//
// 		private async UniTask ShowErrorDialog(NetworkException error, LocKey title, LocKey text, LocKey button, CancellationToken ct) {
// 			ILocString body = text;
//
// #if DEVELOPMENT_BUILD
// 			body = $"{body}\n\n{error.Dump(NetworkException.Details.Short)}".AsLocString();
// #endif
// 			await _systemDialog.ShowAsync(title, body, button, DialogType.SystemOverlay, ct);
// 		}
// 	}
//
// }