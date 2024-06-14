using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    internal class Emitter
    {
        private string _fileName;
        private string _code;
        private string _header;

        public Emitter(string fileName)
        {
            _fileName = fileName;
            _code = string.Empty;
            _header = string.Empty;
        }

        void Emit(string code)
        {
            _code += code;
        }

        void EmitLine(string code)
        {
            _code += code + '\n';
        }

        void HeaderLine(string code)
        {
            _header += code + '\n';
        }

        public async void WriteFile()
        {
            using (StreamWriter outputFile = new(Path.Combine(Directory.GetCurrentDirectory(), _fileName)))
            {
                await outputFile.WriteAsync(_header +_code);
            }
        }
    }
}
