using XLib.Core.Utils.Attributes;

namespace XLib.Configs.Core {

	public enum LocalizationParam {
		[EnumFormat(null, null, bold: true)]
		Name,
		[EnumFormat(null, null, bold: true)]
		Description,
		[EnumFormat("#2B9822", null, true)]
		Power,
		[EnumFormat("#5956F0", null, true)]
		Turn,
		[EnumFormat("#2B9822", null, true)]
		CritChance,
		[EnumFormat("#2B9822", null, true)]
		CritChanceAdd,
		[EnumFormat("#2B9822", null, true)]
		DmgReduce,
		[EnumFormat("#2B9822", null, true)]
		Bonus,
		[EnumFormat("#2B9822", null, true)]
		BarrierCur,
		[EnumFormat("#2B9822", null, true)]
		BarrierMax,
		[EnumFormat("#2B9822", null, true)]
		Damage,
		[EnumFormat("#2B9822", null, true)]
		Heal,
		[EnumFormat("#2B9822", null, true)]
		Caster,
		[EnumFormat("#2B9822", null, true)]
		Cooldown,
		[EnumFormat("#2B9822", null, true)]
		BattleLimit,
		[EnumFormat(null, null, bold: true)]
		Title,
		[EnumFormat("#2B9822", null, true)]
		Additional,
		[EnumFormat("#2B9822", null, true)]
		Owner
	}

}