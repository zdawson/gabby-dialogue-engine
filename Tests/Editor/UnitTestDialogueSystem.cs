using PotassiumK.GabbyDialogue;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UnitTestDialogueSystem : SimpleDialogueSystem
{
    public enum DialogueEventType
    {
        None,
        DialogueStart,
        DialogueEnd,
        Dialogue,
        ContinuedDialogue,
        Jump,
        Option,
        Action,
        Conditional
    }

    public class DialogueEvent
    {
        public DialogueEventType eventType;
        public Dictionary<string, string> tags;
    }

    public class DialogueLineEvent : DialogueEvent
    {
        public string characterName;
        public string dialogueText;
    }

    public class DialogueJumpEvent : DialogueEvent
    {
        public string jumpTarget;
    }

    public class DialogueActionEvent : DialogueEvent
    {
        public string actionName;
    }

    private Queue<DialogueEvent> dialogueEventQueue = new Queue<DialogueEvent>();
    public Queue<DialogueEvent> DialogueEventQueue => dialogueEventQueue;

    public DialogueEngine DialogueEngine => dialogueEngine;

    public new DialogueEvent NextLine()
    {
        Assert.IsTrue(dialogueEventQueue.Count == 0, "NextLine() was called but the dialogue event queue is not empty, this means an event was not handled or not expected.");
        base.NextLine();
        Assert.IsTrue(dialogueEventQueue.Count > 0, "NextLine() was called but no dialogue event was created, this is either an error or the dialogue has ended.");
        return dialogueEventQueue.Dequeue();
    }

    public DialogueEvent NextEvent()
    {
        Assert.IsTrue(dialogueEventQueue.Count > 0, "NextEvent() was called but the queue is empty.");
        return dialogueEventQueue.Dequeue();
    }

    public DialogueEvent Next()
    {
        if (dialogueEventQueue.Count > 0)
        {
            return dialogueEventQueue.Dequeue();
        }
        return NextLine();
    }

    public void RegisterScriptEventHandler(AbstractScriptEventHandler handler)
    {
        this.scriptEventHandler.RegisterScriptEventHandler(handler);
    }

    public override void OnDialogueStart(Dialogue dialogue)
    {
        DialogueEvent dialogueEvent = new DialogueEvent()
        {
            eventType = DialogueEventType.DialogueStart,
            tags = dialogue.Tags
        };
        dialogueEventQueue.Enqueue(dialogueEvent);
    }

    public override void OnDialogueEnd()
    {
        DialogueLineEvent dialogueEvent = new DialogueLineEvent()
        {
            eventType = DialogueEventType.DialogueEnd
        };
        dialogueEventQueue.Enqueue(dialogueEvent);
    }

    public override void OnDialogueLine(string characterName, string dialogueText, Dictionary<string, string> tags)
    {
        DialogueLineEvent dialogueEvent = new DialogueLineEvent()
        {
            eventType = DialogueEventType.Dialogue,
            characterName = characterName,
            dialogueText = dialogueText,
            tags = tags
        };
        dialogueEventQueue.Enqueue(dialogueEvent);
    }

    public override void OnContinuedDialogue(string continuedDialogueText, Dictionary<string, string> tags)
    {
        DialogueLineEvent dialogueEvent = new DialogueLineEvent()
        {
            eventType = DialogueEventType.ContinuedDialogue,
            dialogueText = continuedDialogueText,
            tags = tags
        };
        dialogueEventQueue.Enqueue(dialogueEvent);
    }

    public override void OnDialogueJump(Dialogue dialogue)
    {
        DialogueJumpEvent dialogueEvent = new DialogueJumpEvent()
        {
            eventType = DialogueEventType.Jump,
            jumpTarget = $"{dialogue.CharacterName}.{dialogue.DialogueName}"
        };
        dialogueEventQueue.Enqueue(dialogueEvent);
    }

    public override bool OnAction(string actionName, List<string> parameters)
    {
        DialogueActionEvent dialogueActionEvent = new DialogueActionEvent()
        {
            eventType = DialogueEventType.Action,
            actionName = actionName
        };
        dialogueEventQueue.Enqueue(dialogueActionEvent);

        return base.OnAction(actionName, parameters);
    }

    public override Task<int> OnOptionLine(string[] optionsText)
    {
        throw new System.NotImplementedException();
    }

    public void ExpectDialogueStart(Dictionary<string, string> tags = null)
    {
        DialogueEvent dialogueEvent = NextEvent();
        Assert.AreEqual(DialogueEventType.DialogueStart, dialogueEvent.eventType, "Dialogue event type does not match.");
        CompareTags(tags, dialogueEvent.tags);
    }

    public void ExpectDialogueEnd()
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.DialogueEnd, dialogueEvent.eventType, "Dialogue event type does not match.");
    }

    public void ExpectLine(string character, string text, Dictionary<string, string> tags = null)
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.Dialogue, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueLineEvent dialogueLineEvent = dialogueEvent as DialogueLineEvent;
        Assert.AreEqual(character, dialogueLineEvent.characterName, "Character name does not match.");
        Assert.AreEqual(text, dialogueLineEvent.dialogueText, "Dialogue text does not match.");
        CompareTags(tags, dialogueEvent.tags);
    }

    public void ExpectContinuedLine(string continuedDialogueText, Dictionary<string, string> tags = null)
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.ContinuedDialogue, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueLineEvent dialogueLineEvent = dialogueEvent as DialogueLineEvent;
        Assert.AreEqual(continuedDialogueText, dialogueLineEvent.dialogueText, "Dialogue text does not match.");
        CompareTags(tags, dialogueEvent.tags);
    }

    public void ExpectJump(string jumpTarget)
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.Jump, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueJumpEvent dialogueJumpEvent = dialogueEvent as DialogueJumpEvent;
        Assert.AreEqual(jumpTarget, dialogueJumpEvent.jumpTarget, "Dialogue jump target does not match.");
    }

    public void ExpectAction(ActionTestScriptEventHandler handler, string actionName, string actionHandlerName = "")
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.Action, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueActionEvent dialogueActionEvent = dialogueEvent as DialogueActionEvent;
        Assert.AreEqual(actionName, dialogueActionEvent.actionName, "Action name does not match.");
        Assert.AreEqual(actionHandlerName == "" ? actionName : actionHandlerName, handler.actionCalled, "Action handler name does not match.");
    }

    private void CompareTags(Dictionary<string, string> expected, Dictionary<string, string> actual)
    {
        if (expected == null)
        {
            return;
        }

        Assert.AreEqual(expected.Count, actual.Count, $"Incorrect number of tags.");

        foreach (string key in expected.Keys)
        {
            Assert.IsTrue(actual.ContainsKey(key), $"Expected key missing: {key}");
            Assert.AreEqual(expected[key], actual[key], $"Tag values do not match for key {key} : {expected[key]}, {actual[key]}");
        }
    }
}
