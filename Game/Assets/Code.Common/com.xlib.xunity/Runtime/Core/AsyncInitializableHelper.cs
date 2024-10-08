using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLib.Core.Reflection;
using XLib.Unity.Core.Attributes;
using Zenject;

namespace XLib.Unity.Core {

	public static class AsyncInitializableHelper {

		private static readonly List<IAsyncInitializable> ReadyItems = new(32);
		private static readonly List<IAsyncInitializable> PendingItems = new(32);

		public static void QueueAsyncInitializers(this DiContainer container) {
			foreach (var item in container.ResolveAll<IAsyncInitializable>()
						 .Where(item => !ReadyItems.Contains(item))) 
				PendingItems.AddOnce(item);
		}

		public static async UniTask InitializeAsync(CancellationToken ct = default) {
			var items = PendingItems.ToArray();
			PendingItems.Clear();
			ReadyItems.AddRange(items);

			foreach (var group in items.Select(item => (order: GetOrder(item), item)).GroupBy(x => x.order).OrderBy(x => x.Key)) {
				await UniTask.WhenAll(group.Select(item => {
					Debug.Log($"InitializeAsync: {item.item.GetType().Name}");
					return item.item.InitializeAsync(ct);
				}));
			}
		}

		private static int GetOrder(IAsyncInitializable item) => item.GetType().GetAttribute<AsyncInitOrderAttribute>()?.Order ?? 0;

	}

}