using System;
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

namespace compiler_csharp
{
	class MainClass
	{
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

		public static void Work(string csharp, string exe)
		{
			CompileCSharpCode(csharp, exe);
			Process.Start(exe);
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
