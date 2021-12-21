using System;

namespace PotassiumK.GabbyDialogue
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ConditionalHandler : Attribute
    {
        public string conditionalName;

        public ConditionalHandler(string conditionalName = null)
        {
            this.conditionalName = conditionalName;
        }
    }
}
