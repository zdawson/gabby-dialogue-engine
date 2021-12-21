using PotassiumK.GabbyDialogue;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

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
    public void TestSimpleDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "SimpleDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestMultipleLineDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

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
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "ImplicitCharacter");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestMultipleCharacters()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

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
        UnitTestDialogueSystem dialogueSystem = SetupTest();

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
        UnitTestDialogueSystem dialogueSystem = SetupTest();

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
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "SimpleDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();

        Assert.DoesNotThrow(() => {
            dialogueSystem.DialogueEngine.NextLine();
        });
    }

    [Test]
    public void LinesAfterEnd()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "LinesAfterEnd");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();

        Assert.DoesNotThrow(() => {
            dialogueSystem.DialogueEngine.NextLine();
        });

        Assert.IsTrue(dialogueSystem.DialogueEventQueue.Count == 0);
    }

    [Test]
    public void TestContinuedDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "SimpleContinuedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectContinuedLine("Continued line");
        dialogueSystem.ExpectContinuedLine("Continued line 2");
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

    [Test]
    public void TestContinuedNarration()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "ContinuedNarration");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("", "Narration");
        dialogueSystem.ExpectContinuedLine("Continued Narration");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestSimpleNarratedDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "SimpleNarratedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectLine("", "Line 3");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestDuplicateDialogueNameInSameFile()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        DialogueScript testScript = Resources.Load<DialogueScript>("DuplicateDialogueTests");
        Assert.NotNull(testScript);
        dialogueSystem.AddScript(testScript);

        LogAssert.Expect(LogType.Warning, new Regex(@"[.]*[dD]uplicate[.]*"));

        dialogueSystem.PlayDialogue("Test", "DuplicateDialogueNameInSameFile");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "First");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestDuplicateDialogueNameInAnotherFile()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        DialogueScript testScript = Resources.Load<DialogueScript>("DuplicateDialogueInAnotherFileTest");
        Assert.NotNull(testScript);
        dialogueSystem.AddScript(testScript);

        LogAssert.Expect(LogType.Warning, new Regex(@"[.]*[dD]uplicate[.]*"));

        dialogueSystem.PlayDialogue("Test", "DuplicateDialogueNameInAnotherFile");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "First");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestDialogueDoesNotExist()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "DialogueDoesNotExist");
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*not loaded[.]*"));
    }

    [Test]
    public void TestRemoveDialogueScript()
    {
        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();

        DialogueScript testScript = Resources.Load<DialogueScript>("DialogueTests");
        Assert.NotNull(testScript);
        dialogueSystem.AddScript(testScript);

        dialogueSystem.RemoveScript(testScript);

        DialogueScript testScript2 = Resources.Load<DialogueScript>("DuplicateDialogueInAnotherFileTest");
        Assert.NotNull(testScript2);
        dialogueSystem.AddScript(testScript2);

        dialogueSystem.PlayDialogue("Test", "DuplicateDialogueNameInAnotherFile");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Second");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestRemoveDialogueScriptDuringDialogue()
    {
        DialogueScript testScript = Resources.Load<DialogueScript>("DialogueTests");
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        dialogueSystem.PlayDialogue("Test", "SimpleDialogue");

        dialogueSystem.ExpectDialogueStart();

        dialogueSystem.RemoveScript(testScript);

        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();

        dialogueSystem.PlayDialogue("Test", "SimpleDialogue");
        LogAssert.Expect(LogType.Error, new Regex(@"[.]*not loaded[.]*"));
    }

    [Test]
    public void TestEndDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "MultipleLineDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.EndDialogue();
        dialogueSystem.ExpectDialogueEnd();

        Assert.DoesNotThrow(() => {
            dialogueSystem.DialogueEngine.NextLine();
        });
    }

    private UnitTestDialogueSystem SetupTest(string scriptName = "DialogueTests")
    {
        DialogueScript testScript = Resources.Load<DialogueScript>(scriptName);
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
