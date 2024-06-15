using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompiler
{
    internal class Emitter(string fileName)
    {
        private string _fileName = fileName;
        private string _code = string.Empty;
        private string _header = string.Empty;

        public void Emit(string code)
        {
            _code += code;
        }

        public void EmitLine(string code)
        {
            _code += code + '\n';
        }

        public void HeaderLine(string code)
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
