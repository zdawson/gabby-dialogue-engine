using System.Collections.Generic;

namespace GabbyDialogue
{
    public interface IDialogueBuilder
    {
        bool OnDialogueDefinition(string characterName, string dialogueName);
        void OnDialogueDefinitionEnd();
        bool OnDialogueLine(string characterName, string text);
        bool OnContinuedDialogue(string characterName, string text);
        bool OnNarratedDialogue(string characterName, string text);
        bool OnOptionsBegin();
        bool OnOption(string text);
        bool OnOptionsEnd();
        bool OnEnd();
        bool OnAction(string actionName, List<string> parameters);
        bool OnJump(string characterName, string dialogueName);
        void SetNextLineTags(Dictionary<string, string> tags);
        void SetVersion(string version);
        void SetLanguage(string language);
    }
}
