using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GabbyDialogue
{
    public class DialogParser
    {
        private class ParserState
        {
            public string line;
            public int indentLevel;
            public StreamReader stream;
        }

        private static Dictionary<char, Action<ParserState>> lineHandlerMap = new Dictionary<char, Action<ParserState>> {
            {'[', DialogParser.ParseDialogueBlockDefinition},
            {'(', DialogParser.ParseCharacterDialogue},
            {'-', DialogParser.ParseSequentialDialogue},
            {'+', DialogParser.ParseContinuedDialogue},
            {':', DialogParser.ParseOption},
            {'>', DialogParser.ParseAction},
            {'{', DialogParser.ParseProperties}
        };

        // TODO set regex cache size
        private static string regexValidName = @"[\w-]+(\s+[\w-]+)*";
        private static string regexEndsWithCommentOrNewline = @"\s*(//(.*)|$)";

        public static GabbyDialogueAsset ParseGabbyDialogueScript(string assetPath)
        {
            GabbyDialogueAsset asset = ScriptableObject.CreateInstance<GabbyDialogueAsset>();


            List<Dialogue> dialogues = new List<Dialogue>();

            try
            {
                using (StreamReader stream = new StreamReader(assetPath))
                {
                    ParserState state = new ParserState();
                    state.stream = stream;

                    // TODO parse metadata

                    string rawLine;
                    while ((rawLine = stream.ReadLine()) != null)
                    {
                        state.indentLevel = GetIndentLevel(rawLine);
                        state.line = rawLine.Trim();

                        if (state.line.Length == 0)
                        {
                            continue;
                        }
                        
                        if (!lineHandlerMap.ContainsKey(state.line[0]))
                        {
                            if (!ParseKeyword(state))
                            {
                                Debug.LogError($"Error while parsing Gabby dialogue script: {assetPath}\nUnrecognized symbols on line ${-1}: {state.line}\nMake sure the line begins with a symbol recognized by Gabby.");
                            }                  
                            continue;
                        }

                        lineHandlerMap[state.line[0]](state);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not parse gabby dialogue script: {assetPath}");
                Debug.LogError(e.Message);
                return asset;
            }

            return asset;
        }

        private static int GetIndentLevel(string line)
        {
            for (int i = 0; i < line.Length; ++i)
            {
                if (!Char.IsWhiteSpace(line[i]))
                {
                    return i;
                }
            }
            return 0;
        }

        private static void ParseDialogueBlockDefinition(ParserState state)
        {
            string validateDialogueBlock = @"^\s*\["
                                         + @"(?<c>" + regexValidName + @")"
                                         + @"."
                                         + @"(?<d>" + regexValidName + @")"
                                         + @"\]" + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateDialogueBlock);
            if (!match.Success)
            {
                Debug.LogError("blah");
            }

            string characterName = match.Groups["c"].Value;
            string dialogueName = match.Groups["d"].Value;
            Debug.Log($"{characterName} - {dialogueName}");
        }

        private static void ParseCharacterDialogue(ParserState state)
        {
            string validateCharacterDialogue = @"^\s*\("
                                             + @"(?<c>" + regexValidName + @")"
                                             + @"\)\s+(?<t>[^//]*)"
                                             + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateCharacterDialogue);
            if (!match.Success)
            {
                Debug.LogError("blah");
            }

            string characterName = match.Groups["c"].Value;
            string text = match.Groups["t"].Value;
            Debug.Log($"({characterName}) {text}");
        }

        private static void ParseSequentialDialogue(ParserState state)
        {
            string validateSequentialDialogue = @"^\s*\-\s+"
                                              + @"(?<t>[^//]*)"
                                              + regexEndsWithCommentOrNewline;
            
            Match match = Regex.Match(state.line, validateSequentialDialogue);
            if (!match.Success)
            {
                Debug.LogError("blah");
            }

            string text = match.Groups["t"].Value;
            Debug.Log($"- {text}");
        }

        private static void ParseContinuedDialogue(ParserState state)
        {
            string validateContinuedDialogue = @"^\s*\+\s+"
                                              + @"(?<t>[^//]*)"
                                              + regexEndsWithCommentOrNewline;
            
            Match match = Regex.Match(state.line, validateContinuedDialogue);
            if (!match.Success)
            {
                Debug.LogError("blah");
            }

            string text = match.Groups["t"].Value;
            Debug.Log($"+ {text}");
        }

        private static void ParseOption(ParserState state)
        {
            // TODO check indentation and determine block to add to
            string validateOption = @"^\s*\:\s+"
                                  + @"(?<t>[^//]*)"
                                  + regexEndsWithCommentOrNewline;
            
            Match match = Regex.Match(state.line, validateOption);
            if (!match.Success)
            {
                Debug.LogError("blah");
            }

            string text = match.Groups["t"].Value;
            Debug.Log($": {text}");
        }

        private static void ParseAction(ParserState state)
        {
            Match match;

            string validateJump = @"^\s*\>\>\s+"
                                  + @"(?<j>" + regexValidName + @")"
                                  + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateJump);
            if (match.Success)
            {
                string jumpPoint = match.Groups["j"].Value;
                Debug.Log($">> {jumpPoint}");
                return;
            }

            string validateAction = @"^\s*\>\s+"
                                  + @"(?<t>[^//]*)"
                                  + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateAction);
            if (match.Success)
            {
                string t = match.Groups["t"].Value;
                Debug.Log($"> ${t}");
                return;
            }

            Debug.LogError("blah");
        }

        private static void ParseProperties(ParserState state)
        {
            Debug.Log($"Properties not yet supported, ignoring.");
        }

        private static bool ParseKeyword(ParserState state)
        {
            Match match;

            string validateEnd = @"^\s*end\s*$";

            match = Regex.Match(state.line, validateEnd);
            if (match.Success)
            {
                Debug.Log("end");
                return true;
            }

            // TODO move metadata parsing into own section, with restriction that it must appear at the top of the file
            string validateVersion = @"^\s*gabby 0\.\d\s*$";

            match = Regex.Match(state.line, validateVersion);
            if (match.Success)
            {
                return true;
            }

            string validateLocale = @"^\s*locale(.*?)\s*$";

            match = Regex.Match(state.line, validateLocale);
            if (match.Success)
            {
                return true;
            }

            return false;
        }
    }
}
