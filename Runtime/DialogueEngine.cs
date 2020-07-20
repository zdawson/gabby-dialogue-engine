using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GabbyDialogue
{
    public class DialogueEngine
    {
        [Serializable]
        public class DialogueEngineState
        {
            [NonSerialized]
            public Dialogue dialogue;
            [NonSerialized]
            public DialogueBlock dialogueBlock;
            public string dialogueCharacterName;
            public string dialogueName;
            public uint currentLine = 0;
            public DialogueEngineState parentDialogueState = null;
        }

        private IDialogueHandler dialogueHandler;
        private DialogueEngineState state;
        private bool blockNextLine = false;

        public DialogueEngine(IDialogueHandler dialogueHandler)
        {
            this.dialogueHandler = dialogueHandler;
        }

        private void SetDialogue(Dialogue dialogue)
        {
            state = new DialogueEngineState
            {
                dialogue = dialogue,
                dialogueBlock = dialogue.GetMainDialogueBlock(),
                dialogueCharacterName = dialogue.CharacterName,
                dialogueName = dialogue.DialogueName
            };
        }

        public void StartDialogue(Dialogue dialogue)
        {
            SetDialogue(dialogue);
            NextLine();
        }

        public void NextLine()
        {
            if (blockNextLine)
            {
                return;
            }

            ++state.currentLine;
            while (state.currentLine > state.dialogueBlock.Lines.Length)
            {
                if (state.parentDialogueState == null)
                {
                    dialogueHandler.OnDialogueEnd();
                    state = null;
                    return;
                }
                state = state.parentDialogueState;
                ++state.currentLine;
            }
            DialogueLine line = state.dialogueBlock.Lines[state.currentLine - 1];
            HandleLine(line);
        }

        void PrintDialogueBlock(Dialogue dialogue, DialogueBlock block)
        {
            foreach (DialogueLine line in block.Lines)
            {
                HandleLine(line);
            }
        }

        private void HandleLine(DialogueLine line)
        {
            switch (line.LineType)
            {
                case LineType.DIALOGUE:
                {
                    string characterName = line.LineData[0];
                    string text = line.LineData[1];
                    dialogueHandler.OnDialogueLine(characterName, text);
                    break;
                }
                case LineType.CONTINUED_DIALOGUE:
                {
                    string text = line.LineData[0];
                    dialogueHandler.OnContinuedDialogue(text);
                    break;
                }
                case LineType.OPTION:
                {
                    blockNextLine = true;
                    HandleOptionAsync(line);
                    break;
                }
                case LineType.END:
                {
                    dialogueHandler.OnDialogueEnd();
                    return;
                }
            }
        }

        private async void HandleOptionAsync(DialogueLine line)
        {
            string[] optionsText = new string[line.LineData.Length / 2];
            int[] optionsBlocks = new int[line.LineData.Length / 2];
            for (int i = 0; i < line.LineData.Length; i += 2)
            {
                string text = line.LineData[i];
                int blockID = Int32.Parse(line.LineData[i + 1]);
                optionsText[i/2] = text;
                optionsBlocks[i/2] = blockID;
            }

            int selection = await dialogueHandler.OnOptionLine(optionsText);

            if (selection < 0 || selection > optionsBlocks.Length)
            {
                dialogueHandler.OnDialogueEnd();
            }

            // Push a new dialogue engine state for the block
            DialogueEngineState nextBlockState = new DialogueEngineState {
                parentDialogueState = this.state,
                dialogue = this.state.dialogue,
                dialogueBlock = this.state.dialogue.GetDialogueBlock(optionsBlocks[selection]),
                dialogueCharacterName = this.state.dialogueCharacterName,
                dialogueName = this.state.dialogueName,
            };
            this.state = nextBlockState;
            blockNextLine = false;
        }
    }
}
