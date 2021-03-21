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
            public List<DialogueBlockData> blocks = new List<DialogueBlockData>();

            public DialogueData(string characterName, string dialogueName)
            {
                this.characterName = characterName;
                this.dialogueName = dialogueName;
            }
        }

        private class OptionsBlockData
        {
            public List<(string, DialogueBlockData)> options = new List<(string, DialogueBlockData)>();
        }

        private class DialogueBlockData
        {
            public int blockID;
            public List<DialogueLine> lines = new List<DialogueLine>();
        }

        public string language = null;
        public string version = null;

        private List<Dialogue> dialogues = new List<Dialogue>();
        private DialogueData curDialogue = null;
        private Stack<DialogueBlockData> dialogueBlockStack = new Stack<DialogueBlockData>();
        private Stack<OptionsBlockData> optionsBlockStack = new Stack<OptionsBlockData>();
        private int nextBlockID = 0;

        public bool OnDialogueDefinition(string characterName, string dialogueName)
        {
            nextBlockID = 0;

            DialogueBlockData block = new DialogueBlockData();
            block.blockID = nextBlockID++;
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

        public bool OnNarratedDialogue(string characterName, string text)
        {
            DialogueLine line = new DialogueLine(LineType.NARRATED_DIALOGUE, new string[]{text});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnOptionsBegin()
        {
            OptionsBlockData options = new OptionsBlockData();
            optionsBlockStack.Push(options);
            return true;
        }

        public bool OnOption(string text)
        {
            // Close the previous option's dialogue block
            OptionsBlockData optionsBlock = optionsBlockStack.Peek();
            if (optionsBlock.options.Count > 0)
            {
                dialogueBlockStack.Pop();
            }
            
            DialogueBlockData block = new DialogueBlockData();
            block.blockID = nextBlockID++;
            dialogueBlockStack.Push(block);

            optionsBlockStack.Peek().options.Add((text, block));

            return true;
        }

        public bool OnOptionsEnd()
        {
            // Close the last option's dialogue block
            dialogueBlockStack.Pop();

            OptionsBlockData optionsBlock = optionsBlockStack.Pop();

            string[] lineData = new string[optionsBlock.options.Count * 2];
            int i = 0;
            foreach ((string text, DialogueBlockData block) in optionsBlock.options)
            {
                lineData[i++] = text;
                lineData[i++] = $"{block.blockID}";
                curDialogue.blocks.Add(block);
            }

            DialogueLine line = new DialogueLine(LineType.OPTION, lineData);
            dialogueBlockStack.Peek().lines.Add(line);

            return true;
        }

        public bool OnEnd()
        {
            DialogueLine line = new DialogueLine(LineType.END, new string[0]);
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public void OnDialogueDefinitionEnd()
        {
            // TODO Add an end line to the main block if it wasn't 
            curDialogue.blocks.Add(dialogueBlockStack.Pop());

            curDialogue.blocks.Sort((a, b) => a.blockID - b.blockID);
            List<DialogueBlock> dialogueBlocks = new List<DialogueBlock>();
            foreach (DialogueBlockData blockData in curDialogue.blocks)
            {
                dialogueBlocks.Add(new DialogueBlock(blockData.blockID, blockData.lines.ToArray()));
            }
            
            Dialogue dialogue = new Dialogue(curDialogue.characterName, curDialogue.dialogueName, dialogueBlocks.ToArray());
            dialogues.Add(dialogue);
        }

        public bool OnAction(string actionName, List<string> parameters)
        {
            List<string> lineData = new List<string>();
            lineData.Add(actionName);
            lineData.AddRange(parameters);
            DialogueLine line = new DialogueLine(LineType.ACTION, lineData.ToArray());
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnJump(string characterName, string dialogueName)
        {
            DialogueLine line = new DialogueLine(LineType.JUMP, new string[]{characterName, dialogueName});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public void SetVersion(string version)
        {
            this.version = version;
        }

        public void SetLanguage(string language)
        {
            this.language = language;
        }

        public List<Dialogue> Dialogues => dialogues;
    }
}
