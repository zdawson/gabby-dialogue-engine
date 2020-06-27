using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GabbyDialogue
{
    public class DialogueEngine : MonoBehaviour
    {

        public GabbyDialogueAsset dialogueAsset;

        // Start is called before the first frame update
        void Start()
        {
            foreach (Dialogue dialogue in dialogueAsset.dialogues)
            {
                Debug.Log($"{dialogue.CharacterName}, {dialogue.DialogueName}");
                PrintDialogueBlock(dialogue.GetMainDialogueBlock());
            }
        }

        void PrintDialogueBlock(DialogueBlock block)
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
                    case LineType.END:
                    {
                        return;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
