namespace SimpleCompiler
{
    internal class Program
    {
        //static void Main(string[] args)
        static void Main()
        {

            //SIMPLE TESTING
            string sourceCode = "IF 123 = 123 DO SOMETHING";

            Lexer lexer = new(sourceCode);

            while (lexer.Peek() != '\0')
            { 
                Console.WriteLine(lexer.currentChar);
                lexer.NextCharacter();
            }


            //TOKEN TESTING

            string sourceCode2 = "+- */>>= = !=   ==\n" +
                "#This is a COMMENT new line\n" +
                " +  = \n" +
                " \"this is a string \" ";
            lexer = new(sourceCode2);

            Lexer.Token? token = lexer.GetToken();

            while (token?.TokenKind != Lexer.Token.TokenType.EOF)
            {
                Console.WriteLine(token?.TokenKind);
                token = lexer.GetToken();
            }
        }
    }
}
