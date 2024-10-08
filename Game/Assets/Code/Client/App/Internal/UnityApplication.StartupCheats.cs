// #if FEATURE_STARTUPCONFIG || FEATURE_DEMO
//
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Client.Core.Utils;
// using Shared.Definitions.Battle;
// using Shared.Definitions.Battle.Regimes;
// using Shared.Definitions.Finance;
// using Shared.Logic.Campaign.Modules;
// using Shared.Logic.Finance.Modules;
// using Shared.Logic.HeroPath.Modules;
// using Shared.Logic.Meta.Modules;
// using XLib.Configs.Contracts;
//
// #if FEATURE_CHEATS
// using XLib.Core.CommonTypes;
// using XLib.Unity.LocalStorage;
// #endif
//
// namespace Client.App.Internal {
//
// 	internal partial class UnityApplication {
// #if FEATURE_CHEATS
// 		public static StoredValue<Duration> OfflineTimeOffset { get; set; } = new("Cheats.OfflineTimeOffset", Duration.Zero);
// #endif
// 		private const int BigCount = 100_000_000;
// 		private const int MidCount = 100_000;
// 		private const int SmallCount = 10_000;
// 		private const int TinyCount = 1_000;
//
// 		private async Task ApplyBeforeConnectionCheats(CancellationToken ct, string playerDeviceId) {
// 			if (StartupParams.Value.ResetPlayerProfile) {
// 				await PlayerCheatHelper.DeleteAllProfiles(playerDeviceId, _serverCheats, ct);
// 			}
// 		}
//
// 		private async Task ApplyAfterConnectionCheats(CancellationToken ct) {
// #if FEATURE_CHEATS
// 			if (OfflineTimeOffset.Value != Duration.Zero) {
// 				await new Time.AddSetOffset { Offset = OfflineTimeOffset.Value };
// 				ResetOfflineTimeOffset();
// 			}
// #endif
// 			if (StartupParams.Value.FullProfile) {
// 				var finance = GameData.Once<FinanceGlobalsDefinition>();
// 				var knownResources = finance.KnownResources;
//
// 				var nonKnownItems = GameData.All<GameResourceDefinition>().Where(x => !knownResources.KnownItems.Contains(x) && x.WipFlags.IsInGame());
// 				var items = new[] {
// 						knownResources.SoftCurrency.AsItemStack(BigCount),
// 						knownResources.HardCurrency.AsItemStack(BigCount),
// 						knownResources.Gacha.AsItemStack(MidCount),
// 						knownResources.Energy.AsItemStack(TinyCount),
// 						knownResources.PVPTickets.AsItemStack(TinyCount),
// 						knownResources.LabyrinthEnergy.AsItemStack(TinyCount),
// 					}
// 					.Concat(nonKnownItems.Select(x => x.AsItemStack(SmallCount)))
// 					.ToArray();
//
// 				await new Inventory.SetItems { Items = items };
// 				ct.ThrowIfCancellationRequested();
//
// 				var heroes = GameData.Once<CharactersDefinition>();
// 				await new Heroes.SummonHeroCheat { Heroes = heroes.PlayableCharacters.ToArray() };
// 				ct.ThrowIfCancellationRequested();
//
// 				var coins = heroes.PlayableCharacters.Select(character => new InventoryItemStack(knownResources.GetCoin(character.Rarity), 100).PreferTargetId(character.Id));
// 				await new Inventory.SetItems { Items = coins.ToArray() };
// 				ct.ThrowIfCancellationRequested();
//
// 				await new HeroPathModule.ForceStartRequestCheat();
// 			}
//
// 			if (StartupParams.Value.UnlockRegimes || StartupParams.Value.FullProfile) {
// 				await new Regimes.UnlockRegimesCheat { Regimes = GameData.All<RegimeDefinition>().ToArray() };
// 				ct.ThrowIfCancellationRequested();
// 			}
//
// 			if (StartupParams.Value.CompleteCampaign) {
// 				await new CampaignModule.CompleteChaptersCheat { Chapters = RegimesDefinition.Instance.Campaign.Chapters };
// 				ct.ThrowIfCancellationRequested();
// 			}
// 		}
//
// #if FEATURE_CHEATS
// 		public Duration AddOfflineTimeOffset(Duration duration) {
// 			if (duration == Duration.Zero) return OfflineTimeOffset.Value;
// 			return OfflineTimeOffset.Value += duration;
// 		}
//
// 		public void ResetOfflineTimeOffset() {
// 			OfflineTimeOffset.Value = Duration.Zero;
// 		}
// #endif
// 	}
//
// }
//
// #endif