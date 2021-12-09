using GabbyDialogue;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

public class JumpTests
{
    [Test]
    public void TestJumpInSameFile()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "TestJumpInSameFile");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectJump("Test.TestJumpInSameFileTarget");
        dialogueSystem.ExpectLine("Test", "Same File");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestJumpInAnotherFile()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        DialogueScript testScript = Resources.Load<DialogueScript>("JumpTestsSeparateFile");
        Assert.NotNull(testScript);
        dialogueSystem.AddScript(testScript);

        dialogueSystem.PlayDialogue("Test", "TestJumpInAnotherFile");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectJump("Test.TestJumpInAnotherFileTarget");
        dialogueSystem.ExpectLine("Test", "Another File");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestJumpToNonExistentDialogueFails()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();
        dialogueSystem.PlayDialogue("Test", "TestJumpToNonExistentDialogueFails");

        LogAssert.Expect(LogType.Error, new Regex(@"[.]*jump target does not exist[.]*"));

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectDialogueEnd();
    }

    private UnitTestDialogueSystem SetupTest(string scriptName = "JumpTests")
    {
        DialogueScript testScript = Resources.Load<DialogueScript>(scriptName);
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
