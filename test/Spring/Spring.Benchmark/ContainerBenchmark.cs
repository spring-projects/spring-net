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

using BenchmarkDotNet.Attributes;
using Spring.Benchmark.Classes;
using Spring.Context.Support;

namespace Spring.Benchmark
{
    [ClrJob, CoreJob]
    [MemoryDiagnoser]
    public class ContainerBenchmark
    {
        private XmlApplicationContext container;

        [Params(5_000)]
        public int Iterations { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            container = new XmlApplicationContext("ContainerBenchmark.xml");
            container.Refresh();
        }

        [Benchmark]
        public bool ResolveTransient()
        {
            bool ok = true;
            for (int i = 0; i < Iterations; i++)
            {
                var transient1 = (ITransient1) container.GetObject(typeof(ITransient1).FullName);
                var transient2 = (ITransient2) container.GetObject(typeof(ITransient2).FullName);
                var transient3 = (ITransient3) container.GetObject(typeof(ITransient3).FullName);
                ok &= transient1 != null && transient2 != null && transient3 != null;
            }

            return ok;
        }
        
        [Benchmark]
        public bool ResolveSingleton()
        {
            bool ok = true;
            for (int i = 0; i < Iterations; i++)
            {
                var transient1 = (ISingleton1) container.GetObject(typeof(ISingleton1).FullName);
                var transient2 = (ISingleton2) container.GetObject(typeof(ISingleton2).FullName);
                var transient3 = (ISingleton3) container.GetObject(typeof(ISingleton3).FullName);
                ok &= transient1 != null && transient2 != null && transient3 != null;
            }

            return ok;
        }
    }
}
