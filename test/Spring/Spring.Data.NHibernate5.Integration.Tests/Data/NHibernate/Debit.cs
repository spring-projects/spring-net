/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Spring.Data.NHibernate
{
    public class Debit
    {
        private int debitId;
        private float amount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Credit"/> class.
        /// </summary>
        public Debit()
        {
        }

        public int DebitID
        {
            get { return debitId; }
            set { debitId = value; }
        }

        public float Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        

        

        
    }
}
