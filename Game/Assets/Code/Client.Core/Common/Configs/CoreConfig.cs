using System;
using UnityEngine;

namespace Client.Core.Common.Configs {

	/// <summary>
	///     global system-wide client-specific constants. loads first at program startup
	/// </summary>
	[Serializable]
	public class CoreConfig {

		[Header("Scenes"), SerializeField]
		private string _mainSceneName = "UiScene";

		[Header("Urls")]
		[SerializeField] private string _licenseUrl = "https://../terms-of-service-en";
		[SerializeField] private string _privacyUrl = "https://../privacy-policy-en";
		[SerializeField] private string _supportEMail = "mail@mail.com";

		public string MainSceneName => _mainSceneName;

		public string LicenseUrl => _licenseUrl;

		public string PrivacyUrl => _privacyUrl;

		public string SupportEMail => _supportEMail;

	}

}