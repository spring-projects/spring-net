#region License
// /*
//  * Copyright 2022 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
#endregion

using System.Runtime.CompilerServices;

namespace Spring.Messaging.Nms.Support
{
    public static class TaskExtensions
    {
        public static T GetAsyncResult<T>(this Task<T> task)
        {
            return task.ConfigureAwait(ContinueOnCapturedContext).GetAwaiter().GetResult();
        }
        
        public static void GetAsyncResult(this Task task)
        {
            task.ConfigureAwait(ContinueOnCapturedContext).GetAwaiter().GetResult();
        }
        
        public static ConfiguredTaskAwaitable<T> Awaiter<T>(this Task<T> task)
        {
            return task.ConfigureAwait(ContinueOnCapturedContext);
        }
        
        public static ConfiguredTaskAwaitable Awaiter(this Task task)
        {
            return task.ConfigureAwait(ContinueOnCapturedContext);
        }

        public static bool ContinueOnCapturedContext { get; set; } = false;
    }
}