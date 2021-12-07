using GabbyDialogue;
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
        Option
    }

    public class DialogueEvent
    {
        public DialogueEventType eventType;
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

    private Queue<DialogueEvent> dialogueEventQueue = new Queue<DialogueEvent>();
    public Queue<DialogueEvent> DialogueEventQueue => dialogueEventQueue;

    public override void PlayDialogue(string characterName, string dialogueName)
    {
        Assert.IsNotNull(GetDialogue(characterName, dialogueName), "Dialogue [{characterName}.{dialogueName}] not loaded. (language: '{language}', fallback: '{fallbackLanguage}')");
        base.PlayDialogue(characterName, dialogueName);
    }

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

    public override void OnDialogueStart(Dialogue dialogue)
    {
        DialogueEvent dialogueEvent = new DialogueEvent()
        {
            eventType = DialogueEventType.DialogueStart
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
            dialogueText = dialogueText
        };
        dialogueEventQueue.Enqueue(dialogueEvent);
    }

    public override void OnContinuedDialogue(string continuedDialogueText, Dictionary<string, string> tags)
    {
        DialogueLineEvent dialogueEvent = new DialogueLineEvent()
        {
            eventType = DialogueEventType.ContinuedDialogue,
            dialogueText = continuedDialogueText
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

    public override Task<int> OnOptionLine(string[] optionsText)
    {
        throw new System.NotImplementedException();
    }

    public void ExpectDialogueStart()
    {
        DialogueEvent dialogueEvent = NextEvent();
        Assert.AreEqual(DialogueEventType.DialogueStart, dialogueEvent.eventType, "Dialogue event type does not match.");
        // TODO check that it's the right dialogue
    }

    public void ExpectDialogueEnd()
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.DialogueEnd, dialogueEvent.eventType, "Dialogue event type does not match.");
    }

    public void ExpectLine(string character, string text)
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.Dialogue, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueLineEvent dialogueLineEvent = dialogueEvent as DialogueLineEvent;
        Assert.AreEqual(character, dialogueLineEvent.characterName, "Character name does not match.");
        Assert.AreEqual(text, dialogueLineEvent.dialogueText, "Dialogue text does not match.");
    }

    public void ExpectContinuedLine(string continuedDialogueText)
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.ContinuedDialogue, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueLineEvent dialogueLineEvent = dialogueEvent as DialogueLineEvent;
        Assert.AreEqual(continuedDialogueText, dialogueLineEvent.dialogueText, "Dialogue text does not match.");
    }

    public void ExpectJump(string jumpTarget)
    {
        DialogueEvent dialogueEvent = Next();
        Assert.AreEqual(DialogueEventType.Jump, dialogueEvent.eventType, "Dialogue event type does not match.");
        DialogueJumpEvent dialogueJumpEvent = dialogueEvent as DialogueJumpEvent;
        Assert.AreEqual(jumpTarget, dialogueJumpEvent.jumpTarget, "Dialogue jump target does not match.");
    }
}
