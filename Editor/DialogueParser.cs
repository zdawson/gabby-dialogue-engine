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
            public string rawLine;
            public string line;
            public int lineNumber = 0;
            public int indentLevel = 0;
            public bool blockReadLine = false;
            public StreamReader stream;
            public IDialogueBuilder builder;
            public string lastCharacter;
            public bool isParsingDialogue = false;
            public string assetPath;
            
        }

        private static Dictionary<char, Func<ParserState, bool>> lineHandlerMap = new Dictionary<char, Func<ParserState, bool>> {
            {'[', DialogParser.ParseDialogueDefinition},
            {'(', DialogParser.ParseCharacterDialogue},
            {'-', DialogParser.ParseSequentialDialogue},
            {'+', DialogParser.ParseContinuedDialogue},
            {':', DialogParser.ParseOptionBlock},
            {'>', DialogParser.ParseAction},
            {'{', DialogParser.ParseProperties}
        };

        // TODO set regex cache size
        private static string regexValidName = @"[\w-]+(\s+[\w-]+)*";
        private static string regexEndsWithCommentOrNewline = @"\s*(//(.*)|$)";
        private static string regexNonCommentCharacterSequence = @"[^/]*(/?[^/]*)*";

        public static bool ParseGabbyDialogueScript(string assetPath, IDialogueBuilder builder)
        {
            try
            {
                using (StreamReader stream = new StreamReader(assetPath))
                {
                    ParserState state = new ParserState();
                    state.stream = stream;
                    state.builder = builder;
                    state.assetPath = assetPath;

                    // TODO parse metadata

                    while (ReadLine(state) != null)
                    {
                        ParseLine(state);
                    }

                    if (state.isParsingDialogue)
                    {
                        state.builder.OnDialogueDefinitionEnd();
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogError($"Could not parse gabby dialogue script: {assetPath}");
                Debug.LogError(e.Message);
                return false;
            }

            return true;
        }

        private static string ReadLine(ParserState state)
        {
            if (state.blockReadLine)
            {
                state.blockReadLine = false;
                return state.rawLine;
            }

            state.rawLine = state.stream.ReadLine();
            if (state.rawLine != null)
            {
                state.line = state.rawLine.Trim();
                state.lineNumber++;
                state.indentLevel = GetIndentLevel(state.rawLine);
            }
            return state.rawLine;
        }

        private static void ParseLine(ParserState state)
        {
            if (state.line.Length == 0 || state.line.StartsWith("//"))
            {
                return;
            }

            if ((lineHandlerMap.ContainsKey(state.line[0]) && lineHandlerMap[state.line[0]](state))
                || ParseKeyword(state))
            {
                return;
            }

            Debug.LogError($"Error while parsing Gabby dialogue script: {state.assetPath}\nUnrecognized symbols on line ${state.lineNumber}: {state.line}\nMake sure the line begins with a symbol recognized by Gabby.");
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
                // End the current dialogue before starting a new one
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
                                             + @"\)\s+(?<t>" + regexNonCommentCharacterSequence + @")"
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
                                              + @"(?<t>" + regexNonCommentCharacterSequence + @")"
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
                                              + @"(?<t>" + regexNonCommentCharacterSequence + @")"
                                              + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateContinuedDialogue);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            return state.builder.OnContinuedDialogue(state.lastCharacter, text);
        }

        private static bool ParseSingleOption(ParserState state)
        {
            string validateOption = @"^\s*\:\s+"
                                  + @"(?<t>" + regexNonCommentCharacterSequence + @")"
                                  + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateOption);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            state.builder.OnOption(text);
            return true;
        }

        private static bool ParseOptionBlock(ParserState state)
        {
            int optionBlockIndentLevel = state.indentLevel;

            state.builder.OnOptionsBegin();
            ParseSingleOption(state);

            // Parse the entire option block
            while (ReadLine(state) != null)
            {
                if (state.line.Length == 0 || state.line.StartsWith("//"))
                {
                    continue;
                }

                bool isOption = state.line.StartsWith(":");

                // Check if the block is closed
                if (state.indentLevel < optionBlockIndentLevel
                    || (state.indentLevel == optionBlockIndentLevel && !isOption))
                {
                    // The line belongs to the parent block
                    // Break and handle it in the parent function
                    state.blockReadLine = true;
                    break;
                }

                Func<ParserState, bool> handler;
                if (isOption)
                {
                    if (state.indentLevel > optionBlockIndentLevel)
                    {
                        // Part of a nested option block
                        // Handle it recursively but don't end this block
                        ParseOptionBlock(state);
                        continue;
                    }
                    else
                    {
                        // Part of the current option block
                        handler = ParseSingleOption;
                    }
                }
                else
                {
                    // Regular line inside the current options block
                    handler = lineHandlerMap.ContainsKey(state.line[0]) ? lineHandlerMap[state.line[0]] : ParseKeyword;
                }

                // Handle the current line as part of the current options block
                if (!handler(state))
                {
                    LogParserError(state);
                }
            }

            state.builder.OnOptionsEnd();

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
                                  + @"(?<t>" + regexNonCommentCharacterSequence + @")"
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
                state.builder.OnEnd();
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

        private static void LogParserError(ParserState state)
        {
            Debug.LogError($"Error while parsing Gabby dialogue script: {state.assetPath}\nUnrecognized symbols on line ${state.lineNumber}: {state.line}\nMake sure the line begins with a symbol recognized by Gabby.");
        }
    }
}
