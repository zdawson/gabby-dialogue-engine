using GabbyDialogue;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;

public class PerformanceTests
{
    [Test, Performance]
    public void SimplePerformanceTest()
    {
        PerformanceTestDialogueSystem dialogueSystem = SetupTest();

        Measure.Method(() => {
            dialogueSystem.PlayAllLines();
        }).SetUp(() => {
            dialogueSystem.PlayDialogue("Test", "SimplePerformanceTest");
        }).GC().Run();
    }

    [Test, Performance]
    public void JumpPerformanceTest()
    {
        PerformanceTestDialogueSystem dialogueSystem = SetupTest();

        Measure.Method(() => {
            dialogueSystem.PlayAllLines();
        }).SetUp(() => {
            dialogueSystem.PlayDialogue("Test", "JumpPerformanceTest");
        }).GC().Run();
    }

    [Test, Performance]
    public void ActionPerformanceTest()
    {
        PerformanceTestDialogueSystem dialogueSystem = SetupTest();

        Measure.Method(() => {
            dialogueSystem.PlayAllLines();
        }).SetUp(() => {
            dialogueSystem.PlayDialogue("Test", "ActionPerformanceTest");
        }).GC().Run();
    }

    private PerformanceTestDialogueSystem SetupTest()
    {
        DialogueScript testScript = Resources.Load<DialogueScript>("PerformanceTest");
        Assert.NotNull(testScript);

        PerformanceTestDialogueSystem dialogueSystem = new PerformanceTestDialogueSystem();
        dialogueSystem.AddScript(testScript);

        return dialogueSystem;
    }
}
