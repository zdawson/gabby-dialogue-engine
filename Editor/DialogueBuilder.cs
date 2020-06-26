using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GabbyDialogue
{
    public class DialogueBuilder : IDialogueBuilder
    {
        private class DialogueData
        {
            public string characterName;
            public string dialogueName;
            DialogueBlockData mainBlock;

            public DialogueData(string characterName, string dialogueName, DialogueBlockData mainBlock)
            {
                this.characterName = characterName;
                this.dialogueName = dialogueName;
                this.mainBlock = mainBlock;
            }
        }

        private class OptionsData
        {
            public List<(string, DialogueBlockData)> options = new List<(string, DialogueBlockData)>();
        }

        private class DialogueBlockData
        {
            public List<DialogueLine> lines = new List<DialogueLine>();
        }

        private List<Dialogue> dialogues = new List<Dialogue>();
        private DialogueData curDialogue;
        private Stack<DialogueBlockData> dialogueBlockStack = new Stack<DialogueBlockData>();

        private Dialogue FinalizeDialogue(DialogueData dialogueData, DialogueBlock mainBlock)
        {
            return new Dialogue(dialogueData.characterName, dialogueData.dialogueName, mainBlock);
        }

        private DialogueBlock FinalizeDialogueBlock(DialogueBlockData blockData)
        {
            return new DialogueBlock(blockData.lines.ToArray());
        }

        public bool OnDialogueDefinition(string characterName, string dialogueName)
        {
            Debug.Log("OnDialogueDefinition");
            DialogueBlockData block = new DialogueBlockData();
            dialogueBlockStack.Push(block);

            DialogueData dialogueData = new DialogueData(characterName, dialogueName, block);
            curDialogue = dialogueData;
            
            return true;
        }

        public bool OnDialogueLine(string characterName, string text)
        {
            Debug.Log("OnDialogueLine");
            DialogueLine line = new DialogueLine(characterName, text);
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnContinuedDialogue(string characterName, string text)
        {
            Debug.Log("OnContinuedDialogue");
            DialogueLine line = new DialogueLine(characterName, text);
            line.isContinuation = true;
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnEndDialogue()
        {
            Debug.Log("OnEndDialogue");
            if (dialogueBlockStack.Count == 1)
            {
                // Don't pop the main dialogue block, handle it in OnDialogueDefinitionEnd
                return true;
            }

            DialogueBlockData blockData = dialogueBlockStack.Pop();
            DialogueBlock block = FinalizeDialogueBlock(blockData);
            return true;
        }

        public void OnDialogueDefinitionEnd()
        {
            Debug.Log("OnDialogueDefinitionEnd");
            DialogueBlockData blockData = dialogueBlockStack.Pop();
            DialogueBlock mainBlock = FinalizeDialogueBlock(blockData);
            Dialogue dialogue = FinalizeDialogue(curDialogue, mainBlock);
            dialogues.Add(dialogue);
        }

        public List<Dialogue> Dialogues => dialogues;
    }
}
