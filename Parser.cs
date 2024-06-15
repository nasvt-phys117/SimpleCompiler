using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static SimpleCompiler.Lexer;
using static SimpleCompiler.Emitter;


namespace SimpleCompiler
{
    internal class Parser
    {
        private Token? currentToken;
        private Token? peekToken;

        private readonly Lexer _Lexer;
        private readonly Emitter _emitter;

        private HashSet<string> symbols = [];
        private HashSet<string> labelsDeclared = [];
        private HashSet<string> labelsGotoed = [];

        public Parser(Lexer lexer, Emitter emitter)
        {
            _Lexer    = lexer;
            _emitter  = emitter;

            currentToken    = null;
            peekToken       = null;
            NextToken();
            NextToken();
        }

        bool CheckToken(Lexer.Token.TokenType tokenType)
        {
            return tokenType == currentToken?.TokenKind;
        }

        //bool CheckPeek(Lexer.Token.TokenType tokenType) { return tokenType == peekToken?.TokenKind; }

        void Match(Lexer.Token.TokenType tokenType)
        {
            if(!CheckToken(tokenType))
                Abort($"Expected {tokenType.GetType().Name}, got {currentToken?.TokenKind} ");
            NextToken();
        }

        void NextToken()
        {
            currentToken = peekToken;
            peekToken = _Lexer.GetToken();
        }

        static void Abort(string message)
        {
            Console.WriteLine("Error." + message);
            Environment.Exit(1);
        }

        public void Program()
        {
            //Console.WriteLine("PROGRAM");

            _emitter.HeaderLine("#include<stdio.h>");
            _emitter.HeaderLine("int main(void){");


            while (CheckToken(Token.TokenType.NEWLINE))
                NextToken();

            while (!CheckToken(Token.TokenType.EOF))
            {
                Statement();
            }

            _emitter.EmitLine("return 0;");
            _emitter.EmitLine("}");

            foreach (var label in labelsGotoed)
            {
                if (!labelsDeclared.Contains(label))
                    Abort($"Attempting to GOTO an undeclared label: {label}");
            }
        }

        void NewLine()
        {
            //Console.WriteLine("NEWLINE");
            Match(Token.TokenType.NEWLINE);
            while (CheckToken(Token.TokenType.NEWLINE))
                NextToken();
        }

        void Statement()
        {
            //PRINT
            if (CheckToken(Token.TokenType.PRINT))
            {
                //Console.WriteLine("STATEMENT-PRINT");
                NextToken();

                if (CheckToken(Token.TokenType.STRING))
                {
                    _emitter.EmitLine($"printf(\"{currentToken.TokenText}\\n\");");
                    NextToken();
                }
                else
                {
                    _emitter.Emit("printf(\"%" + ".2f\\n\",(float)(");
                    ProgramExpression();
                    _emitter.EmitLine("));");
                }
            }

            //IF
            else if (CheckToken(Token.TokenType.IF))
            {
                //Console.WriteLine("STATEMENT-IF");
                NextToken();
                _emitter.Emit("if(");
                ProgramComparison();

                Match(Token.TokenType.THEN);
                NewLine();
                _emitter.EmitLine("){");

                while (!CheckToken(Token.TokenType.ENDIF))
                    Statement();

                Match(Token.TokenType.ENDIF);
                _emitter.EmitLine("}");
            }
            
            //WHILE
            else if (CheckToken(Token.TokenType.WHILE))
            {
                //Console.WriteLine("STATEMENT-WHILE");
                NextToken();
                _emitter.Emit("while(");
                ProgramComparison();

                Match(Token.TokenType.REPEAT);
                NewLine();
                _emitter.EmitLine("){");

                while(!CheckToken(Token.TokenType.ENDWHILE))
                    Statement();
                
                Match(Token.TokenType.ENDWHILE);
                _emitter.EmitLine("}");
            }

            //LABEL
            else if (CheckToken(Token.TokenType.LABEL))
            {
                //Console.WriteLine("STATEMENT-LABEL");
                NextToken();

                //Labels
                if (labelsDeclared.Contains(currentToken.TokenText))
                    Abort($"Label already exists: {currentToken.TokenText}");
                labelsDeclared.Add(currentToken.TokenText);

                _emitter.EmitLine($"{currentToken.TokenText}:");
                Match(Token.TokenType.IDENT);
            }

            //GOTO
            else if (CheckToken(Token.TokenType.GOTO))
            {
                //Console.WriteLine("STATEMENT-GOTO");
                NextToken();
                labelsGotoed.Add(currentToken.TokenText);

                _emitter.EmitLine($"goto {currentToken.TokenText};");
                Match(Token.TokenType.IDENT);
            }

            //LET
            else if (CheckToken(Token.TokenType.LET))
            {
                //Console.WriteLine("STATEMENT-LET");
                NextToken();

                if (!symbols.Contains(currentToken.TokenText))
                {
                    symbols.Add(currentToken.TokenText);
                    _emitter.HeaderLine($"float {currentToken.TokenText};");
                }
                _emitter.Emit($"{currentToken.TokenText} = ");
                Match(Token.TokenType.IDENT);
                Match(Token.TokenType.EQ);

                ProgramExpression();
                _emitter.EmitLine(";");
            }

            //INPUT (bad implementation)
            else if (CheckToken(Token.TokenType.INPUT))
            {
                //Console.WriteLine("STATEMENT-INPUT");
                NextToken();

                if (!symbols.Contains(currentToken.TokenText))
                {
                    symbols.Add(currentToken.TokenText);
                    _emitter.HeaderLine($"float {currentToken.TokenText};");
                }
                _emitter.EmitLine("if(0==scanf(\"%"+"f\",&" + currentToken.TokenText+")) {");
                _emitter.EmitLine($"{currentToken.TokenText} = 0;");
                _emitter.Emit($"scanf(\"%");
                _emitter.EmitLine("*s\");");
                _emitter.EmitLine("}");

                Match(Token.TokenType.IDENT);
            }
            else
                Abort($"Invalid statement at {currentToken?.TokenText} ({currentToken?.TokenKind})");
            NewLine();
        }

