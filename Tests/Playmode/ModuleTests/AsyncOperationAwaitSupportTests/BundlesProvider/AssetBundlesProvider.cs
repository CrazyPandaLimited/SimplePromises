using System.Collections.Generic;
using CrazyPanda.UnityCore.DeliverySystem;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class AssetBundlesProvider : IAssetBundlesProvider
    {
        public IEnumerable< string > AssetBundles
        {
            get
            {
                yield return 
                    $"Assets/UnityCoreSystems/Promises/Tests/Playmode/ModuleTests/AsyncOperationAwaitSupportTests/Bundle/{this.GetPlatformPrefix()}/test.bundle";
            }
        }
    }
}