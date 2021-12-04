using GabbyDialogue;
using NUnit.Framework;
using UnityEngine;

public class DialogueTests
{
    [Test]
    public void TestLoadingSingleDialogueScript()
    {
        DialogueScript testScript = Resources.Load<DialogueScript>("DialogueTests");
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        // Check that a few dialogues are loaded
        Assert.NotNull(dialogueSystem.GetDialogue("Test", "SimpleDialogue"));
        Assert.NotNull(dialogueSystem.GetDialogue("Test", "MultipleLineDialogue"));
    }

    [Test]
    public void TestLoadingMultipleDialogueScripts()
    {
    }

    [Test]
    public void TestSimpleDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "SimpleDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestMultipleLineDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "MultipleLineDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectLine("Test", "Line 3");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestImplicitCharacter()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "ImplicitCharacter");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestMultipleCharacters()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "MultipleCharacters");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test1", "Line 1");
        dialogueSystem.ExpectLine("Test2", "Line 2");
        dialogueSystem.ExpectLine("Test2", "Line 3");
        dialogueSystem.ExpectLine("Test1", "Line 4");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestIndentation()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "Indentation");
        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectLine("Test", "Line 3");
        dialogueSystem.ExpectLine("Test", "Line 4");
        dialogueSystem.ExpectLine("Test", "Line 5");
        dialogueSystem.ExpectLine("Test", "Line 6");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestSpecialCharactersInDialogueText()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "SpecialCharactersInDialogueText");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "ë è é ê");
        dialogueSystem.ExpectLine("Test", "ガッビ");
        dialogueSystem.ExpectLine("Test", "Æ æ a Ø ø");
        dialogueSystem.ExpectLine("Test", "ن ه و ي");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestNextLineAfterDialogueEndedIsHandledGracefully()
    {
        UnitTestDialogueSystem dialogueSystem = SetupDialogueTest("DialogueTests");

        dialogueSystem.PlayDialogue("Test", "Sim2pleDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();

        Assert.IsNull(dialogueSystem.NextLine());
    }

    private UnitTestDialogueSystem SetupDialogueTest(string scriptName)
    {
        DialogueScript testScript = Resources.Load<DialogueScript>("DialogueTests");
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
