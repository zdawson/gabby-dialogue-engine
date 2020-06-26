using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GabbyDialogue
{
    public interface IDialogueBuilder
    {
        bool OnDialogueDefinition(string characterName, string dialogueName);
        void OnDialogueDefinitionEnd();
        bool OnDialogueLine(string characterName, string text);
        bool OnContinuedDialogue(string characterName, string text);
        bool OnEndDialogue();
    }
}
