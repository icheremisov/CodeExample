using System;
using System.Collections.Generic;
using System.Linq;

namespace XLib.Unity.Utils {

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class ValueDropdownOfAttribute : Attribute {
		public ValueDropdownOfAttribute(string name = null) : base() {
			Name = name ?? string.Empty;
		}

		public string Name { get; set; }
		public string[] SearchInFolders { get; set; }

		public string Paths {
			set => this.SearchInFolders = ((IEnumerable<string>)value.Split('|')).Select<string, string>((Func<string, string>)(x => x.Trim().Trim('/', '\\'))).ToArray<string>();
			get => this.SearchInFolders != null ? string.Join(",", this.SearchInFolders) : (string)null;
		}
	}

	public static class DropdownUtils {
		public static readonly string Auto = "@XLib.Unity.Tools.ValueDropdownOfType.All($property)";
	}

}