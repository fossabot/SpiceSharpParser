﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    /// <summary>
    /// A lexer for SPICE netlists.
    /// </summary>
    public class SpiceLexer
    {
        private readonly SpiceLexerSettings _options;
        private LexerGrammar<SpiceLexerState> _grammar;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceLexer"/> class.
        /// </summary>
        /// <param name="options">options for lexer.</param>
        public SpiceLexer(SpiceLexerSettings options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            BuildGrammar();
        }

        /// <summary>
        /// Gets tokens for SPICE netlist.
        /// </summary>
        /// <param name="netlistText">A string with SPICE netlist.</param>
        /// <returns>
        /// An enumerable of tokens.
        /// </returns>
        public IEnumerable<SpiceToken> GetTokens(string netlistText)
        {
            if (netlistText == null)
            {
                throw new ArgumentNullException(nameof(netlistText));
            }

            var state = new SpiceLexerState();
            var lexer = new Lexer<SpiceLexerState>(_grammar);

            foreach (var token in lexer.GetTokens(netlistText, state))
            {
                yield return new SpiceToken((SpiceTokenType)token.Type, token.Lexem, state.LineNumber, state.StartColumnIndex,null);
            }
        }

        /// <summary>
        /// Builds SPICE lexer grammar.
        /// </summary>
        private void BuildGrammar()
        {
            var builder = new LexerGrammarBuilder<SpiceLexerState>();
            builder.AddRegexRule(new LexerInternalRule("LETTER", "[a-zA-Zµ]"));
            builder.AddRegexRule(new LexerInternalRule("CHARACTER", @"[a-zA-Z0-9\-\+§]"));
            builder.AddRegexRule(new LexerInternalRule("DIGIT", "[0-9]"));
            builder.AddRegexRule(new LexerInternalRule("SPECIAL", @"[\/\\_\.:%!\#\-;\<\>\^\*\[\]]"));
            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WHITESPACE,
                "A whitespace characters that will be ignored",
                @"[\t 	]+",
                (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.IgnoreToken));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.CONTINUATION_CURRENT_LINE,
                "A current line continuation character that is ignored",
                @"(\\\\)(\r\n|\r|\n)",
                (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.IgnoreToken,
                topRule: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.CONTINUATION_NEXT_LINE,
                "A next line continuation character that is ignored",
                @"((\r\n|\r|\n)[\t 	]*)+\+",
                (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.IgnoreToken,
                useDecisionProvider: (SpiceLexerState state, string lexem) =>
                {
                    if (state.LineNumber == 1 && _options.HasTitle)
                    {
                        return LexerRuleUseDecision.Next;
                    }

                    return LexerRuleUseDecision.Use;
                },
                topRule: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.TITLE,
                "The title - first line of SPICE token",
                @"[^\r\n]+",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.LineNumber == 1 && _options.HasTitle)
                    {
                        return LexerRuleUseDecision.Use;
                    }

                    return LexerRuleUseDecision.Next;
                }));

            builder.AddDynamicRule(new LexerDynamicRule(
                (int)SpiceTokenType.EXPRESSION_BRACKET,
                "A mathematical (also nested) expression in brackets",
                "{",
                (string textToLex) =>
                    {
                        int openBracketCount = 1;
                        int i = 0;
                        for (i = 1; i < textToLex.Length && openBracketCount > 0; i++)
                        {
                            if (textToLex[i] == '}')
                            {
                                openBracketCount--;
                            }

                            if (textToLex[i] == '{')
                            {
                                openBracketCount++;
                            }
                        }

                        if (openBracketCount == 0)
                        {
                            // TODO this is a hack, please refactor me
                            var text = textToLex.Substring(0, i);
                            var replaced = Regex.Replace(text, @"((\r\n|\r|\n)[\t 	]*)+\+", string.Empty);
                            return new Tuple<string, int>(replaced, text.Length);
                        }

                        throw new LexerException("Not matched brackets for expression");
                    }));

            builder.AddDynamicRule(new LexerDynamicRule(
                (int)SpiceTokenType.BOOLEAN_EXPRESSION,
                "A mathematical (also nested) expression in brackets",
                "(",
                (string textToLex) =>
                {
                    int openBracketCount = 1;
                    var i = 0;
                    for (i = 1; i < textToLex.Length && openBracketCount > 0; i++)
                    {
                        if (textToLex[i] == ')')
                        {
                            openBracketCount--;
                        }

                        if (textToLex[i] == '(')
                        {
                            openBracketCount++;
                        }
                    }

                    if (openBracketCount == 0)
                    {
                        var text = textToLex.Substring(0, i);
                        var replaced = Regex.Replace(text, @"((\r\n|\r|\n)[\t 	]*)+\+", string.Empty);
                        return new Tuple<string, int>(replaced, text.Length);
                    }

                    throw new LexerException("Not matched brackets for expression");
                }));

            builder.AddRegexRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DOT,
                    "A dot character",
                    "\\."));

            builder.AddRegexRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.COMMA,
                    "A comma character",
                    ","));

            builder.AddRegexRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DELIMITER,
                    "A delimiter character",
                    @"(\(|\)|\|)",
                    (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.ReturnToken,
                    (SpiceLexerState state, string lexem) =>
                    {
                        if (state.PreviousReturnedTokenType == (int)SpiceTokenType.IF || (state.PreviousReturnedTokenType == (int)SpiceTokenType.ELSE_IF))
                        {
                            return LexerRuleUseDecision.Next;
                        }

                        return LexerRuleUseDecision.Use;
                    }));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.COM_START,
                "An block comment start",
                @"#COM",
                (SpiceLexerState state, string lexem) =>
                {
                    state.InCommentBlock = true;
                    return LexerRuleReturnDecision.IgnoreToken;
                },
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.NewLine || state.PreviousReturnedTokenType == 0)
                    {
                        return LexerRuleUseDecision.Use;
                    }

                    return LexerRuleUseDecision.Next;
                },
                ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.COM_END,
              "An block comment end",
              "#ENDCOM",
              (SpiceLexerState state, string lexem) =>
              {
                  state.InCommentBlock = false;
                  return LexerRuleReturnDecision.IgnoreToken;
              },
              (SpiceLexerState state, string lexem) =>
              {
                  if (state.InCommentBlock)
                  {
                      return LexerRuleUseDecision.Use;
                  }

                  return LexerRuleUseDecision.Next;
              },
              ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
               (int)SpiceTokenType.COM_CONTENT,
               "An block comment content",
               @".*",
               (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.IgnoreToken,
               (SpiceLexerState state, string lexem) =>
               {
                   if (state.InCommentBlock)
                   {
                       return LexerRuleUseDecision.Use;
                   }

                   return LexerRuleUseDecision.Next;
               }));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.EQUAL,
              "An equal character",
              @"="));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.NEWLINE,
                "A new line characters",
                @"(\r\n|\n|\r)",
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.InCommentBlock)
                    {
                        return LexerRuleReturnDecision.IgnoreToken;
                    }

                    return LexerRuleReturnDecision.ReturnToken;
                }));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.ENDS,
                ".ENDS keyword",
                "\\.ENDS",
                ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.END,
                ".END keyword",
                "\\.END",
                ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
               (int)SpiceTokenType.ENDL,
               ".ENDL keyword",
               "\\.ENDL",
               ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.IF,
              ".IF keyword",
              "\\.IF",
              ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.ENDIF,
              ".ENDIF keyword",
              "\\.ENDIF",
              ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.ELSE,
              ".ELSE keyword",
              "\\.ELSE",
              ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
              (int)SpiceTokenType.ELSE_IF,
              ".ELSEIF keyword",
              "\\.ELSEIF",
              ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.MODEL,
                ".MODEL keyword",
                "\\.MODEL",
                ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.VALUE,
                "A value with dot separator",
                @"([+-]?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+)?[tgmkunpf]?(<LETTER>)*)",
                null,
                (SpiceLexerState state, string lexem) => LexerRuleUseDecision.Use,
                ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.PERCENT,
                "A percent value with dot separator",
                @"([+-]?((<DIGIT>)+(\.(<DIGIT>)*)?|\.(<DIGIT>)+)(e(\+|-)?(<DIGIT>)+)?[tgmkunpf]?(<LETTER>)*)%",
                null,
                (SpiceLexerState state, string lexem) => LexerRuleUseDecision.Use,
                ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
             (int)SpiceTokenType.COMMENT_HSPICE,
             "A comment - HSpice style",
             @"\$[^\r\n]*",
             (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.IgnoreToken));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
             (int)SpiceTokenType.COMMENT_PSPICE,
             "A comment - PSpice style",
             @";[^\r\n]*",
             (SpiceLexerState state, string lexem) => LexerRuleReturnDecision.IgnoreToken));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.COMMENT,
                "A full line comment",
                @"\*[^\r\n]*",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (state.LineNumber == 1 && _options.HasTitle)
                    {
                        return LexerRuleUseDecision.Next;
                    }

                    if (state.NewLine)
                    {
                        return LexerRuleUseDecision.Use;
                    }

                    return LexerRuleUseDecision.Next;
                },
                ignoreCase: true));

            builder.AddRegexRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.DOUBLE_QUOTED_STRING,
                    "A string with double quotation marks",
                    "\"(?:[^\"\\\\]|\\\\.)*\""));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
             (int)SpiceTokenType.EXPRESSION_SINGLE_QUOTES,
             "A mathematical expression in single quotes",
             @"'[^']*'",
             null,
             (SpiceLexerState state, string lexem) =>
             {
                 if (state.PreviousReturnedTokenType == (int)SpiceTokenType.EQUAL)
                 {
                     return LexerRuleUseDecision.Use;
                 }

                 return LexerRuleUseDecision.Next;
             },
             ignoreCase: !_options.IsDotStatementNameCaseSensitive));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.EXPRESSION_BRACKET,
                "A mathematical expression in brackets",
                @"{[^{}\r\n]*}",
                ignoreCase: true));

            builder.AddRegexRule(
              new LexerTokenRule<SpiceLexerState>(
                  (int)SpiceTokenType.SINGLE_QUOTED_STRING,
                  "A string with single quotation marks",
                  @"'[^']*'"));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.REFERENCE,
                "A reference",
                "@(<CHARACTER>(<CHARACTER>|<SPECIAL>)*)",
                null,
                (SpiceLexerState state, string lexem) =>
                {
                    if (lexem.EndsWith("\\", StringComparison.Ordinal) && state.BeforeLineBreak)
                    {
                        return LexerRuleUseDecision.Next;
                    }

                    return LexerRuleUseDecision.Use;
                },
                ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WORD,
                "A word",
                "<LETTER>(<CHARACTER>|<SPECIAL>)*",
                null,
                (SpiceLexerState state, string lexem) => LexerRuleUseDecision.Use,
                ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.WORD,
                "A relative path",
                @"(\.\.\\|\.\.\/|\.\/|\.\\)(<CHARACTER>|<SPECIAL>)*",
                null,
                (SpiceLexerState state, string lexem) => LexerRuleUseDecision.Use,
                ignoreCase: true));

            builder.AddRegexRule(
                new LexerTokenRule<SpiceLexerState>(
                    (int)SpiceTokenType.IDENTIFIER,
                    "An identifier",
                    @"((<CHARACTER>|_|\*)(<CHARACTER>|<SPECIAL>)*)",
                    null,
                    (SpiceLexerState state, string lexem) => LexerRuleUseDecision.Use,
                    ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.REFERENCE,
                "A reference",
                "@(<CHARACTER>(<CHARACTER>|<SPECIAL>)+)",
                ignoreCase: true));

            builder.AddRegexRule(new LexerTokenRule<SpiceLexerState>(
                (int)SpiceTokenType.ASTERIKS,
                "An asterisk character",
                "\\*"));

            _grammar = builder.GetGrammar();
        }
    }
}