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
            GabbyDialogueAsset asset = DialogParser.ParseGabbyDialogueScript(ctx.assetPath);

            ctx.AddObjectToAsset("gabbyDialogue", asset);
            ctx.SetMainObject(asset);
        }
    }
}
