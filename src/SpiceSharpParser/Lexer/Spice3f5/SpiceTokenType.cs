﻿namespace SpiceSharpParser.Lexer.Spice3f5
{
    /// <summary>
    /// Types of terminals in spice grammar
    /// </summary>
    public enum SpiceTokenType
    {
        /// <summary>
        /// End of file token
        /// </summary>
        EOF = -1,

        /// <summary>
        /// * character token
        /// </summary>
        ASTERIKS = 1,

        /// <summary>
        /// - character token
        /// </summary>
        MINUS = 2,

        /// <summary>
        /// . character token
        /// </summary>
        DOT = 3,

        /// <summary>
        /// , character token
        /// </summary>
        COMMA = 4,

        /// <summary>
        /// Delimeter characters token
        /// </summary>
        DELIMITER = 5,

        /// <summary>
        /// new line token
        /// </summary>
        NEWLINE = 6,

        /// <summary>
        /// .ENDS token
        /// </summary>
        ENDS = 7,

        /// <summary>
        /// .END token
        /// </summary>
        END = 8,

        /// <summary>
        /// value token
        /// </summary>
        VALUE = 9,

        /// <summary>
        /// token with comment line
        /// </summary>
        COMMENT = 10,

        /// <summary>
        /// expression token
        /// </summary>
        EXPRESSION = 11,

        /// <summary>
        /// reference token
        /// </summary>
        REFERENCE = 12,

        /// <summary>
        /// word token
        /// </summary>
        WORD = 13,

        /// <summary>
        /// whitespace token
        /// </summary>
        WHITESPACE = 14,

        /// <summary>
        /// + character token
        /// </summary>
        PLUS = 15,

        /// <summary>
        /// identifier token
        /// </summary>
        IDENTIFIER = 16,

        /// <summary>
        /// string token
        /// </summary>
        STRING = 17,

        /// <summary>
        /// title token
        /// </summary>
        TITLE = 18,

        /// <summary>
        /// continue token
        /// </summary>
        CONTINUE = 19,

        /// <summary>
        /// = character
        /// </summary>
        EQUAL = 20,

        /// <summary>
        /// ; style comment
        /// </summary>
        COMMENT_PSPICE = 21,

        /// <summary>
        /// $ style comment
        /// </summary>
        COMMENT_HSPICE = 22,

        /// <summary>
        /// .ENDL token
        /// </summary>
        ENDL_HSPICE = 23
    }
}
