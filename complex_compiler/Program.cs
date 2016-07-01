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

	class Java2CSharp : MyJavaBaseListener
	{
		//====================================================================================
		// Helper parts.
		MyJavaParser parser;

		// This is a dictionary of nodes to values.
		// Now we can store a value for each node.
		ParseTreeProperty<object> databank = new ParseTreeProperty<object>();

		public string Output {
			get { return string.Join ("\n", this.result); }
		}

		// store some data for the current node
		public void SetGlobalData(IParseTree node, object data)
		{
			databank.Put (node, data);
		}

		// get what you stored for the node
		public T GetGlobalData<T>(IParseTree node)
		{
			return (T)databank.Get (node);
		}

		public Java2CSharp(MyJavaParser p)
		{
			this.parser = p;

			// Add the new namespace to the compile unit.
			compileUnit.Namespaces.Add(samples);
			// Add the new namespace import for the System namespace.
			samples.Imports.Add(new CodeNamespaceImport("System"));
		}

		//====================================================================================
		// Storage for the results of parsing and analysing

		// What we need to write to output "blabla.cs" file. Then we will compile this file.
		public List<string> result = new List<string>();

		public bool nested = false;
		public List<List<string>> nested_classes = new List<List<string>> ();

		// Create a new CodeCompileUnit to contain 
		// the program graph.
		CodeCompileUnit compileUnit = new CodeCompileUnit();
		// Declare a new namespace called Samples.
		CodeNamespace samples = new CodeNamespace("Samples");


		//====================================================================================
		// Big large declarations. Classes and class members.

		public override void EnterCompilationUnit (MyJavaParser.CompilationUnitContext context)
		{
			result.Add ("using System;");
		}

		public override void EnterTypeDeclaration (MyJavaParser.TypeDeclarationContext context)
		{
			if (context.Parent.RuleIndex == parser.blockStatement ().RuleIndex) {
				// if they are equal then we are declaring a nested class in a method
				this.nested = true;
				this.nested_classes.Add (new List<string> ());
			} else {
				this.nested = false;
			}
		}

		public override void ExitTypeDeclaration (MyJavaParser.TypeDeclarationContext context)
		{
		}


		public override void EnterClassDeclaration (MyJavaParser.ClassDeclarationContext context)
		{
			CodeTypeDeclaration cls_decl = new CodeTypeDeclaration (context.Identifier ().GetText ());
			result.Add("class " + context.Identifier());
		}

		public override void EnterClassBody (MyJavaParser.ClassBodyContext context)
		{
			result.Add ("{");
		}
		public override void ExitClassBody (MyJavaParser.ClassBodyContext context)
		{
			result.Add ("}");
		}

		public override void EnterClassOrInterfaceModifier (MyJavaParser.ClassOrInterfaceModifierContext context)
		{
			result.Add (string.Join (" ", context.children.Select (x => x.GetText ())));
		}

		public override void EnterMethodDeclaration (MyJavaParser.MethodDeclarationContext context)
		{
			string ret = context.type () == null ? "void" : context.type ().GetText ();
			string ws = " ";
			result.Add (ret + ws + context.Identifier ().GetText () + ws);
		}

		public override void EnterFormalParameters (MyJavaParser.FormalParametersContext context)
		{
			result.Add (context.GetText ());
		}
		
		public override void EnterMethodBody (MyJavaParser.MethodBodyContext context)
		{
			result.Add ("{");
		}

		public override void ExitMethodBody (MyJavaParser.MethodBodyContext context)
		{
			result.Add ("}");
		}

		public override void ExitBlockStatement (MyJavaParser.BlockStatementContext context)
		{
			if (context.localVariableDeclaration () != null) {
			}
		}




		//====================================================================================
		// Assemble primitive type declarations into one string. 
		// Becasue we lost all whitespace during parsing.

		// When we exit expression with "new" we add a space
		// since can not just use .GetText(); wich breaks "new"
		public override void ExitExpression (MyJavaParser.ExpressionContext context)
		{
			if (context.creator () != null) {
				// if we need to add space to new operator, do it here
				SetGlobalData (context, "new " + context.creator ().GetText ());
			} else {
				// other expressions are __NOT__ sensitive to whitespace
				SetGlobalData (context, context.GetText ());
			}
		}

		// If we had an expression, 
		// look it up in the databank, (because maybe it had "new" keyword)
		// and we can not just use .GetText(); wich breaks "new"
		public override void ExitVariableDeclarator (MyJavaParser.VariableDeclaratorContext context)
		{
			string res = context.variableDeclaratorId().GetText();
			if (context.expression() != null)
			{
				// if there is an expression on the left side for initialization
				res += "=" + GetGlobalData<string>(context.expression ());
			}
			SetGlobalData (context, res);
		}

		// combine the left and right parts of local var declaration.
		// Because we again need to separate "variable type" with "variable name" by a space
		public override void ExitLocalVariableDeclaration (MyJavaParser.LocalVariableDeclarationContext context)
		{
			result.Add(context.type().GetText() 
				+ " " 
				+ GetGlobalData<string>(context.variableDeclarator()) 
				+ ";"
			);
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
					var input = new AntlrInputStream(new FileStream("../../TestInput.java", FileMode.Open));
					var lexer = new MyJavaLexer(input);
					var tokens = new CommonTokenStream(lexer);
					var parser = new MyJavaParser(tokens);

					// use the first rule of the parser
					// MyJavaParser.CompilationUnitContext tree = parser.compilationUnit();
					var my_action = new Java2CSharp(parser);
					var tree = parser.compilationUnit();
					var walker = new ParseTreeWalker();
					walker.Walk(my_action, tree);
					Console.WriteLine (my_action.Output);
					Console.ReadLine();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
		}
	}
}
