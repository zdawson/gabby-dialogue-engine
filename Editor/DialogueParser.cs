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
            public int lineNumber = 0;
            public int indentLevel = 0;
            public StreamReader stream;
            public IDialogueBuilder builder;
            public string lastCharacter;
            public bool isParsingDialogue = false;
        }

        private static Dictionary<char, Func<ParserState, bool>> lineHandlerMap = new Dictionary<char, Func<ParserState, bool>> {
            {'[', DialogParser.ParseDialogueDefinition},
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

        public static bool ParseGabbyDialogueScript(string assetPath, IDialogueBuilder builder)
        {
            try
            {
                using (StreamReader stream = new StreamReader(assetPath))
                {
                    ParserState state = new ParserState();
                    state.stream = stream;
                    state.builder = builder;

                    // TODO parse metadata

                    string rawLine;
                    while ((rawLine = stream.ReadLine()) != null)
                    {
                        state.indentLevel = GetIndentLevel(rawLine);
                        state.line = rawLine.Trim();
                        state.lineNumber++;

                        if (state.line.Length == 0)
                        {
                            continue;
                        }

                        if ((lineHandlerMap.ContainsKey(state.line[0]) && lineHandlerMap[state.line[0]](state))
                           || state.line.StartsWith("//")
                           || ParseKeyword(state))
                        {
                            continue;
                        }
                        Debug.LogError($"Error while parsing Gabby dialogue script: {assetPath}\nUnrecognized symbols on line ${state.lineNumber}: {state.line}\nMake sure the line begins with a symbol recognized by Gabby.");
                    }

                    state.builder.OnDialogueDefinitionEnd();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not parse gabby dialogue script: {assetPath}");
                Debug.LogError(e.Message);
                return false;
            }

            return true;
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

        private static bool ParseDialogueDefinition(ParserState state)
        {
            string validateDialogueDefinition = @"^\s*\["
                                              + @"(?<c>" + regexValidName + @")"
                                              + @"."
                                              + @"(?<d>" + regexValidName + @")"
                                              + @"\]" + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateDialogueDefinition);
            if (!match.Success)
            {
                return false;
            }

            string characterName = match.Groups["c"].Value;
            string dialogueName = match.Groups["d"].Value;

            if (state.isParsingDialogue)
            {
                state.builder.OnDialogueDefinitionEnd();
            }

            state.isParsingDialogue = true;
            state.lastCharacter = characterName;
            return state.builder.OnDialogueDefinition(characterName, dialogueName);
        }

        private static bool ParseCharacterDialogue(ParserState state)
        {
            string validateCharacterDialogue = @"^\s*\("
                                             + @"(?<c>" + regexValidName + @")"
                                             + @"\)\s+(?<t>[^//]*)"
                                             + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateCharacterDialogue);
            if (!match.Success)
            {
                return false;
            }

            string characterName = match.Groups["c"].Value;
            string text = match.Groups["t"].Value;

            state.lastCharacter = characterName;
            return state.builder.OnDialogueLine(characterName, text);
        }

        private static bool ParseSequentialDialogue(ParserState state)
        {
            string validateSequentialDialogue = @"^\s*\-\s+"
                                              + @"(?<t>[^//]*)"
                                              + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateSequentialDialogue);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            return state.builder.OnDialogueLine(state.lastCharacter, text);
        }

        private static bool ParseContinuedDialogue(ParserState state)
        {
            string validateContinuedDialogue = @"^\s*\+\s+"
                                              + @"(?<t>[^//]*)"
                                              + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateContinuedDialogue);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            return state.builder.OnContinuedDialogue(state.lastCharacter, text);
        }

        private static bool ParseOption(ParserState state)
        {
            // TODO check indentation and determine block to add to
            string validateOption = @"^\s*\:\s+"
                                  + @"(?<t>[^//]*)"
                                  + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateOption);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            // TODO option callback
            return true;
        }

        private static bool ParseAction(ParserState state)
        {
            Match match;

            string validateJump = @"^\s*\>\>\s+"
                                  + @"(?<j>" + regexValidName + @")"
                                  + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateJump);
            if (match.Success)
            {
                string jumpPoint = match.Groups["j"].Value;
                // TODO jump callback
                return true;
            }

            string validateAction = @"^\s*\>\s+"
                                  + @"(?<t>[^//]*)"
                                  + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateAction);
            if (match.Success)
            {
                string t = match.Groups["t"].Value;
                // TODO action callback
                return true;
            }

            return false;
        }

        private static bool ParseProperties(ParserState state)
        {
            Debug.Log($"Properties not yet supported, ignoring.");
            return true;
        }

        private static bool ParseKeyword(ParserState state)
        {
            Match match;

            string validateEnd = @"^\s*end\s*$";

            match = Regex.Match(state.line, validateEnd);
            if (match.Success)
            {
                state.builder.OnEndDialogue();
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
