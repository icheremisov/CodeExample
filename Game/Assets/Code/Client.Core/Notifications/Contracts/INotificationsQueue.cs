using System.Collections.Generic;
using System.Threading;
using Client.Core.Notifications.Types;
using Cysharp.Threading.Tasks;
namespace Client.Core.Notifications.Contracts {

	public interface INotificationsQueue {

		// UniTask TryShowRewards(IErrorOrResult response, NotifyOptions options = NotifyOptions.Default, NotifyParams? notifyParams = null, CancellationToken ct = default);
		// UniTask ShowRewards(IEnumerable<InventoryItemStack> items, NotifyOptions options = NotifyOptions.Default, NotifyParams? notifyParams = null, CancellationToken ct = default);
		//
		// UniTask TryShowNotEnoughResources(IErrorOrResult response, NotifyOptions options = NotifyOptions.Default, CancellationToken ct = default);
		// UniTask ShowNotEnoughResources(IEnumerable<InventoryItemStack> notEnoughItems, NotifyOptions options = NotifyOptions.Default, CancellationToken ct = default);

	}

}