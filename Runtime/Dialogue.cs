using System;
using UnityEngine;

namespace GabbyDialogue
{
    
    public enum LineType
    {
        DIALOGUE, // [char, text]
        NARRATED_DIALOGUE,
        CONTINUED_DIALOGUE, // [text]
        OPTION, // [opt, jump, opt, jump, ...]
        ACTION, // [func, param, ...]
        JUMP, // [target]
        END // nothing
    }

    [Serializable]
    public class DialogueLine
    {
        [SerializeField]
        private LineType lineType;
        [SerializeField]
        private string[] lineData;

        public DialogueLine(LineType lineType, string[] lineData)
        {
            this.lineType = lineType;
            this.lineData = lineData;
        }

        public LineType LineType => lineType;
        public string[] LineData => lineData;
    }

    [Serializable]
    public class DialogueBlock
    {
        [SerializeField]
        private int blockID;
        [SerializeField]
        private DialogueLine[] lines;

        public DialogueBlock(int blockID, DialogueLine[] lines)
        {
            this.blockID = blockID;
            this.lines = lines;
        }

        public int BlockID => blockID;
        public DialogueLine[] Lines => lines;
    }

    [Serializable]
    public class Dialogue
    {
        [SerializeField]
        private string characterName;
        [SerializeField]
        private string dialogueName;
        [SerializeField]
        private DialogueBlock[] dialogueBlocks;

        // TODO properties? How to handle arrays, objects?

        public Dialogue(string characterName, string dialogueName, DialogueBlock[] dialogueBlocks)
        {
            this.characterName = characterName;
            this.dialogueName = dialogueName;
            this.dialogueBlocks = dialogueBlocks;
        }

        public DialogueBlock GetMainDialogueBlock()
        {
            return GetDialogueBlock(0);
        }

        public DialogueBlock GetDialogueBlock(int blockID)
        {
            return dialogueBlocks[blockID];
        }

        public string DialogueName => dialogueName;
        public string CharacterName => characterName;
    }
}
