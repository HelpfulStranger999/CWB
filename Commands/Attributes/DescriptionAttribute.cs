using System;

namespace CWBDrone.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class |
        AttributeTargets.Module | AttributeTargets.Method | AttributeTargets.Parameter,
        AllowMultiple = false, Inherited = false)]
    public class DescriptionAttribute : Attribute
    {
        public string Text { get; set; }
        public DescriptionAttribute(string text) => Text = text;
    }
}
