using System.Threading.Tasks;

public interface IDialogueHandler
{
    void OnDialogueLine(string characterName, string dialogueText);
    void OnContinuedDialogue(string additionalDialogueText);
    Task<int> OnOptionLine(string[] optionsText);
    void OnSpeakingCharacterChanged(string characterName);
    void OnAction(string actionName, string[] parameters);
    void OnDialogueEnd();

    void GetVariable(string[] variablePath);
    void SetVariable(string[] variablePath, dynamic value);
}
