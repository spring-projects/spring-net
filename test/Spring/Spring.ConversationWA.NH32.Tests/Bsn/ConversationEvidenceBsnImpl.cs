using System;
using System.Data;
using System.Configuration;

namespace Spring.Bsn
{
    /// <summary>
    /// <see cref="IConversationEvidenceBsn"/>
    /// </summary>
    public class ConversationEvidenceBsnImpl: IConversationEvidenceBsn
    {
        private String uniqueId = "";

        /// <summary>
        /// Create instance with unique id.
        /// </summary>
        public ConversationEvidenceBsnImpl()
        {
            uniqueId = Guid.NewGuid().ToString();
        }

        #region IConversationEvidenceBsn Members

        /// <summary>
        /// <see cref="IConversationEvidenceBsn"/>
        /// </summary>
        /// <returns></returns>
        public String UniqueId()
        {
            return this.uniqueId;
        }

        #endregion
    }
}
