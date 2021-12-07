using System;

namespace GabbyDialogue
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionHandler : Attribute
    {
        public string actionName;
        public bool autoAdvance;

        public ActionHandler(string actionName = null, bool autoAdvance = true)
        {
            this.actionName = actionName;
            this.autoAdvance = autoAdvance;
        }
    }
}
