using System.Threading.Tasks;

namespace GabbyDialogue
{
    public interface IDialogueHandler
    {
        void OnDialogueLine(string characterName, string dialogueText);
        void OnContinuedDialogue(string additionalDialogueText);
        Task<int> OnOptionLine(string[] optionsText);
        void OnDialogueEnd();
        Dialogue GetDialogue(string characterName, string dialogueName);
    }
}
