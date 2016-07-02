﻿using System;
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

		// Lookup table for some translations
		Dictionary<string,string> trans_dict = new Dictionary<string, string>()
		{
			{"boolean", "System.Boolean"},
			{"char", "System.Char"},
		};
		string translate(string java)
		{
			if (trans_dict.ContainsKey (java)) {
				return trans_dict [java];
			} else {
				// if its a name of a class for example
				return java;
			}
		}

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

        // Current variable that is being initialized or declared;
        CodeVariableDeclarationStatement current_variable;

        //====================================================================================
        // Big large declarations. Classes and class members.

        public override void EnterCompilationUnit (MyJavaParser.CompilationUnitContext context)
        {
            // Add the new namespace to the compile unit.
            compileUnit.Namespaces.Add(samples);
            // Add the new namespace import for the System namespace.
            samples.Imports.Add(new CodeNamespaceImport("System"));
        }

        public override void EnterFieldMemberDecl (MyJavaParser.FieldMemberDeclContext context)
        {
			current_field = new CodeMemberField ();
            current_field.Name 
                = context.fieldDeclaration ().variableDeclarator ().variableDeclaratorId ().GetText ();
			current_field.Type 
				= new CodeTypeReference (translate(context.fieldDeclaration().type().GetText()));
            current_class.Members.Add(current_field);
			current_field.Attributes = MemberAttributes.Public;
        }

        public override void EnterLocalVariableDeclaration (MyJavaParser.LocalVariableDeclarationContext context)
        {
            current_variable = new CodeVariableDeclarationStatement ();
            current_variable.Name = context.variableDeclarator ().variableDeclaratorId ().GetText ();
			current_variable.Type = new CodeTypeReference(translate(context.type().GetText()));
			current_meth.Statements.Add (current_variable);
        }

        public override void EnterClassDeclaration (MyJavaParser.ClassDeclarationContext context)
        {
            parent_class = current_class;
            current_class = new CodeTypeDeclaration (context.Identifier ().GetText ());
            current_class.IsClass = true;
            if (parent_class != null) {
                parent_class.Members.Add (current_class);
            } else {
                samples.Types.Add (current_class);
            }
        }

        public override void ExitClassDeclaration (MyJavaParser.ClassDeclarationContext context)
        {
            current_class = parent_class;
            parent_class = null;
        }

        public override void EnterMethodDeclaration (MyJavaParser.MethodDeclarationContext context)
        {
			// TODO: fix main method finding. Add flags to check for "static public"
			string name = context.Identifier ().GetText ();
			if (name == "main") {
				current_meth = new CodeEntryPointMethod();
			} else {
            	current_meth = new CodeMemberMethod ();
				current_meth.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            	current_meth.Name = context.Identifier ().GetText ();
				current_meth.ReturnType 
					= new CodeTypeReference (context.type() == null ? "" : context.type().GetText());
			}
            current_class.Members.Add (current_meth);
        }

        public override void EnterFormalParameter (MyJavaParser.FormalParameterContext context)
        {
            current_meth.Parameters.Add(
              new CodeParameterDeclarationExpression(context.type().GetText()
					, context.variableDeclaratorId().GetText())
            );
        }

        //====================================================================================
        // Assemble primitive type declarations into one string. 
        // Becasue we lost all whitespace during parsing.

        // When we exit expression with "new" we add a space
        // since can not just use .GetText(); wich breaks "new"
        public override void ExitExpression (MyJavaParser.ExpressionContext context)
        {
			string to_set = "";
            if (context.creator () != null) {
                // if we need to add space to new operator, do it here
                to_set = "new " + context.creator ().GetText ();
			} else {
                // other expressions are __NOT__ sensitive to whitespace
                to_set = context.GetText ();
            }
			SetGlobalData (context, to_set.Replace("System.out.println", "Console.WriteLine"));
        }

		public override void ExitLocalVariableDeclaration (MyJavaParser.LocalVariableDeclarationContext context)
		{
			var expr = context.variableDeclarator ().expression ();
			if (expr != null) {
				current_variable.InitExpression
					= new CodeSnippetExpression (GetGlobalData<string> (expr));
			}
		}

		public override void ExitFieldDeclaration (MyJavaParser.FieldDeclarationContext context)
		{
			var expr = context.variableDeclarator ().expression ();
			if (expr != null) {
				current_field.InitExpression
					= new CodeSnippetExpression (GetGlobalData<string> (expr));
			}
		}

		public override void ExitStatement (MyJavaParser.StatementContext context)
		{
			if (context.expression () != null) {
				current_meth.Statements.Add(
					new CodeSnippetExpression (GetGlobalData<string>(context.expression())));
			}
		}


    }


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
                var my_action = new Java2CSharp(parser);
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
