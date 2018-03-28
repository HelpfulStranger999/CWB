using System;

namespace CWBDrone.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SummaryAttribute : Attribute
    {
        public string Text { get; }
        public SummaryAttribute(string text) => Text = text;
    }
}
