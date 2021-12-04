using GabbyDialogue;
using NUnit.Framework;
using UnityEngine;

public class ContinuedDialogueTests
{
    [Test]
    public void TestContinuedDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "SimpleContinuedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectContinuedLine("Continued line");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestContinuedDialogueOnFirstLine()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "ContinuedDialogueOnFirstLine");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectContinuedLine("Line 1");
        dialogueSystem.ExpectDialogueEnd();
    }

    private UnitTestDialogueSystem SetupTest()
    {
        DialogueScript testScript = Resources.Load<DialogueScript>("ContinuedDialogueTests");
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
