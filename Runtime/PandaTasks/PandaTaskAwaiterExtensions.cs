using System.Diagnostics;
using CrazyPanda.UnityCore.PandaTasks;

// place it in root namespace, so we don't need to add any usings to write 'await SomeTask'
[ DebuggerNonUserCode ]
public static class PandaTaskAwaiterExtenstions
{
    public static PandaTaskAwaiter GetAwaiter( this IPandaTask task )
    {
        return new PandaTaskAwaiter( task );
    }

    public static PandaTaskAwaiter< T > GetAwaiter< T >( this IPandaTask< T > task )
    {
        return new PandaTaskAwaiter< T >( task );
    }
}