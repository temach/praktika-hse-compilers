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
		}

		//====================================================================================
		// Storage for the results of parsing and analysing

		// What we need to write to output "blabla.cs" file. Then we will compile this file.
		public List<string> result = new List<string>();

		public bool nested = false;
		public List<List<string>> nested_classes = new List<List<string>> ();

		// Create a new CodeCompileUnit to contain 
		// the program graph.
		public CodeCompileUnit compileUnit = new CodeCompileUnit();
		// Declare a new namespace called Samples.
		CodeNamespace samples = new CodeNamespace("Samples");

		//====================================================================================
		// Current, past values used while processing and assigning

		// ONLY A  DEPTH OF ONE NESTING IS SUPPORTED CURRENTLY
		// BECAUSE TO SUPPORT MORE DEPTH WE NEED CHAIN OF PARENTS

		// When we find nested class we need to assign it to its parent
		// so we track the parent
		CodeTypeDeclaration parent_class;
		// Always points at the current class being built
		CodeTypeDeclaration current_class;

		// Current method that is being build
		CodeMemberMethod current_meth;

		// Current field being build
		CodeMemberField current_field;

		//====================================================================================
		// Big large declarations. Classes and class members.

		public override void EnterCompilationUnit (MyJavaParser.CompilationUnitContext context)
		{
			// Add the new namespace to the compile unit.
			compileUnit.Namespaces.Add(samples);
			// Add the new namespace import for the System namespace.
			samples.Imports.Add(new CodeNamespaceImport("System"));
		}

		public override void EnterType (MyJavaParser.TypeContext context)
		{
			current_field.Type = new CodeTypeReference (typeof(System.Char));
		}

		// EnterFieldMemberDecl and EnterLocalVariableDeclaration are the same things really
		public override void EnterFieldMemberDecl (MyJavaParser.FieldMemberDeclContext context)
		{
			current_field = new CodeMemberField();
			current_class.Members.Add(current_field);
		}

		public override void EnterLocalVariableDeclaration (MyJavaParser.LocalVariableDeclarationContext context)
		{
			current_field = new CodeMemberField();
			current_class.Members.Add(current_field);
		}

		public override void EnterVariableDeclaratorId (MyJavaParser.VariableDeclaratorIdContext context)
		{
			current_field.Name = context.Identifier ().GetText();
		}

		public override void EnterClassDeclaration (MyJavaParser.ClassDeclarationContext context)
		{
			parent_class = current_class;
			current_class = new CodeTypeDeclaration (context.Identifier ().GetText ());
			current_class.IsClass = true;
			samples.Types.Add (current_class);
		}

		public override void ExitClassDeclaration (MyJavaParser.ClassDeclarationContext context)
		{
			current_class = parent_class;
			parent_class = null;
		}

		public override void EnterMethodDeclaration (MyJavaParser.MethodDeclarationContext context)
		{
			current_meth = new CodeMemberMethod ();
			current_meth.Attributes = MemberAttributes.Public;
			current_meth.Name = context.Identifier ().GetText ();
			current_meth.ReturnType = new CodeTypeReference (typeof(System.Double));
			current_class.Members.Add (current_meth);
		}

		public override void EnterFormalParameter (MyJavaParser.FormalParameterContext context)
		{
			current_meth.Parameters.Add(
				new CodeParameterDeclarationExpression(context.type().GetText(), context.variableDeclaratorId().GetText())
			);
		}







	}


	class MainClass
	{
		/// <summary>
		/// Generate CSharp source code from the compile unit.
		/// </summary>
		/// <param name="filename">Output file name</param>
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
					GenerateCSharpCode("OutputUnit.cs", my_action.compileUnit);
					// Console.WriteLine (my_action.Output);
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
