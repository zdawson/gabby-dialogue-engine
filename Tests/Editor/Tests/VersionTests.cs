using PotassiumK.GabbyDialogue;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

public class VersionTests
{
    [Test]
    public void TestVersionIsCorrect()
    {
        DialogueScript testScript = Resources.Load<DialogueScript>("VersionTests/VersionTestCorrect");
        Assert.NotNull(testScript);
    }

    [Test]
    public void TestVersionIsTooOld()
    {
        DialogueBuilder builder = new DialogueBuilder();
        bool success = DialogParser.ParseGabbyDialogueScript("Packages/gabby-dialogue-engine/Tests/Resources/VersionTests/VersionTestTooOld.gab", builder);

        Assert.IsTrue(success);

        DialogueScript testScript = Resources.Load<DialogueScript>("VersionTests/VersionTestTooOld");
        Assert.NotNull(testScript);
        LogAssert.Expect(LogType.Warning, new Regex(@"[.]*older[.]*"));
    }

    [Test]
    public void TestVersionIsTooNew()
    {
        DialogueBuilder builder = new DialogueBuilder();
        bool success = DialogParser.ParseGabbyDialogueScript("Packages/gabby-dialogue-engine/Tests/Resources/VersionTests/VersionTestTooNew.gab", builder);

        Assert.IsTrue(success);

        DialogueScript testScript = Resources.Load<DialogueScript>("VersionTests/VersionTestTooNew");
        Assert.NotNull(testScript);
        LogAssert.Expect(LogType.Warning, new Regex(@"[.]*newer[.]*"));
    }
}
