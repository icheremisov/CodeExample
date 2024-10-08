// using Client.Cheats.Contracts;
// using Client.Cheats.Internal;
// using Client.Meta.Finance.Contracts;
// using Cysharp.Threading.Tasks;
// using JetBrains.Annotations;
// using Shared.Definitions.Core;
// using Shared.Definitions.Finance;
// using Shared.Definitions.Finance.Containers;
// using Shared.Logic.Finance.Modules;
// using UnityEngine;
// using XLib.Configs.Core;
// using XLib.Configs.Inventory;
// using XLib.Core.Utils;
//
// namespace Client.Cheats.Common {
//
// 	[PublicAPI, CheatCategory("Common")]
// 	public static class ResourcesCheats {
// 		public const string ResourcesHeroCoins = "Resources/Hero coins";
//
// 		[CheatPluginGUI("Resources/Common", KeyCode.R)]
// 		private static void CommonResources(FinanceGlobalsDefinition globalsDefinition, Inventory inventory, ILockable lockable) {
// 			var configs = globalsDefinition.KnownResources;
// 			Item(configs.SoftCurrency, inventory, lockable);
// 			Item(configs.HardCurrency, inventory, lockable);
// 			Item(configs.Energy, inventory, lockable);
// 			Item(configs.Gacha, inventory, lockable);
// 			Item(configs.PVPTickets, inventory, lockable);
// 			Item(configs.PVPMedals, inventory, lockable);
// 			Item(configs.LabyrinthEnergy, inventory, lockable);
// 			Item(configs.HeroPathPoints, inventory, lockable);
// 		}
//
// 		[CheatPluginGUI("Resources/Game Resources")]
// 		private static void Resource(GameResourceDefinition resource, Inventory inventory, ILockable lockable) {
// 			if (resource.WithTargetItem) return;
// 			Item(resource, inventory, lockable);
// 		}
//
// 		private static void Item(GameItem item, Inventory inventory, ILockable lockable) {
// 			var count = inventory.GetCount(item.AsItemStack());
//
// 			if (CheatGui.Input(item.FileName, item.InventoryIcon, count, out var newValue, new RangeInt(0, item.MaxCount), 1, 10, 100, 1000, 10000))
// 				new Inventory.SetItems() { Items = new[] { item.AsItemStack(newValue) } }.Send(lockable);
// 		}
//
// 		[CheatPluginGUI("Resources/Hero Experience")]
// 		private static void HeroExperience(HeroExperienceContainerDefinition exp, Inventory inventory, ILockable lockable) => Item(exp, inventory, lockable);
//
// 		[CheatPluginGUI("Resources/Equipment Experience")]
// 		private static void EquipmentExperience(EquipmentExperienceContainerDefinition exp, Inventory inventory, ILockable lockable) => Item(exp, inventory, lockable);
//
// 		[CheatPluginGUI("Resources/Chest")]
// 		private static void Chest(ChestDefinition chest, Inventory inventory, ILockable lockable) {
// 			Item(chest, inventory, lockable);
// 		}
//
// 		[CheatPluginGUI("Resources/Containers")]
// 		private static void HeroExperience(ContainerDefinition container, Inventory inventory, ILockable lockable, IFinanceController financeController) {
// 			if (CheatGui.InputDelta(container.FileName, out var changed, 1, 10, 100)) {
// 				AddItems(financeController, new[] { container.AsItemStack(changed) }).Forget();
// 			}
// 		}
//
// 		private static async UniTask AddItems(IFinanceController financeController, InventoryItemStack[] stacks) {
// 			Cheat.Minimize();
// 			await financeController.AddItems(stacks);
// 			Cheat.Maximize();
// 		}
//
// 		[CheatPluginGUI(ResourcesHeroCoins)]
// 		private static void HeroCoins(CharacterCoreDefinition hero, Inventory inventory, ILockable lockable) {
// 			var item = hero.ToHeroCoin();
// 			var count = inventory.GetCount(item);
//
// 			if (CheatGui.Input(hero.FileName, hero.InventoryIcon, count, out var newValue, new RangeInt(0, item.As<IInventoryItem>().MaxCount), 1, 10, 100, 1000, 10000))
// 				new Inventory.SetItems() { Items = new[] { item.WithAmount(newValue) } }.Send(lockable);
// 		}
// 	}
//
// }