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

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Spring.Collections;
using Spring.Objects.Factory.Support;

namespace Spring.Benchmark
{
    [ClrJob, CoreJob]
    [MemoryDiagnoser]
    public class HybridSetBenchmark
    {
        private HashedSet hashedSet;
        private HashSet<MethodOverrides> hashSet;
        private HybridSet hybridSet;
        private MethodOverrides[] items;
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            items = new MethodOverrides[100];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new MethodOverrides();
            }
        }
        
        [IterationSetup]
        public void Setup()
        {
            hashSet = new HashSet<MethodOverrides>();
            hybridSet = new HybridSet();
            hashedSet = new HashedSet();
        }

        [Params(1, 5, 10, 20)]
        public int Iterations { get; set; }
        
        [Benchmark]
        public void AddHashSet()
        {
            int iterations = Iterations;
            for (int i = 0; i < iterations; ++i)
            {
                hashSet.Add(items[i]);
            }
        }   
                
        [Benchmark]
        public void AddHashedSet()
        {
            int iterations = Iterations;
            for (int i = 0; i < iterations; ++i)
            {
                hashedSet.Add(items[i]);
            }
        }   
        
        [Benchmark]
        public void AddHybridSet()
        {
            int iterations = Iterations;
            for (int i = 0; i < iterations; ++i)
            {
                hybridSet.Add(items[i]);
            }
        }
    }
}