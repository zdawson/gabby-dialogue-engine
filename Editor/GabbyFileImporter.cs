using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using GabbyDialogue;

namespace GabbyDialogue
{
    [ScriptedImporter(version: 1, ext: "gab")]
    public class GabbyFileImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            GabbyDialogueAsset asset = ScriptableObject.CreateInstance<GabbyDialogueAsset>();

            DialogueBuilder builder = new DialogueBuilder();
            bool success = DialogParser.ParseGabbyDialogueScript(ctx.assetPath, builder);
            if (success)
            {
                // Add the dialogues if parsed successfully
                // We still create the scriptable object so it appears in the hierarchy
                asset.dialogues = builder.Dialogues.ToArray();
            }         

            ctx.AddObjectToAsset("gabbyDialogue", asset);
            ctx.SetMainObject(asset);
        }
    }
}
