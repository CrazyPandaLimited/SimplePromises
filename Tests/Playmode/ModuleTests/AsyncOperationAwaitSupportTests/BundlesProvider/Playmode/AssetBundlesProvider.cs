using System.Collections.Generic;
using CrazyPanda.UnityCore.DeliverySystem;
using UnityEngine;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class AssetBundlesProvider : CrazyPanda.UnityCore.DeliverySystem.AssetBundlesProvider
    {
        public override IEnumerable< string > AssetBundles
        {
            get
            {
                yield return 
                    $"Assets/UnityCoreSystems/Promises/Tests/Playmode/ModuleTests/AsyncOperationAwaitSupportTests/Bundle/{this.GetPlatformPrefix()}/test.bundle";
            }
        }

#if !UNITY_EDITOR        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void CopyAssets()
        {
            var assetBundlesProvider = new AssetBundlesProvider();
            await assetBundlesProvider.CopyAssetBundlesToDataPath();
        }
#endif
        
    }
}