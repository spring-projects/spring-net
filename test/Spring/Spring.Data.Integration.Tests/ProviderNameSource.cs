#region License

// /*
//  * Copyright 2018 the original author or authors.
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

#region using

using Spring.Objects.Factory.Config;

#endregion

namespace Spring
{
    public class ProviderNameSource : IVariableSource
    {
        public bool CanResolveVariable(string name)
        {
            return name.ToLower() == "providername";
        }

        public string ResolveVariable(string name)
        {
            if (name.ToLower() != "providername")
            {
                return null;
            }
#if NETCOREAPP
            return "SqlServer";
#else
            return "SqlServer-2.0";
#endif
        }
    }
}