using System.Threading.Tasks;
using System.Collections.Generic;

namespace GabbyDialogue
{
    public interface IDialogueHandler
    {        
        void OnDialogueLine(string characterName, string dialogueText, Dictionary<string, string> tags);
        void OnContinuedDialogue(string additionalDialogueText, Dictionary<string, string> tags);
        Task<int> OnOptionLine(string[] optionsText);
        void OnDialogueStart(Dialogue dialogue);
        void OnDialogueJump(Dialogue dialogue);
        void OnDialogueEnd();
        Dialogue GetDialogue(string characterName, string dialogueName);
    }
}