        private void ProgramComparison()
        {
            //Console.WriteLine("COMPARISON");

            ProgramExpression();

            if (IsComparisonOperator())
            {
                if (currentToken.TokenText is not null)
                    _emitter.Emit(currentToken.TokenText);
                else
                {
                    _emitter.Emit(currentToken.TokenChar.ToString());
                }
                NextToken();
                ProgramExpression();
            }
            else
                Abort($"Expected comparison operator at: {currentToken?.TokenText}");

            while (IsComparisonOperator())
            {
                if (currentToken.TokenText is not null)
                    _emitter.Emit(currentToken.TokenText);
                else
                {
                    _emitter.Emit(currentToken.TokenChar.ToString());
                }
                NextToken();
                ProgramExpression();
            }
        }

        private bool IsComparisonOperator()
        {
            if (CheckToken(Token.TokenType.GT) || CheckToken(Token.TokenType.GTEQ) || CheckToken(Token.TokenType.LT) || CheckToken(Token.TokenType.LTEQ) || CheckToken(Token.TokenType.EQEQ) || CheckToken(Token.TokenType.NOTEQ))
                return true;
            else
                return false;
        }

        private void ProgramExpression()
        {
            //Console.WriteLine("EXPRESSION");
            ProgramTerm();

            while (CheckToken(Token.TokenType.PLUS) || CheckToken(Token.TokenType.MINUS))
            {
                _emitter.Emit(currentToken.TokenChar.ToString());
                NextToken();
                ProgramTerm();
            }
        }

        private void ProgramTerm()
        {
            //Console.WriteLine("TERM");

            ProgramUnary();
            
            while ( CheckToken(Token.TokenType.ASTERISK) || CheckToken(Token.TokenType.SLASH) )
            {
                _emitter.Emit(currentToken.TokenChar.ToString());
                NextToken();
                ProgramUnary();
            }
        }

        private void ProgramUnary()
        {
            Console.WriteLine("UNARY");

            if (CheckToken(Token.TokenType.PLUS) || CheckToken(Token.TokenType.MINUS))
            {
                _emitter.Emit(currentToken.TokenChar.ToString());
                NextToken();
            }
            ProgramPrimary();
        }

        private void ProgramPrimary()
        {
            Console.WriteLine($"PRIMARY ({currentToken?.TokenText})");

            if (CheckToken(Token.TokenType.NUMBER))
            {
                _emitter.Emit(currentToken.TokenText);
                NextToken();
            }

            else if (CheckToken(Token.TokenType.IDENT))
            {
                if (!symbols.Contains(currentToken.TokenText))
                    Abort($"Referencing variable before assignment: {currentToken.TokenText}");
                _emitter.Emit(currentToken.TokenText);
                NextToken();
            }
            else
                Abort($"Unexpected token at {currentToken?.TokenText}");
        }
    }
}
