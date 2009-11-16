using System;

namespace Spring.Util
{
    public interface IEventExceptionsCollector
    {
        bool HasExceptions { get; }
        Delegate[] Sources { get;}
        Exception[] Exceptions { get; }
        Exception this[Delegate source] { get; }
    }
}