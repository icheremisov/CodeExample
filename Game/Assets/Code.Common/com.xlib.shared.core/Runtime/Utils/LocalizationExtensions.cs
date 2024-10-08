using System.Globalization;

namespace XLib.Core.Utils {

	public static class LocalizationExtensions {

		public static void ForceIncluded() {
			_ = new ChineseLunisolarCalendar();
			_ = new HebrewCalendar();
			_ = new HijriCalendar();
			_ = new JapaneseCalendar();
			_ = new JapaneseLunisolarCalendar();
			_ = new KoreanCalendar();
			_ = new KoreanLunisolarCalendar();
			_ = new PersianCalendar();
			_ = new TaiwanCalendar();
			_ = new TaiwanLunisolarCalendar();
			_ = new ThaiBuddhistCalendar();
			_ = new UmAlQuraCalendar();
		}

	}

}