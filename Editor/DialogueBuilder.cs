using System.Collections.Generic;

namespace GabbyDialogue
{
    public class DialogueBuilder : IDialogueBuilder
    {
        private class DialogueData
        {
            public string characterName;
            public string dialogueName;
            public Dictionary<string, string> tags;
            public List<DialogueBlockData> blocks = new List<DialogueBlockData>();

            public DialogueData(string characterName, string dialogueName, Dictionary<string, string> tags)
            {
                this.characterName = characterName;
                this.dialogueName = dialogueName;
                this.tags = new Dictionary<string, string>(tags);
            }
        }

        private class OptionsBlockData
        {
            public List<(string, DialogueBlockData)> options = new List<(string, DialogueBlockData)>();
        }

        private class ConditionalBlockData
        {
            public List<(string, List<string>, DialogueBlockData)> conditionalBlocks = new List<(string, List<string>, DialogueBlockData)>();
            public DialogueBlockData elseBlock;
        }

        private class DialogueBlockData
        {
            public int blockID;
            public List<DialogueLine> lines = new List<DialogueLine>();
        }

        public string language = null;
        public System.Version version = null;
        public Dictionary<string, string> nextLineTags = new Dictionary<string, string>();

        private List<Dialogue> dialogues = new List<Dialogue>();
        private DialogueData curDialogue = null;
        private Stack<DialogueBlockData> dialogueBlockStack = new Stack<DialogueBlockData>();
        private Stack<OptionsBlockData> optionsBlockStack = new Stack<OptionsBlockData>();
        private Stack<ConditionalBlockData> conditionalBlockStack = new Stack<ConditionalBlockData>();
        private int nextBlockID = 0;

        public bool OnDialogueDefinition(string characterName, string dialogueName)
        {
            nextBlockID = 0;

            DialogueBlockData block = new DialogueBlockData();
            block.blockID = nextBlockID++;
            dialogueBlockStack.Push(block);

            DialogueData dialogueData = new DialogueData(characterName, dialogueName, nextLineTags);
            curDialogue = dialogueData;

            nextLineTags.Clear();

            return true;
        }

        public bool OnDialogueLine(string characterName, string text)
        {
            DialogueLine line = CreateLine(LineType.Dialogue, new string[]{characterName, text});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnContinuedDialogue(string characterName, string text)
        {
            DialogueLine line = CreateLine(LineType.ContinuedDialogue, new string[]{text});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnNarratedDialogue(string characterName, string text)
        {
            DialogueLine line = CreateLine(LineType.NarratedDialogue, new string[]{text});
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

            DialogueLine line = CreateLine(LineType.Option, lineData);
            dialogueBlockStack.Peek().lines.Add(line);

            return true;
        }

        public bool OnEnd()
        {
            DialogueLine line = CreateLine(LineType.End, new string[0]);
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public void OnDialogueDefinitionEnd()
        {
            curDialogue.blocks.Add(dialogueBlockStack.Pop());

            curDialogue.blocks.Sort((a, b) => a.blockID - b.blockID);
            List<DialogueBlock> dialogueBlocks = new List<DialogueBlock>();
            foreach (DialogueBlockData blockData in curDialogue.blocks)
            {
                dialogueBlocks.Add(new DialogueBlock(blockData.blockID, blockData.lines.ToArray()));
            }

            Dialogue dialogue = new Dialogue(curDialogue.characterName, curDialogue.dialogueName, curDialogue.tags, dialogueBlocks.ToArray());
            dialogues.Add(dialogue);
        }

        public bool OnAction(string actionName, List<string> parameters)
        {
            List<string> lineData = new List<string>();
            lineData.Add(actionName);
            lineData.AddRange(parameters);
            DialogueLine line = CreateLine(LineType.Action, lineData.ToArray());
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnJump(string characterName, string dialogueName)
        {
            DialogueLine line = CreateLine(LineType.Jump, new string[]{characterName, dialogueName});
            dialogueBlockStack.Peek().lines.Add(line);
            return true;
        }

        public bool OnConditionalBegin()
        {
            ConditionalBlockData block = new ConditionalBlockData();
            conditionalBlockStack.Push(block);
            return true;
        }

        public bool OnConditionalEnd()
        {
            // Close the last open dialogue block
            dialogueBlockStack.Pop();

            ConditionalBlockData conditionalBlock = conditionalBlockStack.Pop();

            // [[n,n,...,e], callback, param, param, ..., jump, callback, ..., elsejump]
            string dataLayoutString = "";
            List<string> lineData = new List<string>();
            foreach ((string callback, List<string> parameters, DialogueBlockData dialogueBlock) in conditionalBlock.conditionalBlocks)
            {
                lineData.Add($"{dialogueBlock.blockID}");
                lineData.Add(callback);
                lineData.AddRange(parameters);
                curDialogue.blocks.Add(dialogueBlock);

                dataLayoutString += $"{parameters.Count},";
            }

            if (conditionalBlock.elseBlock != null)
            {
                lineData.Add($"{conditionalBlock.elseBlock.blockID}");
                curDialogue.blocks.Add(conditionalBlock.elseBlock);
                dataLayoutString += "e";
            }
            else
            {
                // Drop the last comma
                dataLayoutString = dataLayoutString.Substring(0, dataLayoutString.Length - 1);
            }

            lineData.Insert(0, dataLayoutString);

            DialogueLine line = CreateLine(LineType.Conditional, lineData.ToArray());
            dialogueBlockStack.Peek().lines.Add(line);

            return true;
        }

        public bool OnIf(string callbackName, List<string> parameters)
        {
            // Start a dialogue block for this condition
            DialogueBlockData block = new DialogueBlockData();
            block.blockID = nextBlockID++;
            dialogueBlockStack.Push(block);

            conditionalBlockStack.Peek().conditionalBlocks.Add((callbackName, parameters, block));

            return true;
        }

        public bool OnElseIf(string callbackName, List<string> parameters)
        {
            // Close the previous condition's dialogue block
            ConditionalBlockData conditionalBlock = conditionalBlockStack.Peek();
            if (conditionalBlock.conditionalBlocks.Count > 0)
            {
                dialogueBlockStack.Pop();
            }

            // Start a dialogue block for this condition
            DialogueBlockData block = new DialogueBlockData();
            block.blockID = nextBlockID++;
            dialogueBlockStack.Push(block);

            conditionalBlockStack.Peek().conditionalBlocks.Add((callbackName, parameters, block));

            return true;
        }

        public bool OnElse()
        {
            // Close the previous condition's dialogue block
            ConditionalBlockData conditionalBlock = conditionalBlockStack.Peek();
            if (conditionalBlock.conditionalBlocks.Count > 0)
            {
                dialogueBlockStack.Pop();
            }

            // Start a dialogue block for this condition
            DialogueBlockData block = new DialogueBlockData();
            block.blockID = nextBlockID++;
            dialogueBlockStack.Push(block);

            // Else has no condition
            conditionalBlockStack.Peek().elseBlock = block;

            return true;
        }

        public void SetVersion(System.Version version)
        {
            this.version = version;
        }

        public void SetLanguage(string language)
        {
            this.language = language;
        }

        public void SetNextLineTags(Dictionary<string, string> tags)
        {
            this.nextLineTags = tags;
        }

        private DialogueLine CreateLine(LineType lineType, string[] lineData)
        {
            DialogueLine line = new DialogueLine(lineType, lineData);
            line.Tags = nextLineTags;
            nextLineTags.Clear();
            return line;
        }

        public List<Dialogue> Dialogues => dialogues;
    }
}
