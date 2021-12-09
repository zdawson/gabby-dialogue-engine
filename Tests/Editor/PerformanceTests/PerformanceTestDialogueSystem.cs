using GabbyDialogue;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PerformanceTestDialogueSystem : SimpleDialogueSystem
{

    bool isDialogueFinished = false;

    public PerformanceTestDialogueSystem()
    {
        scriptEventHandler.RegisterScriptEventHandler(new PerformanceTestScriptEventHandler());
    }

    public void PlayAllLines()
    {
        while (!isDialogueFinished)
        {
            NextLine();
        }
    }

    public override void OnDialogueStart(Dialogue dialogue)
    {
        isDialogueFinished = false;
    }

    public override void OnDialogueEnd()
    {
        isDialogueFinished = true;
    }

    public override void OnDialogueLine(string characterName, string dialogueText, Dictionary<string, string> tags)
    {
    }

    public override void OnContinuedDialogue(string continuedDialogueText, Dictionary<string, string> tags)
    {
    }

    public override void OnDialogueJump(Dialogue dialogue)
    {
    }

    public override Task<int> OnOptionLine(string[] optionsText)
    {
        throw new System.NotImplementedException();
    }
}
