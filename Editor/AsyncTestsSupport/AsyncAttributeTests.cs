﻿using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    // class intended to test AsyncTest attribute itself
    class AsyncAttributeTests
    {
        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromCompletedPandaTask()
        {
            await PandaTasksUtilitys.CompletedTask;
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
            await PandaTasksUtilitys.CompletedTask;
        }

        [ AsyncTest ]
        public async IPandaTask< int > ReturnPandaTaskValueFromCompletedPandaTask()
        {
            return await PandaTasksUtilitys.GetCompletedTask(1);
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
            return await PandaTasksUtilitys.GetCompletedTask(1);
        }

        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromSystemTaskDelay()
        {
            await Task.Delay( 10 );
        }

        [ AsyncTest ]
        public async Task ReturnSystemTaskFromSystemTaskDelay()
        {
            await Task.Delay( 10 );
        }

        [ AsyncTest ]
        public async IPandaTask ReturnPandaTaskFromSystemTaskRun()
        {
            await Task.Run( () => Task.Delay( 10 ) );
        }

        [ AsyncTest ]
        public async Task ReturnSystemTaskFromSystemTaskRun()
        {
            await Task.Run( () => Task.Delay( 10 ) );
        }
    }
}