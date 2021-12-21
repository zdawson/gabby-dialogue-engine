using PotassiumK.GabbyDialogue;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

public class ConditionalTests
{
    [Test]
    public void TestIfElseIfElse()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ConditionalTestScriptEventHandler scriptEventHandler = new ConditionalTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestIfElseIfElse");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectLine("Test", "Line 3");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestEndWithinConditional()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ConditionalTestScriptEventHandler scriptEventHandler = new ConditionalTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestEndWithinConditional");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestConditionalHandlers()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ConditionalHandlerTest scriptEventHandler = new ConditionalHandlerTest();

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*[Rr]eturn type[.]*"));

        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);
    }

    private UnitTestDialogueSystem SetupTest(string scriptName = "ConditionalTests")
    {
        DialogueScript testScript = Resources.Load<DialogueScript>(scriptName);
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
