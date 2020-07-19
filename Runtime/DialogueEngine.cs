using System;
using System.Collections;
using System.Collections.Generic;
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
            public string characterName;
            public string dialogueName;
            public uint currentLine = 0;
            public DialogueEngineState parentDialogueState = null;
        }

        private IDialogueHandler dialogueHandler;
        private DialogueEngineState state;

        public DialogueEngine(IDialogueHandler dialogueHandler)
        {
            this.dialogueHandler = dialogueHandler;
        }

        private void SetDialogue(Dialogue dialogue)
        {
            state = new DialogueEngineState
            {
                dialogue = dialogue,
                characterName = dialogue.CharacterName,
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
            ++state.currentLine;
            DialogueLine line = state.dialogue.GetMainDialogueBlock().Lines[state.currentLine - 1];
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
                    for (int i = 0; i < line.LineData.Length; i += 2)
                    {
                        string text = line.LineData[i];
                        int blockID = Int32.Parse(line.LineData[i + 1]);
                        Debug.Log($"OPTION: {text}");
                        DialogueBlock optionBlock = state.dialogue.GetDialogueBlock(blockID);
                    }
                    break;
                }
                case LineType.END:
                {
                    dialogueHandler.OnDialogueEnd();
                    return;
                }
            }

        }
    }
}
