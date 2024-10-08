using System;

namespace XLib.Unity.Installers.Attributes {

	[AttributeUsage(AttributeTargets.Class)]
	public class InstallerContainerAttribute : Attribute {
		
		public InstallerContainer Container { get; set; }

		public InstallerContainerAttribute(InstallerContainer container) {
			Container = container;
		}
		
	}

}