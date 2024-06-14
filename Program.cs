namespace SimpleCompiler
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if(args.Length != 1)
            {
                Console.WriteLine("Error: Need source file to compile!");
                Console.WriteLine("Usage: ./SimpleCompiler.exe [input_file]");
                Environment.Exit(1);
            }

            using StreamReader reader = new(args[0]);
            string sourceCode = reader.ReadToEnd();

            Lexer lexer = new(sourceCode);
            Emitter emitter = new("out.c");
            Parser parser = new(lexer, emitter);

            parser.Program();
            emitter.WriteFile();
            Console.WriteLine("Compiling Completed.");
        }
    }
}
