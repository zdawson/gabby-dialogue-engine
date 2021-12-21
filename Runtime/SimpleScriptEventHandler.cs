using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace PotassiumK.GabbyDialogue
{
    public class SimpleScriptEventHandler
    {
        private struct ActionHandlerBinding
        {
            public AbstractScriptEventHandler scriptEventHandler;
            public MethodInfo methodInfo;
            public bool autoAdvance;
        }

        private struct ConditionalHandlerBinding
        {
            public AbstractScriptEventHandler scriptEventHandler;
            public MethodInfo methodInfo;
        }

        private List<AbstractScriptEventHandler> _scriptEventHandlers = new List<AbstractScriptEventHandler>();

        private Dictionary<string, ActionHandlerBinding> _actionHandlers = new Dictionary<string, ActionHandlerBinding>();
        private Dictionary<string, ConditionalHandlerBinding> _conditionalHandlers = new Dictionary<string, ConditionalHandlerBinding>();

        public void RegisterScriptEventHandler(AbstractScriptEventHandler scriptEventHandler)
        {
            if (_scriptEventHandlers.Contains(scriptEventHandler))
            {
                Debug.LogWarning($"Script event handler already registered\n{scriptEventHandler.ToString()}");
                return;
            }
            _scriptEventHandlers.Add(scriptEventHandler);

            MethodInfo[] methods = scriptEventHandler.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                ActionHandler actionHandlerAttribute = method.GetCustomAttribute<ActionHandler>();
                if (actionHandlerAttribute != null)
                {
                    string actionName = actionHandlerAttribute.actionName != null ? actionHandlerAttribute.actionName : method.Name;
                    bool autoAdvance = actionHandlerAttribute.autoAdvance;
                    if (method.ReturnType != typeof(void))
                    {
                        Debug.LogError($"Return type of action handlers must be void.\nAction handler: {actionName}");
                    }
                    _actionHandlers[actionName] = new ActionHandlerBinding()
                    {
                        scriptEventHandler = scriptEventHandler,
                        methodInfo = method,
                        autoAdvance = autoAdvance
                    };
                    continue;
                }

                ConditionalHandler conditionalHandlerAttribute = method.GetCustomAttribute<ConditionalHandler>();
                if (conditionalHandlerAttribute != null)
                {
                    string conditionalName = conditionalHandlerAttribute.conditionalName != null ? conditionalHandlerAttribute.conditionalName : method.Name;
                    if (method.ReturnType != typeof(bool))
                    {
                        Debug.LogError($"Return type of conditional handlers must be bool.\nConditional handler: {conditionalName}");
                    }
                    _conditionalHandlers[conditionalName] = new ConditionalHandlerBinding()
                    {
                        scriptEventHandler = scriptEventHandler,
                        methodInfo = method
                    };
                    continue;
                }
            }
        }

        public bool OnAction(string actionName, List<string> parameters)
        {
            ActionHandlerBinding actionHandler;
            if (!_actionHandlers.TryGetValue(actionName, out actionHandler))
            {
                Debug.LogWarning($"Unhandled action: {actionName}");
                return true;
            }

            object[] parsedParameters;
            if (!BindParameters(parameters, actionHandler.methodInfo, out parsedParameters))
            {
                string parameterErrorLog = GetParameterErrorLogString(parameters, actionHandler.methodInfo);
                Debug.LogError($"Mismatched parameters when attempting to run action `{actionName}`\n{parameterErrorLog}");
                return true;
            }

            actionHandler.methodInfo.Invoke(actionHandler.scriptEventHandler, parsedParameters);
            return actionHandler.autoAdvance;
        }

        public bool OnConditional(string conditionalName, List<string> parameters)
        {
            ConditionalHandlerBinding conditionalHandler;
            if (!_conditionalHandlers.TryGetValue(conditionalName, out conditionalHandler))
            {
                Debug.LogWarning($"Unhandled conditional: {conditionalName}");
                return false;
            }

            object[] parsedParameters;
            if (!BindParameters(parameters, conditionalHandler.methodInfo, out parsedParameters))
            {
                string parameterErrorLog = "Expected types (), got values ()";
                Debug.LogError($"Mismatched parameters when attempting to run conditional `{conditionalName}`\n{parameterErrorLog}");
                return false;
            }

            return (bool) conditionalHandler.methodInfo.Invoke(conditionalHandler.scriptEventHandler, parsedParameters);
        }

        private bool BindParameters(List<string> parameterData, MethodInfo methodInfo, out object[] result)
        {
            ParameterInfo[] parameterInfo = methodInfo.GetParameters();

            if (parameterData.Count > parameterInfo.Length)
            {
                Debug.LogError($"Too many parameters passed to script event handler\nExpected {parameterInfo.Length}, got {parameterData.Count}");
                result = null;
                return false;
            }

            object[] parsedParameters = new object[parameterInfo.Length];
            for (int i = 0; i < parameterInfo.Length; ++i)
            {
                ParameterInfo parameter = parameterInfo[i];
                if (i < parameterData.Count)
                {
                    string strValue = parameterData[i];
                    Type targetType = parameter.ParameterType;
                    object parsedResult;
                    if (!TryParseParameter(targetType, strValue, out parsedResult))
                    {
                        Debug.LogError($"Could not parse parameter `{parameter.Name}` for script event handler `{methodInfo.Name}`\nExpected type: {targetType}, actual value: {strValue}");
                        result = null;
                        return false;
                    }
                    parsedParameters[i] = parsedResult;
                }
                else
                {
                    // Check if there is a default
                    if (parameter.HasDefaultValue)
                    {
                        parsedParameters[i] = parameter.DefaultValue;
                    }
                    else
                    {
                        Debug.LogError($"Missing parameter `{parameter.Name}` for script event handler `{methodInfo.Name}`");
                        result = null;
                        return false;
                    }
                }
            }
            result = parsedParameters;
            return true;
        }

        private bool TryParseParameter(Type targetType, string strValue, out object result)
        {
            if (targetType == typeof(string))
            {
                result = strValue;
            }
            else if (targetType == typeof(int))
            {
                int parseResult;
                if (!Int32.TryParse(strValue, out parseResult))
                {
                    result = null;
                    return false;
                }
                result = parseResult;
            }
            else if (targetType == typeof(float))
            {
                float parseResult;
                if (!Single.TryParse(strValue, out parseResult))
                {
                    result = null;
                    return false;
                }
                result = parseResult;
            }
            else if (targetType == typeof(double))
            {
                double parseResult;
                if (!Double.TryParse(strValue, out parseResult))
                {
                    result = null;
                    return false;
                }
                result = parseResult;
            }
            else if (targetType == typeof(bool))
            {
                bool parseResult;
                if (!Boolean.TryParse(strValue, out parseResult))
                {
                    result = null;
                    return false;
                }
                result = parseResult;
            }
            else if (targetType == typeof(char))
            {
                char parseResult;
                if (!Char.TryParse(strValue, out parseResult))
                {
                    result = null;
                    return false;
                }
                result = parseResult;
            }
            else if (targetType == typeof(uint))
            {
                uint parseResult;
                if (!UInt32.TryParse(strValue, out parseResult))
                {
                    result = null;
                    return false;
                }
                result = parseResult;
            }
            else
            {
                Debug.LogError($"Cannot parse into expected type: {targetType}");
                result = null;
                return false;
            }

            return true;
        }

        private string GetParameterErrorLogString(List<string> parameters, MethodInfo methodInfo)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("Expected parameters (");

            ParameterInfo[] parameterInfo = methodInfo.GetParameters();
            for (int i = 0; i < parameterInfo.Length; ++i)
            {
                ParameterInfo parameter = parameterInfo[i];
                sb.Append($"{parameter.ParameterType} {parameter.Name}");
                if (i < parameterInfo.Length - 1)
                {
                    sb.Append($",");
                }
            }

            sb.Append("), got values (");

            for (int i = 0; i < parameters.Count; ++i)
            {
                sb.Append($"{parameters[i]}");
                if (i < parameters.Count - 1)
                {
                    sb.Append($",");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }
    }
}
