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
                foreach (DialogueLine line in dialogue.DialogueBlock.Lines)
                {
                    DialogueLine dialogueLine = line as DialogueLine;
                    Debug.Log($"{dialogueLine.CharacterName}: {dialogueLine.Text}");
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
