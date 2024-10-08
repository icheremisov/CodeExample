using JetBrains.Annotations;
using UnityEditor.AddressableAssets.Settings;
using XLib.BuildSystem.Types;

namespace XLib.BuildSystem.Addressables
{
    [UsedImplicitly]
    public class BuildAddressablesProcessor : IBeforeBuildRunner
    {
        public int Priority => 999999;

        public void OnBeforeBuild(BuildRunnerOptions options, RunnerReport report)
        {
            report.Logger.Log($"{nameof(BuildAddressablesProcessor)} building...");
            // BundlesBuilder.BuildBundles(options, report);
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent(out var result);
        }
    }
}