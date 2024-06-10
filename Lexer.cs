using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleCompiler;

internal class Lexer
{
    private string Source { get; set; }
    
    private int currentPosition;
    private int startingPosition;
    
    public char currentChar;
    public char lastChar;

    public Lexer(string source)
    {
        source += '\n';
        Source = source;
        
        currentPosition = -1;
        NextCharacter();
    }

    public void NextCharacter()
    {
        currentPosition++;
        if (currentPosition >= Source.Length)
            currentChar = '\0';
        else
            currentChar = Source[currentPosition];
    }

    public char Peek()
    {
        if (currentPosition + 1 >= Source.Length)
            return '\0';

        return Source[currentPosition + 1];
    }

    public Token? GetToken()
    {
        SkipWhitespace();
        SkipComments();

        Token token;

        //plus
        if (currentChar == '+')
        {
            token = new(currentChar, Token.TokenType.PLUS);
        }

        //minus
        else if (currentChar == '-')
        {
            token = new(currentChar, Token.TokenType.MINUS);
        }

        //asterisk
        else if (currentChar == '*')
        {
            token = new(currentChar, Token.TokenType.ASTERISK);
        }

        //slash
        else if (currentChar == '/')
        {
            token = new(currentChar, Token.TokenType.SLASH);
        }

        //assign or equal
        else if (currentChar == '=')
        {
            if (Peek() == '=')
            {
                lastChar = currentChar;
                NextCharacter();
                token = new(lastChar.ToString() + currentChar.ToString(), Token.TokenType.EQEQ);
            }
            else
                token = new(currentChar, Token.TokenType.EQ);
        }

        // greater and greater or equal
        else if (currentChar == '>')
        {
            if (Peek() == '=')
            {
                lastChar = currentChar;
                NextCharacter();
                token = new(lastChar.ToString() + currentChar.ToString(), Token.TokenType.GTEQ);
            }
            else
                token = new(currentChar, Token.TokenType.GT);
        }

        //less and less or equal
        else if (currentChar == '<')
        {
            if (Peek() == '=')
            {
                lastChar = currentChar;
                NextCharacter();
                token = new(lastChar.ToString() + currentChar.ToString(), Token.TokenType.LTEQ);
            }
            else
                token = new(currentChar, Token.TokenType.LT);
        }

        //not equal
        else if (currentChar == '!')
        {
            if (Peek() == '=')
            {
                lastChar = currentChar;
                NextCharacter();
                token = new(lastChar.ToString() + currentChar.ToString(), Token.TokenType.NOTEQ);
            }
            else
            {
                token = new(lastChar.ToString() + currentChar.ToString());
                Abort("Expected !=, got !" + Peek());
            }
        }

        //strings
        else if (currentChar =='\"')
        {
            NextCharacter();
            startingPosition = currentPosition;
            
            while(currentChar != '\"')
            {
                if (currentChar == '\r' || currentChar == '\n' || currentChar == '\t' || currentChar == '\\' || currentChar == '%')
                    Abort("Illegal character in string");
                NextCharacter();
            }
            token = new(Source[startingPosition..currentPosition], Token.TokenType.STRING); //range operator used
        }

        //numbers
        else if (Char.IsNumber(currentChar))
        {
            startingPosition = currentPosition;
            while (Char.IsNumber(Peek()))
                NextCharacter();
            if (Peek() == '.')
            {
                NextCharacter();
                if (!Char.IsNumber(Peek()))
                    Abort("Illegal Character in NUMBER");
                while (Char.IsNumber(Peek()))
                    NextCharacter();
            }
            token = new(Source[startingPosition..currentPosition], Token.TokenType.NUMBER);
        }

        else if( Char.IsLetter(currentChar) )
        {
            startingPosition = currentPosition;
            while (Char.IsLetterOrDigit(Peek()))
                NextCharacter();
            
            string tokenText = Source[startingPosition..(currentPosition + 1)];
            string? keyWord = CheckIfKeyword(tokenText);

            if (String.IsNullOrEmpty(keyWord))
                token = new(tokenText, Token.TokenType.IDENT);
            else
            {
                Enum.TryParse(keyWord, false, out Token.TokenType type);
                token = new(tokenText, type);
            }
        }

        //new line
        else if (currentChar == '\n')
        {
            token = new(currentChar, Token.TokenType.NEWLINE);
        }


        //end of file
        else if (currentChar == '\0')
        {
            token = new(currentChar, Token.TokenType.EOF);
        }

        else
        {
            token = new(currentChar);
            Abort("Unknown Token: " + currentChar);
        }
        
        NextCharacter();
        return token;
    }

    private static string? CheckIfKeyword(string s)
    {
        foreach (var tokenType in Enum.GetValues(typeof(Token.TokenType)))
        {
            if (String.Equals(tokenType.ToString(), s) && (int)tokenType >= 100 && (int)tokenType < 200)
                return tokenType.ToString();
        }
        return String.Empty;
    }

    private void SkipComments()
    {
        if( currentChar == '#' )
        {
            while (currentChar != '\n')
                NextCharacter();
        }
    }

    private void SkipWhitespace()
    {
        while (currentChar == ' ' || currentChar == '\t' || currentChar == '\r')
            NextCharacter();
    }


    private static void Abort(string message)
    {
        Console.WriteLine("Lexing Error." + message);
        Environment.Exit(1);
    }

    public class Token
    {
        public char? TokenChar { get; set; }
        public string? TokenText { get; set; }
        public TokenType TokenKind { get; set; }


        public Token(char tokenText, TokenType tokenKind) {
            TokenChar = tokenText;
            TokenKind = tokenKind;
        }
        
        public Token(string tokenText, TokenType tokenKind)
        {
            TokenText = tokenText;
            TokenKind = tokenKind;
        }

        public Token(char tokenText) { TokenChar = tokenText; }
        public Token(string tokenText) { TokenText = tokenText; }

        public enum TokenType
        {
            EOF = -1,
            NEWLINE = 0,
            NUMBER = 1,
            IDENT = 2,
            STRING = 3,
            //KEYWORDS
            LABEL = 101,
            GOTO = 102,
            PRINT = 103,
            INPUT = 104,
            LET = 105,
            IF = 106,
            THEN = 107,
            ENDIF = 108,
            WHILE = 109,
            REPEAT = 110,
            ENDWHILE = 111,
            //OPERATORS
            EQ = 201,
            PLUS = 202,
            MINUS = 203,
            ASTERISK = 204,
            SLASH = 205,
            EQEQ = 206,
            NOTEQ = 207,
            LT = 208,
            LTEQ = 209,
            GT = 210,
            GTEQ =211
        }
    }
}
