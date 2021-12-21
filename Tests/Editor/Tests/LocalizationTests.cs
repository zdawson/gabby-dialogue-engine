using PotassiumK.GabbyDialogue;
using NUnit.Framework;
using UnityEngine;

public class LocalizationTests
{
    [Test]
    public void TestDialogueScriptLanguageSetCorrectly()
    {
        DialogueScript dialogueScriptEn = Resources.Load<DialogueScript>("LocalizationTests_en");
        Assert.NotNull(dialogueScriptEn);
        Assert.AreEqual("english", dialogueScriptEn.language);

        DialogueScript dialogueScriptLoc = Resources.Load<DialogueScript>("LocalizationTests_loc");
        Assert.NotNull(dialogueScriptLoc);
        Assert.AreEqual("localized", dialogueScriptLoc.language);
    }

    [Test]
    public void TestDialogueUsesCorrectLanguage()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.SetLanguage("english");
        dialogueSystem.PlayDialogue("Test", "LocalizedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectDialogueEnd();

        dialogueSystem.SetLanguage("localized");
        dialogueSystem.PlayDialogue("Test", "LocalizedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Localized line 1");
        dialogueSystem.ExpectLine("Test", "Localized line 2");
        dialogueSystem.ExpectLine("Test", "Localized line 3");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestJumpWithLocalization()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.SetLanguage("english");
        dialogueSystem.PlayDialogue("Test", "LocalizedJump");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectJump("Test.LocalizedJumpTarget");
        dialogueSystem.ExpectLine("Test", "Jump target");
        dialogueSystem.ExpectDialogueEnd();

        dialogueSystem.SetLanguage("localized");
        dialogueSystem.PlayDialogue("Test", "LocalizedJump");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectJump("Test.LocalizedJumpTarget");
        dialogueSystem.ExpectLine("Test", "Localized jump target");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestImplicitLanguage()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.PlayDialogue("Test", "LocalizedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestFallbackLanguage()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.SetLanguage("localized", "english");
        dialogueSystem.PlayDialogue("Test", "DialogueThatOnlyExistsInEnglish");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.ExpectDialogueEnd();
    }

    [Test]
    public void TestChangingLanguageDuringDialogue()
    {
        UnitTestDialogueSystem dialogueSystem = SetupTest();

        dialogueSystem.SetLanguage("english");
        dialogueSystem.PlayDialogue("Test", "LocalizedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Line 1");
        dialogueSystem.SetLanguage("localized");
        dialogueSystem.ExpectLine("Test", "Line 2");
        dialogueSystem.ExpectDialogueEnd();

        dialogueSystem.PlayDialogue("Test", "LocalizedDialogue");

        dialogueSystem.ExpectDialogueStart();
        dialogueSystem.ExpectLine("Test", "Localized line 1");
        dialogueSystem.ExpectLine("Test", "Localized line 2");
        dialogueSystem.ExpectLine("Test", "Localized line 3");
        dialogueSystem.ExpectDialogueEnd();
    }

    private UnitTestDialogueSystem SetupTest()
    {
        DialogueScript dialogueScriptEn = Resources.Load<DialogueScript>("LocalizationTests_en");
        Assert.NotNull(dialogueScriptEn);

        DialogueScript dialogueScriptLoc = Resources.Load<DialogueScript>("LocalizationTests_loc");
        Assert.NotNull(dialogueScriptLoc);

        UnitTestDialogueSystem dialogueSystem = new UnitTestDialogueSystem();
        dialogueSystem.AddScript(dialogueScriptEn);
        dialogueSystem.AddScript(dialogueScriptLoc);

        return dialogueSystem;
    }
}
