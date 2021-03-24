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

        // TODO use regex? Would be nicer for multi-character line designators, or overloaded designators
        private static Dictionary<char, Func<ParserState, bool>> lineHandlerMap = new Dictionary<char, Func<ParserState, bool>> {
            {'[', DialogParser.ParseDialogueDefinition},
            {'(', DialogParser.ParseCharacterDialogue},
            {'-', DialogParser.ParseSequentialDialogue},
            {'+', DialogParser.ParseContinuedDialogue},
            {'*', DialogParser.ParseNarratedDialogue},
            {':', DialogParser.ParseOptionBlock},
            {'>', DialogParser.ParseAction},
            {'{', DialogParser.ParseProperties}
        };

        // TODO set regex cache size
        private static string regexValidName = @"[\w-]+(?:\s+[\w-]+)*";
        // private static string regexEndsWithCommentOrNewline = @"\s*(?://(?:.*)|$)";
        private static string regexEndsWithCommentOrNewline = @"\s*(?:\/{2,}.*)*$";
        // private static string regexNonCommentCharacterSequence = @"[^/]*(/?[^/]*)*";
        private static string regexNonCommentCharacterSequence = @"[^\/\n]*(?:\/[^\/\n]+)*";
        private static string regexQuotedString = @"""(?:[^""\\]|\\.)*""";
        private static string regexUnquotedString = @"[\w\-\.]+(?:\s+[\w\-\.]+)*";
        private static string regexQuotedOrUnquotedString = @"(?:" + regexQuotedString + @"|" + regexUnquotedString + @")"; // Quoted strings contain any characters, unquoted strings have restricted characters
        private static string regexCommaSeparatedStringList = @"(?:(?<p>" + regexQuotedOrUnquotedString + @")(?:\,\s)?)*";

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
            string validateCharacterDialogue = @"^\s*\(" // Line designator / open parentheses
                                             + @"(?<char>" + regexValidName + @")" // Character name
                                             + @"(?:\s*,\s*(?<tag>[^,\s]+(?:\s+[^,\s]+)*))*" // Inline tags
                                             + @"\)\s*" // Close parentheses
                                             + @"(?<text>" + regexNonCommentCharacterSequence + @")" // Text
                                             + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateCharacterDialogue);
            if (!match.Success)
            {
                return false;
            }

            string characterName = match.Groups["char"].Value;
            string text = match.Groups["text"].Value;

            var tagCaptures = match.Groups["tag"].Captures;
            if (tagCaptures.Count > 0)
            {
                // TODO add support for named tag parsing
                // TODO move tag parsing into top level function, reused by tag lines
                Dictionary<string, string> inlineTags = new Dictionary<string, string>();
                foreach (Capture c in tagCaptures)
                {
                    string tag = c.Value;
                    inlineTags.Add(tag, "");
                }
                state.builder.SetNextLineTags(inlineTags);
            }

            state.lastCharacter = characterName;
            return state.builder.OnDialogueLine(characterName, text);
        }

        private static bool ParseSequentialDialogue(ParserState state)
        {
            string validateSequentialDialogue = @"^\s*\-\s*" // Line designator
                                              + @"(?<t>" + regexNonCommentCharacterSequence + @")" // Text
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
            string validateContinuedDialogue = @"^\s*\+\s*" // Line designator
                                              + @"(?<t>" + regexNonCommentCharacterSequence + @")" // Text
                                              + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateContinuedDialogue);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            return state.builder.OnContinuedDialogue(state.lastCharacter, text);
        }

        private static bool ParseNarratedDialogue(ParserState state)
        {
            string validateNarratedDialogue = @"^\s*\*\s*" // Line designator
                                              + @"(?<t>" + regexNonCommentCharacterSequence + @")" // Text
                                              + regexEndsWithCommentOrNewline;

            Match match = Regex.Match(state.line, validateNarratedDialogue);
            if (!match.Success)
            {
                return false;
            }

            string text = match.Groups["t"].Value;
            return state.builder.OnNarratedDialogue(state.lastCharacter, text);
        }

        private static bool ParseSingleOption(ParserState state)
        {
            string validateOption = @"^\s*\:\s*" // Line designator
                                  + @"(?<t>" + regexNonCommentCharacterSequence + @")" // Text
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

            string validateJump = @"^\s*\>\>\s*" // Line designator
                                  + @"(?<c>" + regexValidName + @")" // Character
                                  + @"\s*\.\s*" // Separator
                                  + @"(?<d>" + regexValidName + @")" // Dialogue
                                  + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateJump);
            if (match.Success)
            {
                string characterName = match.Groups["c"].Value;
                string dialogueName = match.Groups["d"].Value;
                state.builder.OnJump(characterName, dialogueName);
                return true;
            }

            string validateAction = @"^\s*\>\s*" // Line designator
                                  + @"(?<n>" + regexValidName + @")" // Action name
                                  + @"\s*\(" + regexCommaSeparatedStringList + @"\)" // Parameter list
                                  + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateAction);
            if (match.Success)
            {
                string actionName = match.Groups["n"].Value;
                List<string> actionParameters = new List<string>();
                foreach (Capture c in match.Groups["p"].Captures)
                {
                    string value = c.Value;
                    // Remove quotes from quoted string parameters
                    Match quotedStringMatch = Regex.Match(value, regexQuotedString);
                    if (quotedStringMatch.Success)
                    {   
                        value = value.Substring(1, value.Length - 2);
                    }
                    actionParameters.Add(value);
                }
                state.builder.OnAction(actionName, actionParameters);
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

            string validateEnd = @"^\s*end" + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateEnd);
            if (match.Success)
            {
                state.builder.OnEnd();
                return true;
            }

            // TODO add restriction that metadata must appear at the top of the file
            string validateVersion = @"^\s*gabby\s+"
                                   + @"(?<v>\d+(?:.\d+)*)"
                                   + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateVersion);
            if (match.Success)
            {
                string version = match.Groups["v"].Value;
                state.builder.SetVersion(version);
                return true;
            }

            string validateLanguage = @"^\s*language\s+"
                                    + @"(?<l>[\w\-]+)"
                                    + regexEndsWithCommentOrNewline;

            match = Regex.Match(state.line, validateLanguage);
            if (match.Success)
            {
                string language = match.Groups["l"].Value;
                state.builder.SetLanguage(language);
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
