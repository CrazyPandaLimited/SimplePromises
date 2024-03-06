using System.Threading.Tasks;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    // class intended to test AsyncTest attribute itself
    class AsyncAttributeTests
    {
        private static object[][] _testCaseSource = new object[][]
        {
            new object[] { 1, 2, 3 }
        };

        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromCompletedPandaTask()
        {
            await PandaTasksUtilities.CompletedTask;
        }

        [ AsyncTest ]
        public async Task ReturnSystemTaskFromCompletedSystemTask()
        {
            await Task.CompletedTask;
        }
        
        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromCompletedSystemTask()
        {
            await Task.CompletedTask;
        }

        [ AsyncTest ]
        public async Task ReturnSystemTaskFromCompletedPandaTask()
        {
            await PandaTasksUtilities.CompletedTask;
        }

        [ AsyncTest ]
        public async IPandaTask< int > ReturnPandaTaskValueFromCompletedPandaTask()
        {
            return await PandaTasksUtilities.GetCompletedTask(1);
        }

        [ AsyncTest ]
        public async Task< int > ReturnSystemTaskValueFromCompletedSystemTask()
        {
            return await Task.FromResult(1);
        }
        
        [ AsyncTest ]
        public async IPandaTask< int > ReturnPandaTaskValueFromCompletedSystemTask()
        {
            return await Task.FromResult(1);
        }

        [ AsyncTest ]
        public async Task< int > ReturnSystemTaskValueFromCompletedPandaTask()
        {
            return await PandaTasksUtilities.GetCompletedTask(1);
        }

        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromSystemTaskDelay()
        {
            DelayPandaTask.Reset();
            await PandaTasksUtilities.Delay( 10 );
        }

        [ AsyncTest ]
        public async Task ReturnSystemTaskFromSystemTaskDelay()
        {
            await PandaTasksUtilities.Delay( 10 );
        }

#if !UNITY_WEBGL
        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromSystemTaskRun()
        {
            await Task.Run( () => Task.Delay( 10 ) );
        }
#endif
        
#if !UNITY_WEBGL
        [ AsyncTest ]
        public async Task ReturnSystemTaskFromSystemTaskRun()
        {
            await Task.Run( () => Task.Delay( 10 ) );
        }
#endif
        [ AsyncTest ]
        [ AsyncTestCase(1, 2, 3) ]
        public Task AsyncTestCaseSimpleArgs(int a, int b, int c)
        {
            return Task.CompletedTask;
        }
        
        [ AsyncTest ]
        [ AsyncTestCase( 1, 2, 3 ) ]
        public Task AsyncTestCaseParamsArgs( params int[] allArgs )
        {
            return Task.CompletedTask;
        }
        
        [ AsyncTest ]
        [ AsyncTestCase( 1, 2, 3 ) ]
        public Task AsyncTestCaseMixedArgs( int a, params int[] otherArgs )
        {
            return Task.CompletedTask;
        }
        
        [ AsyncTest ]
        [ AsyncTestCaseSource( nameof(_testCaseSource) ) ]
        public Task AsyncTestCaseSourceSimpleArgs( int a, int b, int c )
        {
            return Task.CompletedTask;
        }
    }
}
