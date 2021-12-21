using UnityEngine;

namespace PotassiumK.GabbyDialogue
{
    [UnityEditor.AssetImporters.ScriptedImporter(version: 3, ext: "gab")]
    public class GabbyFileImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            DialogueScript asset = ScriptableObject.CreateInstance<DialogueScript>();

            DialogueBuilder builder = new DialogueBuilder();
            bool success = DialogParser.ParseGabbyDialogueScript(ctx.assetPath, builder);
            if (success)
            {
                // Add the dialogues if parsed successfully
                // We still create the scriptable object so it appears in the hierarchy
                asset.dialogues = builder.Dialogues.ToArray();
                asset.version = builder.version.ToString();
                asset.language = builder.language;
            }

            ctx.AddObjectToAsset("gabbyDialogue", asset);
            ctx.SetMainObject(asset);
        }
    }
}
