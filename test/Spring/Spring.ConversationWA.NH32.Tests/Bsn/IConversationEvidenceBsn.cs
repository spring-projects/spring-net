using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Bsn
{
    /// <summary>
    /// Simulates a business infrastructure in order to demonstrate the end of 
    /// the conversation.
    /// </summary>
    public interface IConversationEvidenceBsn
    {
        /// <summary>
        /// Return a unique id per instance.
        /// </summary>
        /// <returns></returns>
        String UniqueId();
    }
}
