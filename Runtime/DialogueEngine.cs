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
            public uint currentLineWithinBlock = 0;
            public DialogueEngineState parentDialogueState = null;
        }

        private IDialogueEventHandler _dialogueHandler;
        private DialogueEngineState _state;
        private bool _blockNextLine = false;

        public DialogueEngine(IDialogueEventHandler dialogueHandler)
        {
            this._dialogueHandler = dialogueHandler;
        }

        private void SetDialogue(Dialogue dialogue)
        {
            _state = new DialogueEngineState
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
            _dialogueHandler.OnDialogueStart(dialogue);
            NextLine();
        }

        public void EndDialogue()
        {
            if (_state == null)
            {
                return;
            }
            _dialogueHandler.OnDialogueEnd();
            _state = null;
        }

        public void NextLine()
        {
            if (_state == null || _blockNextLine)
            {
                return;
            }

            ++_state.currentLineWithinBlock;

            // If we've stepped out of the current block, pop blocks until we return to one with more content
            while (_state.currentLineWithinBlock > _state.dialogueBlock.Lines.Length)
            {
                if (_state.parentDialogueState == null)
                {
                    EndDialogue();
                    return;
                }
                _state = _state.parentDialogueState;
                ++_state.currentLineWithinBlock;
            }

            DialogueLine line = _state.dialogueBlock.Lines[_state.currentLineWithinBlock - 1];
            HandleLine(line);
        }

        private void HandleLine(DialogueLine line)
        {
            switch (line.LineType)
            {
                case LineType.Dialogue:
                {
                    string characterName = line.LineData[0];
                    string text = line.LineData[1];
                    _dialogueHandler.OnDialogueLine(characterName, text, line.Tags);
                    break;
                }
                case LineType.NarratedDialogue:
                {
                    string text = line.LineData[0];
                    _dialogueHandler.OnDialogueLine("", text, line.Tags);
                    break;
                }
                case LineType.ContinuedDialogue:
                {
                    string text = line.LineData[0];
                    _dialogueHandler.OnContinuedDialogue(text, line.Tags);
                    break;
                }
                case LineType.Option:
                {
                    _blockNextLine = true;
                    HandleOptionAsync(line);
                    break;
                }
                case LineType.End:
                {
                    EndDialogue();
                    break;
                }
                case LineType.Action:
                {
                    bool autoAdvance = true;
                    string actionName = line.LineData[0];
                    List<string> actionParameters = new List<string>(line.LineData);
                    actionParameters.RemoveAt(0);

                    autoAdvance = _dialogueHandler.OnAction(actionName, actionParameters);

                    if (autoAdvance)
                    {
                        NextLine();
                    }
                    break;
                }
                case LineType.Jump:
                {
                    string characterName = line.LineData[0];
                    string dialogueName = line.LineData[1];
                    Dialogue dialogue = _dialogueHandler.GetDialogue(characterName, dialogueName);
                    if (dialogue == null)
                    {
                        Debug.LogError($"Dialogue jump target does not exist: {characterName}.{dialogueName}");
                        break;
                    }
                    // Jump to the new dialogue
                    SetDialogue(dialogue);
                    _dialogueHandler.OnDialogueJump(dialogue);
                    NextLine();
                    break;
                }
                case LineType.Conditional:
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
                            List<string> parameters = new List<string>(line.LineData.Skip(curPosition + 2).Take(numParams));
                            curPosition += 2 + numParams;

                            // Run the callback and see if the condition passes
                            if (_dialogueHandler.OnCondition(callback, parameters))
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
                parentDialogueState = this._state,
                dialogue = this._state.dialogue,
                dialogueBlock = this._state.dialogue.GetDialogueBlock(blockID),
                dialogueCharacterName = this._state.dialogueCharacterName,
                dialogueName = this._state.dialogueName,
            };
            this._state = nextBlockState;
            _blockNextLine = false;
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

            int selection = await _dialogueHandler.OnOptionLine(optionsText);

            if (selection < 0 || selection > optionsBlocks.Length)
            {
                _dialogueHandler.OnDialogueEnd();
            }

            PushDialogueBlock(optionsBlocks[selection]);
        }
    }
}
