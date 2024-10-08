//////////////////////////////////////////////////
///// GENERATED FILE
//////////////////////////////////////////////////
// use 'Build/Tools/Generate Game Features' for rebuild this file


namespace XLib.BuildSystem {
	public static class GameFeature {
		
		/// <summary>
		/// Development build 
		/// </summary>
		public const bool DevelopmentBuild 
#if DEVELOPMENT_BUILD
		 = true;
#else		
		 = false;
#endif

		
		/// <summary>
		/// Enable config at start 
		/// </summary>
		public const bool StartupConfig 
#if FEATURE_STARTUPCONFIG
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Enable console with logs 
		/// </summary>
		public const bool Console 
#if FEATURE_CONSOLE
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Enable cheats 
		/// </summary>
		public const bool Cheats 
#if FEATURE_CHEATS
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Enable realtime PVP 
		/// </summary>
		public const bool RealtimePVP 
#if FEATURE_REALTIMEPVP
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Art and testing section 
		/// </summary>
		public const bool ArtAndTesting 
#if FEATURE_ARTANDTESTING
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Use Staging configs and URLs 
		/// </summary>
		public const bool Staging 
#if FEATURE_STAGING
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Use Production analytics 
		/// </summary>
		public const bool UseProdAnalytics 
#if FEATURE_USEPRODANALYTICS
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Use Production configs and URLs 
		/// </summary>
		public const bool Production 
#if FEATURE_PRODUCTION
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Special build for 'Friends-and-family' 
		/// </summary>
		public const bool Demo 
#if FEATURE_DEMO
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Enable battle logs 
		/// </summary>
		public const bool BattleLogs 
#if FEATURE_BATTLELOGS
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Disable remote server (Built-in only) 
		/// </summary>
		public const bool DisableServer 
#if FEATURE_DISABLESERVER
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Disable all login functionality 
		/// </summary>
		public const bool DisableLogin 
#if FEATURE_DISABLELOGIN
		 = true;
#else		
		 = false;
#endif


		/// <summary>
		/// Enable In-App Purchases 
		/// </summary>
		public const bool InAppPurchases 
#if FEATURE_INAPPPURCHASES
		 = true;
#else		
		 = false;
#endif


	}
}