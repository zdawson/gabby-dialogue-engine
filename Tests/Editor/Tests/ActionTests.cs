using PotassiumK.GabbyDialogue;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

public class ActionTests
{
    [Test]
    public void TestSimple()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestSimple");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestSimple");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestActionWithDifferentNameThanHandler()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestActionWithDifferentNameThanHandler");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestActionWithDifferentNameThanHandler", "DifferentName");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestString()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestString");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestString");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestBool()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestBool");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestBool");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestInt()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestInt");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestInt");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestUint()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestUint");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestUint");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestFloat()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestFloat");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestFloat");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestDouble()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestDouble");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestDouble");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestMultipleParameters()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestMultipleParameters");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestMultipleParameters");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestDefaultParameters()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestDefaultParameters");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestDefaultParameters");
        dialogueSystem.ExpectAction(scriptEventHandler, "TestDefaultParameters");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestMissingParametersFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestMissingParametersFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Missing parameter[.]*"));
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Mismatched parameters[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestMultipleParameters", null);
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestTooManyParametersFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestTooManyParametersFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Too many parameters[.]*"));
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Mismatched parameters[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestMultipleParameters", null);
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestPassingWrongTypeFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestPassingWrongTypeFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Could not parse[.]*"));
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Mismatched parameters[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestInt", null);
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestNegativeUintFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestNegativeUintFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Could not parse[.]*"));
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Mismatched parameters[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestUint", null);
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestPassingFloatToIntFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestPassingFloatToIntFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Could not parse[.]*"));
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Mismatched parameters[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestInt", null);
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestPassingIntToBoolFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionTestScriptEventHandler scriptEventHandler = new ActionTestScriptEventHandler();
        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);

        dialogueSystem.PlayDialogue("Test", "TestPassingIntToBoolFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Could not parse[.]*"));
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*Mismatched parameters[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectAction(scriptEventHandler, "TestBool", null);
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestActionHandlers()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        ActionHandlerTest scriptEventHandler = new ActionHandlerTest();

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*[Rr]eturn type[.]*"));

        dialogueSystem.RegisterScriptEventHandler(scriptEventHandler);
    }

    private UnitTestDialogueSystem SetupTest(string scriptName = "ActionTests")
    {
        DialogueScript testScript = Resources.Load<DialogueScript>(scriptName);
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
