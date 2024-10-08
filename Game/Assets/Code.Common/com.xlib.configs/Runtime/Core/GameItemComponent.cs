using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.Configs.Contracts;
using XLib.Configs.Sheets.Contracts;
using XLib.Core.Reflection;
using XLib.Core.Utils;

namespace XLib.Configs.Core {

	public abstract partial class GameItemComponent : GameItemOrComponent, IComparable<GameItemComponent>, IGameItemComponent, IOrderBy {
		[SerializeField, HideInInspector, ShtIgnore]
		protected FileId _fileId;

		[SerializeField, ReadOnly, ShtIgnore, Required]
		protected GameItemBase _owner;
		private string _assetName;
		public T GetOwner<T>() where T : GameItemBaseContainer => (T)_owner;

		[ShtIgnore]
		public FileId Id { get => _fileId; set => _fileId = value; }
		
		[ShtIgnore]
		public ItemId OwnerId => _owner != null ? _owner.Id : ItemId.None;
		
		[ShtIgnore]
		public IGameDatabase GameDatabase => _owner != null ? _owner.GameDatabase : null;

		[InfoBox("@ConfigDescription()", InfoMessageType.Info, "@ConfigDescription() != null", GUIAlwaysEnabled = true)]
		[GUIColor(0.8f, 0.8f, 0.4f)]
		[ShtSerialize, PropertyOrder(-2), HideLabel, ShtKey, ShtPriority(-200), ShtNoValidation, ShtProtected,
		 ShtBackground(0.8f), ShtTooltip(classType: typeof(TooltipFunctions), methodName: "SetDateTimeAndBranch"), HideInInlineEditors]
		public FullItemId FullId {
			get => OwnerId.ToFullItemId(Id);
#if UNITY_EDITOR
			// need for ShtSerialize?
			set => Debug.Assert(FullId == value);
#endif			
		}

		[ShtSerialize, ShtProtected, ShtPriority(-180), ShtBackground(0.8f)]
		public string FileName => $"{(_owner != null ? _owner.FileName : "[NONE]")}:{_assetName}"; 
		
		protected string ConfigDescription() => GetType().GetAttribute<ItemDescriptionAttribute>()?.Description ?? null;
		
		[ShtProtected()]
		public new string name {
			// берем из копии, тк обращении к имени в Unity возможно только в основном потоке 
			get => string.IsNullOrEmpty(_assetName) ? _assetName = base.name : _assetName;
			set {
				_assetName = value;
				base.name = value;
			}
		}
		public string AssetName => _assetName;
		protected virtual void Awake() => _assetName = base.name;

		public static implicit operator FileId(GameItemComponent comp) => comp == null ? FileId.None : comp._fileId;

		public int CompareTo(GameItemComponent other) {
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			var fileIdComparison = _fileId.CompareTo(other._fileId);
			return fileIdComparison != 0 ? fileIdComparison : Comparer<GameItemBase>.Default.Compare(_owner, other._owner);
		}

		public override string ToString() {
			return this._owner ? $"{_owner}/{name}#{Id.ToKeyString()}" : $"{name}#{Id.ToKeyString()}";
		}

		public virtual int OrderByValue => OwnerId.AsInt() * 1000 + Id.AsInt();

		public virtual void Init(IGameDatabase gameDatabase) {}
	}

}