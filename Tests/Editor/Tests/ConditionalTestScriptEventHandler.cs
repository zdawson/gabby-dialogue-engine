using PotassiumK.GabbyDialogue;

public class ConditionalTestScriptEventHandler : AbstractScriptEventHandler
{
    [ConditionalHandler]
    private bool simpleConditional(bool result)
    {
        return result;
    }
}

public class ConditionalHandlerTest : AbstractScriptEventHandler
{
    [ConditionalHandler]
    private void returnTypeIsNotBool(bool result)
    {
    }
}
