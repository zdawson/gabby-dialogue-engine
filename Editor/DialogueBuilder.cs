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
            public Stack<DialogueBlockData> blocks = new Stack<DialogueBlockData>();

            public DialogueData(string characterName, string dialogueName)
            {
                this.characterName = characterName;
                this.dialogueName = dialogueName;
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

        public bool OnDialogueDefinition(string characterName, string dialogueName)
        {
            DialogueBlockData block = new DialogueBlockData();
            dialogueBlockStack.Push(block);

            DialogueData dialogueData = new DialogueData(characterName, dialogueName);
            curDialogue = dialogueData;
            
            return true;
        }

        public bool OnDialogueLine(string characterName, string text)
        {
            DialogueLine line = new DialogueLine(LineType.DIALOGUE, new string[]{characterName, text});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnContinuedDialogue(string characterName, string text)
        {
            DialogueLine line = new DialogueLine(LineType.CONTINUED_DIALOGUE, new string[]{text});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnEndDialogue()
        {
            // TODO check indentation
            DialogueLine line = new DialogueLine(LineType.END, new string[0]);
            dialogueBlockStack.Peek().lines.Add(line);
            curDialogue.blocks.Push(dialogueBlockStack.Pop());
            return true;
        }

        public void OnDialogueDefinitionEnd()
        {
            // Add an end line if it wasn't provided
            // if ()
            // {
            //     OnEndDialogue();
            // }

            List<DialogueBlock> dialogueBlocks = new List<DialogueBlock>();
            int id = 0;
            while (curDialogue.blocks.Count > 0)
            {
                DialogueBlockData blockData = curDialogue.blocks.Pop();
                dialogueBlocks.Add(new DialogueBlock(id++, blockData.lines.ToArray()));
            }
            
            Dialogue dialogue = new Dialogue(curDialogue.characterName, curDialogue.dialogueName, dialogueBlocks.ToArray());
            dialogues.Add(dialogue);
        }

        public List<Dialogue> Dialogues => dialogues;
    }
}
