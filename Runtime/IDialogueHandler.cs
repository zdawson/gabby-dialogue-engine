using System.Threading.Tasks;
using System.Collections.Generic;

namespace PotassiumK.GabbyDialogue
{
    public interface IDialogueEventHandler
    {
        void OnDialogueStart(Dialogue dialogue);
        void OnDialogueEnd();
        void OnDialogueLine(string characterName, string dialogueText, Dictionary<string, string> tags);
        void OnContinuedDialogue(string continuedDialogueText, Dictionary<string, string> tags);
        void OnDialogueJump(Dialogue dialogue);
        Task<int> OnOptionLine(string[] optionsText);
        bool OnAction(string actionName, List<string> parameters);
        bool OnCondition(string conditionalName, List<string> parameters);
        Dialogue GetDialogue(string characterName, string dialogueName);
    }
}
