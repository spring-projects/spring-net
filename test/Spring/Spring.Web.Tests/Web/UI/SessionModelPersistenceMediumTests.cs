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

using System.Collections;
using System.Web.UI;
using NUnit.Framework;
using Spring.Collections;
using Spring.TestSupport;

namespace Spring.Web.UI;

/// <summary>
///
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class SessionModelPersistenceMediumTests
{
    private class TestSessionModelPersistenceMedium : SessionModelPersistenceMedium
    {
        private Hashtable _sessionItems = new CaseInsensitiveHashtable();

        public Hashtable SessionItems
        {
            get { return _sessionItems; }
        }

        protected override object GetItem(System.Web.UI.Control context, string key)
        {
            //return base.GetItem( context, key );
            return _sessionItems[key];
        }

        protected override void SetItem(System.Web.UI.Control context, string key, object item)
        {
            //base.SetItem( context, key, item );
            _sessionItems[key] = item;
        }

        protected override string GetKey(Control context)
        {
            //return base.GetKey( context );
            return context.ID;
        }
    }

    [Test]
    public void StoresAndRetrievesModelItem()
    {
        TestSessionModelPersistenceMedium pm = new TestSessionModelPersistenceMedium();
        Control tuc = new TestUserControl("TucID");
        pm.SaveToMedium(tuc, this);
        // ensure key was generated by GetKey() and Item was added to storage
        Assert.AreEqual(this, pm.SessionItems["TucID"]);

        // ensure key was generated by GetKey() and Item is retrieved from storage
        Assert.AreEqual(this, pm.LoadFromMedium(tuc));
    }
}
