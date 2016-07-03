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

		// Lookup table for some translations
		Dictionary<string,string> trans_dict = new Dictionary<string, string>()
		{
			{"boolean", "System.Boolean"},
			{"boolean[]", "System.Boolean[]"},
			{"char", "System.Char"},
			{"char[]", "System.Char[]"},
			{"System.out.println", "Console.WriteLine"}
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

        // Create a new CodeCompileUnit to contain 
        // the program graph.
        public CodeCompileUnit compileUnit = new CodeCompileUnit();
        // Declare a new namespace called Samples.
		CodeNamespace current_namespace = new CodeNamespace("Samples");

        //====================================================================================
        // Current, past values used while processing and assigning

        //------------------------------------------------------------------------------------
		// Current class is always the last in the list
		List<CodeTypeDeclaration> nest_types = new List<CodeTypeDeclaration>();

        // When we find nested class we need to assign it to its parent
        // so we track the parent
        CodeTypeDeclaration parent_class {
			get { return nest_types.Count () > 1 ? nest_types.ElementAt(nest_types.Count() - 2) : null; }
		}
        // Always points at the current class being built
        CodeTypeDeclaration current_class {
			get { return nest_types.Count () > 0 ? nest_types.Last () : null; }
		}

        //------------------------------------------------------------------------------------
		// To allow nested blocks of statements
		List<CodeStatementCollection> nest_blocks = new List<CodeStatementCollection> ();

		// can be inside another block. If this is null then we are inside a method
		CodeStatementCollection parent_block {
			get { return nest_blocks.Count () > 1 ? nest_blocks.ElementAt(nest_types.Count() - 2) : null; }
		}
		// current block of statements: block of method , block of loop, block of inner loop, etc.
		CodeStatementCollection current_block {
			get { return nest_blocks.Count () > 0 ? nest_blocks.Last() : null; }
		}

        //------------------------------------------------------------------------------------
		// Other current values

		// check if method is public and static. 
		// Used to look for "main" entry point.
		bool is_public_static = false;

        // Current method that is being build
        CodeMemberMethod current_meth;

        // Current field being build
        CodeMemberField current_field;

        // Current variable that is being initialized or declared;
        CodeVariableDeclarationStatement current_variable;

		// Current loop. NESTED LOOPS NOT ALLOWED.
		CodeIterationStatement current_loop;


        //====================================================================================
        // Big large declarations. Classes and class members.

        public override void EnterCompilationUnit (MyJavaParser.CompilationUnitContext context)
        {
            // Add the new namespace to the compile unit.
            compileUnit.Namespaces.Add(current_namespace);
            // Add the new namespace import for the System namespace.
            current_namespace.Imports.Add(new CodeNamespaceImport("System"));
            current_namespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
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

        public override void EnterClassDeclaration (MyJavaParser.ClassDeclarationContext context)
        {
			// PUSH new class onto nesting stack
			nest_types.Add (new CodeTypeDeclaration (context.Identifier ().GetText ()));
			// we are working with the NEW class
            current_class.IsClass = true;
            if (parent_class != null) {
                parent_class.Members.Add (current_class);
            } else {
				// add to namespace
                current_namespace.Types.Add (current_class);
            }
        }

        public override void ExitClassDeclaration (MyJavaParser.ClassDeclarationContext context)
        {
			// remove last nesting item, Pop from the nesting stack
			nest_types.RemoveAt (nest_types.Count () - 1);
        }

		public override void EnterClassOrInterfaceModifier (MyJavaParser.ClassOrInterfaceModifierContext context)
		{
			is_public_static = false;
			var modifiers = context.children.Select (x => x.GetText ());
			if (modifiers.Contains ("public") && modifiers.Contains ("static")) {
				is_public_static = true;
			}
		}

        public override void EnterMethodDeclaration (MyJavaParser.MethodDeclarationContext context)
        {
			
			string name = context.Identifier ().GetText ();
			if (name == "main" && is_public_static) {
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
        // Becasue we loose all whitespace during parsing.

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


        //====================================================================================
		// Handle statement colelctions. Inside method, inside for loop, nested for loop, etc.

        public override void EnterLocalVariableDeclaration (MyJavaParser.LocalVariableDeclarationContext context)
        {
            current_variable = new CodeVariableDeclarationStatement ();
            current_variable.Name = context.variableDeclarator ().variableDeclaratorId ().GetText ();
			current_variable.Type = new CodeTypeReference(translate(context.type().GetText()));
        }

		// When we exit we check for initialiser expression
		public override void ExitLocalVariableDeclaration (MyJavaParser.LocalVariableDeclarationContext context)
		{
			var initializer = context.variableDeclarator ().variableInitializer ();
			// if init is not none
			if (initializer != null) {
				// get the code for the expression
				string init_code = null;
				var expr = initializer.expression ();
				if (expr != null) {
					init_code = GetGlobalData<string> (initializer.expression ());
				} else {
					var array_init = initializer.arrayInitializer ();
					if (array_init != null) {
						// this means no "new" keyword in array initialiser 
						// and no nested array initialisers
						init_code = array_init.GetText ();
					}
				}
				current_variable.InitExpression
					= new CodeSnippetExpression (init_code);
			}
		}

		// When we exit we check for initialiser expression
		public override void ExitFieldDeclaration (MyJavaParser.FieldDeclarationContext context)
		{
			var initializer = context.variableDeclarator ().variableInitializer ();
			// if init is not none
			if (initializer != null) {
				// get the code for the expression
				string init_code = null;
				var expr = initializer.expression ();
				if (expr != null) {
					init_code = GetGlobalData<string> (initializer.expression ());
				} else {
					var array_init = initializer.arrayInitializer ();
					if (array_init != null) {
						// this means no "new" keyword in array initialiser 
						// and no nested array initialisers
						init_code = array_init.GetText ();
					}
				}
				current_field.InitExpression
					= new CodeSnippetExpression (init_code);
			}
		}


        //====================================================================================
		// Handle nested blocks of statements

		public override void EnterBlock (MyJavaParser.BlockContext context)
		{
			// PUSH inner block onto the stack
			nest_blocks.Add (new CodeStatementCollection());
		}

		// After a collection of statements is assembled
		public override void ExitBlock (MyJavaParser.BlockContext context)
		{
			// Either we are in a some nested loop or in a method
			if (current_loop == null) {
				current_meth.Statements.AddRange (current_block);
			} else {
				current_loop.Statements.AddRange (current_block);
			}
			// POP the block
			nest_blocks.RemoveAt (nest_blocks.Count () - 1);
		}

		// Only here (and in for-loop starter counter) we add stuff to the current block.
		// We add local variable declarations and statements (i.e. commands/actions)
		// We do NOT add type declarations.
		public override void ExitBlockStatement (MyJavaParser.BlockStatementContext context)
		{
			var type_decl = context.typeDeclaration ();
			if (type_decl != null) {
				// nothing to do here, this is a type declaration
				return;
			}
			var stmt = context.statement ();
			if (stmt != null) {
				// if the statement is not empty
				// maybe its just an expression
				if (stmt.expression () != null) {
					current_block.Add(
						new CodeSnippetExpression (GetGlobalData<string>(stmt.expression())));
				}
				// maybe its a for loop
				if (stmt.enhancedForControl () != null) { 
					current_block.Add (current_loop);
					current_loop = null;
				}
				// ignore empty statements, and block statements are handled in ExitBlock
			} else {
				var local_var_decl = context.localVariableDeclaration ();
				if (local_var_decl != null) {
					current_block.Add (current_variable);
				}
			}
		}


        //====================================================================================
		// For each loops

		public override void EnterEnhancedForControl (MyJavaParser.EnhancedForControlContext context)
		{
			string counter_name = new string( Guid.NewGuid ().ToString ()
				.Where (ch => char.IsLetter (ch))
				.ToArray () 
			);
			current_loop = new CodeIterationStatement ();
			// initStatement parameter for pre-loop initialization.
			current_loop.InitStatement = new CodeSnippetStatement("System.Int32 " + counter_name + " = 1");
			// testExpression parameter to test for continuation condition.
			current_loop.TestExpression 
				= new CodeSnippetExpression (counter_name + " < " + context.expression ().GetText () + ".Count()");
			// increment statement
			current_loop.IncrementStatement = new CodeSnippetStatement (counter_name + " += 1");
			current_loop.Statements.Add (
				new CodeSnippetExpression ("var " 
					+ context.variableDeclaratorId ().GetText () 
					+ " = "
					+ context.expression().GetText()
					+ "[" + counter_name + "]")
			);
		}


    }

}

