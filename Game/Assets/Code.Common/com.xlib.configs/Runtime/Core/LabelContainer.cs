using System;
using System.Collections.Generic;
using UnityEngine;
using XLib.Configs.Contracts;

namespace XLib.Configs.Core {

	[Serializable]
	public class TagConstants {
		[SerializeField] private LabelItem _buff;
		[SerializeField] private LabelItem _debuff;
		[SerializeField] private LabelItem _attack;
		[SerializeField] private LabelItem _controll;

		public LabelItem Buff => _buff;
		public LabelItem Debuff => _debuff;
		public LabelItem Attack => _attack;
		public LabelItem Control => _controll;
	}

	[ContainerConfig(WithCustomName = true, WithColor = true, AllowCopyPaste = false, AllowReorder = false)]
	public class LabelContainer : GameItemBaseContainer<LabelItem>, IGameItemSingleton {
		[SerializeField]
		private TagConstants _constants;
		public TagConstants Constants => _constants;
		
		public IEnumerable<LabelItem> BuffOrDebuff {
			get {
				yield return Constants.Buff;
				yield return Constants.Debuff;
			}
		}
	}

}