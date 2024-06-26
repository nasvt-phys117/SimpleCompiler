﻿namespace SimpleCompiler
{
    internal class Program
    {
        static void Main(string[] args)
        //static void Main()
        {

            //SIMPLE TESTING
            //string sourceCode = "IF 123 = 123 DO SOMETHING";

            //Lexer lexer = new(sourceCode);

            //while (lexer.Peek() != '\0')
            //{
            //    Console.WriteLine(lexer.currentChar);
            //    lexer.NextCharacter();
            //}


            //TOKEN TESTING

            //string sourceCode2 = "PRINT \"These are the first 10 Natural Numbers\" \n" +
            //    "LET a = 1 \n" +
            //    "LET b = 10 \n" +
            //    "WHILE a <= b REPEAT \n" +
            //    "PRINT a \n" +
            //    "a = a + 1 \n" +
            //    "ENDWHILE \n" +
            //    "if IF while let";
            //lexer = new(sourceCode2);

            //Lexer.Token? token = lexer.GetToken();

            //while (token?.TokenKind != Lexer.Token.TokenType.EOF)
            //{
            //    Console.WriteLine(token?.TokenKind);
            //    token = lexer.GetToken();
            //}

            if(args.Length != 1)
            {
                Console.WriteLine("Error: Need source file to compile!");
                Console.WriteLine("Usage: ./SimpleCompiler.exe [input_file]");
                Environment.Exit(1);
            }

            using StreamReader reader = new(args[0]);
            string sourceCode = reader.ReadToEnd();

            Lexer lexer = new(sourceCode);
            Parser parser = new(lexer);

            parser.Program();
            Console.WriteLine("Parsing Completed.");

        }
    }
}
