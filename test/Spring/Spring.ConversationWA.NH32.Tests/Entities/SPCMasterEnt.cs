using System;
using System.Collections.Generic;
using System.Text;
using Spring.ConversationWA;

namespace Spring.Entities
{
    /// <summary>
    /// Master Entity for 'session-per-conversation' tests: 
    /// <see cref="WebConversationStateTest.SPCLazyLoadTest()"/>, 
    /// <see cref="WebConversationStateTest.SPCSwitchConversationSameRequestTest()"/>
    /// </summary>
    /// <author>Hailton de Castro</author>
    public class SPCMasterEnt
    {
        private Int32? id;
        /// <summary>
        /// Entity key
        /// </summary>
        public virtual Int32? Id
        {
            get { return id; }
            set { id = value; }
        }

        private String description;
        /// <summary>
        /// Description
        /// </summary>
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        private IList<SPCDetailEnt> sPCDetailEntList;
        /// <summary>
        /// <see cref="SPCDetailEnt"/> one-to-many relationship.
        /// </summary>
        public virtual IList<SPCDetailEnt> SPCDetailEntList
        {
            get { return sPCDetailEntList; }
            set { sPCDetailEntList = value; }
        }
    }
}
