using System;
using System.Collections.Generic;
using System.Linq;
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
            public bool isNarration = false;
            public DialogueEngineState parentDialogueState = null;
        }

        private IDialogueHandler dialogueHandler;
        private AbstractScriptingHandler scriptingHandler;
        private DialogueEngineState state;
        private bool blockNextLine = false;

        public DialogueEngine(IDialogueHandler dialogueHandler, AbstractScriptingHandler scriptingHandler = null)
        {
            this.dialogueHandler = dialogueHandler;
            this.scriptingHandler = scriptingHandler;
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

        private void HandleLine(DialogueLine line)
        {
            // Appended dialogue continues narration, anything else unsets it
            if (state.isNarration && (line.LineType != LineType.CONTINUED_DIALOGUE))
            {
                state.isNarration = false;
            };

            switch (line.LineType)
            {
                case LineType.DIALOGUE:
                {
                    string characterName = line.LineData[0];
                    string text = line.LineData[1];
                    dialogueHandler.OnDialogueLine(characterName, text, line.Tags);
                    break;
                }
                case LineType.NARRATED_DIALOGUE:
                {
                    string text = line.LineData[0];
                    dialogueHandler.OnDialogueLine("", text, line.Tags);
                    break;
                }
                case LineType.CONTINUED_DIALOGUE:
                {
                    string text = line.LineData[0];
                    dialogueHandler.OnContinuedDialogue(text, line.Tags);
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
                    break;
                }
                case LineType.ACTION:
                {
                    bool autoAdvance = true;
                    if (scriptingHandler != null)
                    {
                        string actionName = line.LineData[0];
                        List<string> actionParameters = new List<string>(line.LineData);
                        actionParameters.RemoveAt(0);
                        autoAdvance = scriptingHandler.OnAction(actionName, actionParameters);
                    }
                    if (autoAdvance)
                    {
                        NextLine();   
                    }
                    break;
                }
                case LineType.JUMP:
                {
                    string characterName = line.LineData[0];
                    string dialogueName = line.LineData[1];
                    Dialogue dialogue = dialogueHandler.GetDialogue(characterName, dialogueName);
                    if (dialogue == null)
                    {
                        Debug.LogWarning($"Dialogue jump target does not exist: {characterName}.{dialogueName}");
                        break;
                    }
                    StartDialogue(dialogue);
                    break;
                }
                case LineType.CONDITIONAL:
                {
                    // Need to process the conditional layout due to current format limitations
                    string layoutString = line.LineData[0];
                    string[] blocks = layoutString.Split(',');
                    int curPosition = 1;
                    for (int i = 0; i < blocks.Length; ++i)
                    {
                        if (blocks[i] != "e")
                        {
                            int numParams = Convert.ToInt32(blocks[i]);
                            int jump = Convert.ToInt32(line.LineData[curPosition]);
                            string callback = line.LineData[curPosition + 1];
                            string[] parameters = line.LineData.Skip(curPosition + 2).Take(numParams).ToArray();
                            curPosition += 2 + numParams;

                            // Run the callback and see if the condition passes
                            if (RunCondition(jump, callback, parameters))
                            {
                                PushDialogueBlock(jump);
                                break;
                            }
                        }
                        else
                        {
                            int jump = Convert.ToInt32(line.LineData[curPosition]);
                            PushDialogueBlock(jump);
                            break;
                        }
                    }
                    NextLine();
                    break;
                }
            }
        }

        private void PushDialogueBlock(int blockID)
        {
            // Push a new dialogue engine state for the block
            DialogueEngineState nextBlockState = new DialogueEngineState {
                parentDialogueState = this.state,
                dialogue = this.state.dialogue,
                dialogueBlock = this.state.dialogue.GetDialogueBlock(blockID),
                dialogueCharacterName = this.state.dialogueCharacterName,
                dialogueName = this.state.dialogueName,
            };
            this.state = nextBlockState;
            blockNextLine = false;
        }

        private bool RunCondition(int jump, string callback, string[] parameters)
        {
            bool result;
            bool handled = scriptingHandler.OnCondition(callback, parameters, out result);
            if (!handled)
            {
                Debug.LogWarning($"Unhandled conditional: {callback}");
                return false;
            }

            return result;
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

            PushDialogueBlock(optionsBlocks[selection]);
        }
    }
}
