using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Assets.Configs;
using XLib.Core.Reflection;
using Zenject;

namespace Client.Core.Configs {

	[CreateAssetMenu(menuName = "Configs/Client", fileName = "ClientConfig", order = 0)]
	public class ClientConfigHolder : SerializedScriptableObject, IBaseHolder<ClientConfig> {

		public const string AssetName = "ClientConfig";

		[HideLabel, InlineProperty, SerializeField, Required]
		private ClientConfig _item;

		public ClientConfig Item => _item;

		public void BindSelf(DiContainer container) {
			container.Bind<ClientConfig>().FromInstance(_item);

			foreach (var property in _item.GetType().EnumerateInstanceProperties(false)) 
				container.Bind(property.PropertyType).FromInstance(property.GetValue(_item));
		}

		private void OnEnable() => _item.OnEnable();
	}

}