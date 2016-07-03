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

		public static bool CompileCSharpCode(string sourceFile, string exeFile)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider();

			// Build the parameters for source compilation.
			CompilerParameters cp = new CompilerParameters();

			// Add an assembly reference.
			cp.ReferencedAssemblies.Add( "System.dll" );

			// Generate an executable instead of
			// a class library.
			cp.GenerateExecutable = true;

			// Set the assembly file name to generate.
			cp.OutputAssembly = exeFile;

			// Save the assembly as a physical file.
			cp.GenerateInMemory = false;

			// Invoke compilation.
			CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);

			if (cr.Errors.Count > 0)
			{
				// Display compilation errors.
				Console.WriteLine("Errors building {0} into {1}",
					sourceFile, cr.PathToAssembly);
				foreach (CompilerError ce in cr.Errors)
				{
					Console.WriteLine("  {0}", ce.ToString());
					Console.WriteLine();
				}
			}
			else
			{
				Console.WriteLine("Source {0} built into {1} successfully.",
					sourceFile, cr.PathToAssembly);
			}

			// Return the results of compilation.
			if (cr.Errors.Count > 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        public static void Main (string[] args)
        {
            try
            {
                var input = new AntlrInputStream(new FileStream("../../TestInput.java", FileMode.Open));
                var lexer = new MyJavaLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new MyJavaParser(tokens);

                // use the first rule of the parser
                // MyJavaParser.CompilationUnitContext tree = parser.compilationUnit();
                var my_action = new Java2CSharp();
                var tree = parser.compilationUnit();
                var walker = new ParseTreeWalker();
                walker.Walk(my_action, tree);
                GenerateCSharpCode("OutputUnit.cs", my_action.compileUnit);
				CompileCSharpCode("OutputUnit.cs", "myexe.exe");
				Process.Start("myexe.exe");

                // Console.WriteLine (my_action.Output);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
