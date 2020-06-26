using System;

namespace GabbyDialogue
{
    [Serializable]
    public class Dialogue
    {
        private string dialogueName;
        private string characterName;

        // TODO properties? How to handle arrays, objects?
        
        private string[] lines; // TODO maybe command pattern? 

        public Dialogue(string dialogueName, string characterName, string[] lines)
        {
            this.dialogueName = dialogueName;
            this.characterName = characterName;
            this.lines = lines;
        }
    }
}
