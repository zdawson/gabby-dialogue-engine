using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GabbyDialogue
{
    public class DialogueEngine : MonoBehaviour
    {

        public GabbyDialogueAsset dialogueAsset;

        void Start()
        {
            foreach (Dialogue dialogue in dialogueAsset.dialogues)
            {
                Debug.Log($"{dialogue.CharacterName}, {dialogue.DialogueName}");
                PrintDialogueBlock(dialogue, dialogue.GetMainDialogueBlock());
            }
        }

        void PrintDialogueBlock(Dialogue dialogue, DialogueBlock block)
        {
            foreach (DialogueLine line in block.Lines)
            {
                switch (line.LineType)
                {
                    case LineType.DIALOGUE:
                    {
                        string characterName = line.LineData[0];
                        string text = line.LineData[1];
                        Debug.Log($"{characterName}: {text}");
                        break;
                    }
                    case LineType.CONTINUED_DIALOGUE:
                    {
                        string text = line.LineData[0];
                        Debug.Log($"+ {text}");
                        break;
                    }
                    case LineType.OPTION:
                    {
                        for (int i = 0; i < line.LineData.Length; i += 2)
                        {
                            string text = line.LineData[i];
                            int blockID = Int32.Parse(line.LineData[i+1]);
                            Debug.Log($"OPTION: {text}");
                            PrintDialogueBlock(dialogue, dialogue.GetDialogueBlock(blockID));
                        }
                        break;
                    }
                    case LineType.END:
                    {
                        return;
                    }
                }
            }
        }
    }
}
