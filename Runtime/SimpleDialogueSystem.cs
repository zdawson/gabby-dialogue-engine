using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PotassiumK.GabbyDialogue
{
    public abstract class SimpleDialogueSystem : IDialogueEventHandler
    {
        protected DialogueEngine dialogueEngine;
        protected SimpleScriptEventHandler scriptEventHandler;

        protected List<DialogueScript> dialogueScripts = new List<DialogueScript>();
        protected Dictionary<int, List<Dialogue>> dialogues = new Dictionary<int, List<Dialogue>>(); // Store multiple values per key

        private string _language = "";
        private string _fallbackLanguage = "";

        public SimpleDialogueSystem()
        {
            dialogueEngine = new DialogueEngine(this);
            scriptEventHandler = new SimpleScriptEventHandler();
        }

        public virtual void PlayDialogue(string characterName, string dialogueName)
        {
            Dialogue dialogue = GetDialogue(characterName, dialogueName);
            if (dialogue == null)
            {
                Debug.LogError($"Dialogue [{characterName}.{dialogueName}] not loaded.\nlanguage: '{_language}'. fallback: '{_fallbackLanguage}'.");
                return;
            }

            PlayDialogue(dialogue);
        }

        public virtual void PlayDialogue(Dialogue dialogue)
        {
            Debug.Assert(dialogue != null);
            dialogueEngine.StartDialogue(dialogue);
        }

        public virtual void EndDialogue()
        {
            dialogueEngine.EndDialogue();
        }

        public virtual void NextLine()
        {
            dialogueEngine.NextLine();
        }

        public void AddScript(DialogueScript dialogueScript)
        {
            Debug.Assert(dialogueScript != null);

            dialogueScripts.Add(dialogueScript);
            foreach (Dialogue dialogue in dialogueScript.dialogues)
            {
                AddDialogue(dialogueScript.language, dialogue);
            }

            // If the language hasn't been set yet, default to whatever the script is using
            if (_language == "")
            {
                _language = dialogueScript.language;
            }
        }

        public void RemoveScript(DialogueScript dialogueScript)
        {
            Debug.Assert(dialogueScript != null);

            dialogueScripts.Remove(dialogueScript);
            foreach (Dialogue dialogue in dialogueScript.dialogues)
            {
                RemoveDialogue(dialogueScript.language, dialogue);
            }
        }

        private void AddDialogue(string language, Dialogue dialogue)
        {
            Debug.Assert(dialogue != null);

            int hashCode = GetDialogueHashCode(language, dialogue.CharacterName, dialogue.DialogueName);

            List<Dialogue> existingDialogues;
            if (!dialogues.TryGetValue(hashCode, out existingDialogues))
            {
                dialogues.Add(hashCode, new List<Dialogue>() {dialogue});
                return;
            }

            for (int i = existingDialogues.Count - 1; i >= 0; --i)
            {
                Dialogue other = existingDialogues[i];
                if (dialogue.CharacterName == other.CharacterName
                 && dialogue.DialogueName == other.DialogueName)
                {
                    // The dialogue is already loaded, don't reload it
                    // NOTE: This will not handle hash collisions between different languages where the character and dialogue names are the same
                    //       Ie. where hash(lang1.character.dialogue) == hash(lang2.character.dialogue)
                    Debug.LogWarning($"Duplicate dialogue names, ignoring\n[{dialogue.CharacterName}.{dialogue.DialogueName}]");
                    return;
                }
            }

            // The dialogue is not already loaded, it's just a hash collision. Add it to the list.
            existingDialogues.Add(dialogue);
        }

        private void RemoveDialogue(string language, Dialogue dialogue)
        {
            Debug.Assert(dialogue != null);

            int hashCode = GetDialogueHashCode(language, dialogue.CharacterName, dialogue.DialogueName);

            List<Dialogue> existingDialogues;
            if (!dialogues.TryGetValue(hashCode, out existingDialogues))
            {
                return;
            }

            for (int i = existingDialogues.Count - 1; i >= 0; --i)
            {
                Dialogue other = existingDialogues[i];
                if (dialogue == other)
                {
                    existingDialogues.RemoveAt(i);
                    return;
                }
            }
        }

        private void RemoveAll()
        {
            dialogues.Clear();
            dialogueScripts.Clear();
        }

        public Dialogue GetDialogue(string characterName, string dialogueName)
        {
            Dialogue result = GetDialogue(_language, characterName, dialogueName);
            if (result != null)
            {
                return result;
            }

            return GetDialogue(_fallbackLanguage, characterName, dialogueName);
        }

        private Dialogue GetDialogue(string language, string characterName, string dialogueName)
        {
            List<Dialogue> matchingDialogues;
            int hash = GetDialogueHashCode(language, characterName, dialogueName);
            int hash2 = GetDialogueHashCode(language, characterName, dialogueName);
            int hash3 = GetDialogueHashCode(language, characterName, dialogueName);
            if (dialogues.TryGetValue(GetDialogueHashCode(language, characterName, dialogueName), out matchingDialogues))
            {
                if (matchingDialogues.Count == 1)
                {
                    return matchingDialogues[0];
                }

                foreach (Dialogue dialogue in matchingDialogues)
                {
                if (dialogue.CharacterName == characterName
                    && dialogue.DialogueName == dialogueName)
                    {
                        return dialogue;
                    }
                }
            }

            return null;
        }

        public abstract void OnContinuedDialogue(string continuedDialogueText, Dictionary<string, string> tags);
        public abstract void OnDialogueEnd();
        public abstract void OnDialogueJump(Dialogue dialogue);
        public abstract void OnDialogueLine(string characterName, string dialogueText, Dictionary<string, string> tags);
        public abstract void OnDialogueStart(Dialogue dialogue);
        public abstract Task<int> OnOptionLine(string[] optionsText);

        public virtual bool OnAction(string actionName, List<string> parameters)
        {
            return scriptEventHandler.OnAction(actionName, parameters);
        }

        public virtual bool OnCondition(string conditionalName, List<string> parameters)
        {
            return scriptEventHandler.OnConditional(conditionalName, parameters);
        }

        private static int GetDialogueHashCode(string language, string characterName, string dialogueName)
        {
            return $"{language}:{characterName}:{dialogueName}".GetHashCode();
        }

        /// <summary>
        /// Sets the language to use for dialogue, and an optional fallback language if a dialogue can't be found.
        /// If a dialogue is already in progress, it must be restarted before the language change will take effect.
        /// </summary>
        public void SetLanguage(string language, string fallbackLanguage = "")
        {
            this._language = language;
            this._fallbackLanguage = fallbackLanguage;
        }

        public string GetLanguage() => _language;
        public string GetFallbackLanguage() => _fallbackLanguage;
    }
}
