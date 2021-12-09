using GabbyDialogue;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

public class TagTests
{
    [Test]
    public void TestDialogueTags()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "TestDialogueTags");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1", new Dictionary<string, string>(){{"tagKey", "tagValue"}, {"tagWithoutValue", ""}});
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestLineTags()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "TestLineTags");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1", new Dictionary<string, string>(){{"testTag", ""}});
        dialogueSystem.ExpectLine("Test", "Line 2", new Dictionary<string, string>(){{"secondTag", ""}, {"thirdTag", ""}});
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestInlineTags()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "TestInlineTags");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1", new Dictionary<string, string>(){{"testTag", ""}});
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestOverwritingTags()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "TestOverwritingTags");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestTagValues()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "TestTagValues");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1", new Dictionary<string, string>()
        {
            {"stringTag", "value"},
            {"stringTagWithSymbols", "a,b. <>"},
            {"boolTag", " false"},
            {"intTag", "0"},
            {"floatTag", "0.0"}
        });
        dialogueSystem.ExpectDialogueEnd();
    }

    private UnitTestDialogueSystem SetupTest(string scriptName = "TagTests")
    {
        DialogueScript testScript = Resources.Load<DialogueScript>(scriptName);
        Assert.NotNull(testScript);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
