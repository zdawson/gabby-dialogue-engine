using System;
using UnityEngine;
using UnityEditor;

using System.IO;
using GabbyDialogue;

namespace GabbyDialogue
{
    [UnityEditor.AssetImporters.ScriptedImporter(version: 2, ext: "gab")]
    public class GabbyFileImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            GabbyDialogueAsset asset = ScriptableObject.CreateInstance<GabbyDialogueAsset>();

            DialogueBuilder builder = new DialogueBuilder();
            bool success = DialogParser.ParseGabbyDialogueScript(ctx.assetPath, builder);
            if (success)
            {
                // Add the dialogues if parsed successfully
                // We still create the scriptable object so it appears in the hierarchy
                asset.dialogues = builder.Dialogues.ToArray();
                asset.version = builder.version;
                asset.language = builder.language;
            }         

            ctx.AddObjectToAsset("gabbyDialogue", asset);
            ctx.SetMainObject(asset);
        }
    }
}
