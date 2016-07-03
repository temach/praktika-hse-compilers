using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.IO;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.CSharp;

namespace complex_compiler
{

    class MainClass
    {

        // Generate CSharp source code from the compile unit.
        public static void GenerateCSharpCode(string fileName, CodeCompileUnit unit)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (StreamWriter sourceWriter = new StreamWriter(fileName))
            {
                provider.GenerateCodeFromCompileUnit(
                        unit, sourceWriter, options);
            }
        }


		public static void Work(string java, string csharp)
		{
			using (var fs = new FileStream(java, FileMode.Open))
			{
				var input = new AntlrInputStream(fs);
                var lexer = new MyJavaLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new MyJavaParser(tokens);

                // use the first rule of the parser
                // MyJavaParser.CompilationUnitContext tree = parser.compilationUnit();
                var my_action = new Java2CSharp();
                var tree = parser.compilationUnit();
                var walker = new ParseTreeWalker();
                walker.Walk(my_action, tree);
                GenerateCSharpCode(csharp, my_action.compileUnit);
			}
		}

        public static void Main (string[] args)
        {
            try
            {
				if (args.Length < 2)
				{
					throw new System.Exception("" +
						"Usage: \n\n\n" +
						" Compiling into c sharp: \n" +
						" java2csharp input_file.java output_file.cs");
				}
				string in_file_java = args[0];
				string out_file_csharp = args[1];
				Work(in_file_java, out_file_csharp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
