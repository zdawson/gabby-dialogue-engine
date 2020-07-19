using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GabbyDialogue
{
    /**
     * Processed .gab file data
     * This should store data in a way that's convenient for the dialogue engine
     */
    public class GabbyDialogueAsset : ScriptableObject
    {
        public Dialogue[] dialogues;
    }
}
