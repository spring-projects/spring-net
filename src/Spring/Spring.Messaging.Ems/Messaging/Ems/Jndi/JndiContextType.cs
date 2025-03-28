namespace Spring.Messaging.Ems.Jndi;

/// <summary>
/// The various JNDI Context types.
/// </summary>
public enum JndiContextType
{
    /// <summary>
    /// Create a tibjmsnaming context to lookup administered object inside the tibjmsnaming server.
    /// </summary>
    JMS,

    /// <summary>
    /// Create a ldap context to lookup administered object in an ldap server.
    /// </summary>
    LDAP
};
