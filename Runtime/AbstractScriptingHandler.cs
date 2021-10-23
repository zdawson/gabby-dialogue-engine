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
    public struct ActionResult
    {
        public bool handled;
        public bool autoAdvance;
    }

    public abstract class AbstractScriptingHandler
    {
        protected Dictionary<string, Func<List<ActionParameter>, ActionResult>> actionHandlers = new Dictionary<string, Func<List<ActionParameter>, ActionResult>>();
        protected Dictionary<string, Func<List<ActionParameter>, bool>> conditionalHandlers = new Dictionary<string, Func<List<ActionParameter>, bool>>();

        public bool OnAction(string actionName, IEnumerable<string> parameters)
        {
            Func<List<ActionParameter>, ActionResult> action;
            if (!actionHandlers.TryGetValue(actionName, out action))
            {
                Debug.LogWarning($"Unhandled action: {actionName}");
                return true;
            }

            // TODO either simplify this, or add support for multiple action handlers for a given action name
            ActionResult result = action.Invoke(ParseParameters(parameters));
            if (!result.handled)
            {
                Debug.LogWarning($"Unhandled action: {actionName}");
                return true;
            }

            return result.autoAdvance;
        }

        public bool OnCondition(string callbackName, IEnumerable<string> parameters, out bool conditionResult)
        {
            Func<List<ActionParameter>, bool> callback;
            if (!conditionalHandlers.TryGetValue(callbackName, out callback))
            {
                conditionResult = false;
                return false;
            }

            conditionResult = callback.Invoke(ParseParameters(parameters));
            return true;
        }

        protected List<ActionParameter> ParseParameters(IEnumerable<string> stringParameters)
        {
            List<ActionParameter> actionParameters = new List<ActionParameter>();
            foreach (string str in stringParameters)
            {
                int intResult;
                float floatResult;
                bool boolResult;
                // TODO support parsing key:value pairs
                if (Int32.TryParse(str, out intResult))
                {
                    actionParameters.Add(new ActionParameter()
                    {
                        type = ParameterType.Number,
                        value = intResult
                    });
                }
                else if (Single.TryParse(str, out floatResult))
                {
                    actionParameters.Add(new ActionParameter()
                    {
                        type = ParameterType.Number,
                        value = floatResult
                    });
                }
                else if (Boolean.TryParse(str, out boolResult))
                {
                    actionParameters.Add(new ActionParameter()
                    {
                        type = ParameterType.Boolean,
                        value = boolResult
                    });
                }
                else
                {
                    actionParameters.Add(new ActionParameter()
                    {
                        type = ParameterType.String,
                        value = str
                    });
                }
            }
            return actionParameters;
        }

        protected void AddActionHandler(string actionName, Func<List<ActionParameter>, ActionResult> action)
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

        protected void AddConditionalHandler(string callbackName, Func<List<ActionParameter>, bool> callback)
        {
            if (conditionalHandlers.ContainsKey(callbackName))
            {
                Debug.LogWarning($"Attempting to add duplicate action handler for action: {callbackName}");
                return;
            }
            conditionalHandlers[callbackName] = callback;
        }

        protected void RemoveConditionalHandler(string callbackName)
        {
            conditionalHandlers.Remove(callbackName);
        }
    }
}
