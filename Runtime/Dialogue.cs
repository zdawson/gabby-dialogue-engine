using System;
using UnityEngine;

namespace GabbyDialogue
{

    [Serializable]
    public class DialogueLine
    {
        [SerializeField]
        private string characterName;
        [SerializeField]
        private string text;
        [SerializeField]
        public bool isContinuation = false;

        public DialogueLine(string characterName, string text)
        {
            this.characterName = characterName;
            this.text = text;
        }

        public string CharacterName => characterName;
        public string Text => text;
    }

    [Serializable]
    public class OptionLine
    {
        [SerializeField]
        private string[] options;
        [SerializeField]
        private DialogueBlock[] dialogueBlocks;

        public OptionLine()
        {
            
        }

        public string[] Options => options;
        public DialogueBlock[] DialogueBlocks => dialogueBlocks;
    }

    [Serializable]
    public class DialogueBlock
    {
        [SerializeField]
        private DialogueLine[] lines;

        public DialogueBlock(DialogueLine[] lines)
        {
            this.lines = lines;
        }

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
        private DialogueBlock dialogueBlock;

        // TODO properties? How to handle arrays, objects?

        public Dialogue(string characterName, string dialogueName, DialogueBlock dialogueBlock)
        {
            this.characterName = characterName;
            this.dialogueName = dialogueName;
            this.dialogueBlock = dialogueBlock;
        }

        public string DialogueName => dialogueName;
        public string CharacterName => characterName;
        public DialogueBlock DialogueBlock => dialogueBlock;
    }
}
