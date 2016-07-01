using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.IO;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Text;

namespace complex_compiler
{

	class Java2CSharp : MyJavaBaseListener
	{
		MyJavaParser parser;

		public Java2CSharp(MyJavaParser p)
		{
			this.parser = p;
		}

	}


	class MainClass
	{

		public static void Main (string[] args)
		{
			while (true)
			{
				try
				{
					var input = new AntlrInputStream(Encoding.ASCII.GetBytes(Console.ReadLine()));
					var lexer = new MyJavaLexer(input);
					var tokens = new CommonTokenStream(lexer);
					var parser = new MyJavaParser(tokens);

					// use the first rule of the parser
					MyJavaParser.CompilationUnitContext tree = parser.compilationUnit();

					var walker = new ParseTreeWalker();
					var my_action = new Java2CSharp(parser);
					walker.Walk(my_action, tree);
					Console.WriteLine ();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
		}
	}
}
