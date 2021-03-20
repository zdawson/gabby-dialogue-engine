using System;
using System.Collections.Generic;
using UnityEngine;

namespace GabbyDialogue
{
    public enum ParameterType
    {
        String,
        Number,
        Boolean,
        KeyValue
    }
    public class ActionParameter
    {
        public ParameterType type;
        public object value;
    }

    public abstract class AbstractScriptingHandler
    {
        protected Dictionary<string, Action<List<ActionParameter>>> actionHandlers = new Dictionary<string, Action<List<ActionParameter>>>();
        protected Dictionary<string, Func<List<ActionParameter>, bool>> conditionalHandlers = new Dictionary<string, Func<List<ActionParameter>, bool>>();

        public bool OnAction(string actionName, List<string> parameters)
        {
            Action<List<ActionParameter>> action;
            if (!actionHandlers.TryGetValue(actionName, out action))
            {
                return false;
            }

            action.Invoke(ParseParameters(parameters));
            return true;
        }

        protected List<ActionParameter> ParseParameters(List<string> stringParameters)
        {
            List<ActionParameter> actionParameters = new List<ActionParameter>();
            foreach (string str in stringParameters)
            {
                actionParameters.Add(new ActionParameter(){
                    type=ParameterType.String,
                    value=str});
            }
            return actionParameters;
        }

        protected void AddActionHandler(string actionName, Action<List<ActionParameter>> action)
        {
            if (actionHandlers.ContainsKey(actionName))
            {
                Debug.LogWarning($"Attempting to add duplicate action handler for action: {actionName}");
                return;
            }
            actionHandlers[actionName] = action;
        }

        protected void RemoveActionHandler(string actionName)
        {
            actionHandlers.Remove(actionName);
        }

    }
}
