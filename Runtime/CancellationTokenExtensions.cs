using System;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    public static class CancellationTokenExtensions
    {
        public static void RegisterIfCanBeCanceled(this CancellationToken token, Action callback)
        {
            if( token.CanBeCanceled )
            {
                token.Register( callback );
            }
        }
    }
}
