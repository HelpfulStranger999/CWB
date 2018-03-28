using System;

namespace CWBDrone.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ConfigIgnoreAttribute : Attribute { }
}
