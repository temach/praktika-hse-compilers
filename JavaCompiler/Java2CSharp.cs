using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

namespace JavaCompiler
{
    class Java2CSharp : MyJavaBaseListener
    {
        //====================================================================================
        // Helper parts.

        // Lookup table for some translations
        Dictionary<string, string> trans_dict = new Dictionary<string, string>()
		{
			{"boolean", "System.Boolean"},
			{"char", "System.Char"},
		};
        string translate(string java)
        {
            if (trans_dict.ContainsKey(java))
            {
                return trans_dict[java];
            }
            else
            {
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
            databank.Put(node, data);
        }

        // get what you stored for the node
        public T GetGlobalData<T>(IParseTree node)
        {
            return (T)databank.Get(node);
        }

        //====================================================================================
        // Storage for the results of parsing and analysing

        // Create a new CodeCompileUnit to contain 
        // the program graph.
        public CodeCompileUnit compileUnit = new CodeCompileUnit();
        // Declare a new namespace called Samples.
        CodeNamespace samples = new CodeNamespace("Samples");

        //====================================================================================
        // Current, past values used while processing and assigning

        // Current class is always the last in the list
        List<CodeTypeDeclaration> nesting = new List<CodeTypeDeclaration>();

        // When we find nested class we need to assign it to its parent
        // so we track the parent
        CodeTypeDeclaration parent_class
        {
            get { return nesting.Count() > 1 ? nesting.ElementAt(nesting.Count() - 2) : null; }
        }
        // Always points at the current class being built
        CodeTypeDeclaration current_class
        {
            get { return nesting.Count() > 0 ? nesting.Last() : null; }
        }

        // Current method that is being build
        CodeMemberMethod current_meth;

        // Current field being build
        CodeMemberField current_field;

        // Current variable that is being initialized or declared;
        CodeVariableDeclarationStatement current_variable;

        bool is_public_static = false;

        //====================================================================================
        // Big large declarations. Classes and class members.

        public override void EnterCompilationUnit(MyJavaParser.CompilationUnitContext context)
        {
            // Add the new namespace to the compile unit.
            compileUnit.Namespaces.Add(samples);
            // Add the new namespace import for the System namespace.
            samples.Imports.Add(new CodeNamespaceImport("System"));
        }

        public override void EnterFieldMemberDecl(MyJavaParser.FieldMemberDeclContext context)
        {
            current_field = new CodeMemberField();
            current_field.Name
                = context.fieldDeclaration().variableDeclarator().variableDeclaratorId().GetText();
            current_field.Type
                = new CodeTypeReference(translate(context.fieldDeclaration().type().GetText()));
            current_class.Members.Add(current_field);
            current_field.Attributes = MemberAttributes.Public;
        }

        public override void EnterLocalVariableDeclaration(MyJavaParser.LocalVariableDeclarationContext context)
        {
            current_variable = new CodeVariableDeclarationStatement();
            current_variable.Name = context.variableDeclarator().variableDeclaratorId().GetText();
            current_variable.Type = new CodeTypeReference(translate(context.type().GetText()));
            current_meth.Statements.Add(current_variable);
        }

        public override void EnterClassDeclaration(MyJavaParser.ClassDeclarationContext context)
        {
            // PUSH new class onto nesting stack
            nesting.Add(new CodeTypeDeclaration(context.Identifier().GetText()));
            // we are working with the NEW class
            current_class.IsClass = true;
            if (parent_class != null)
            {
                parent_class.Members.Add(current_class);
            }
            else
            {
                samples.Types.Add(current_class);
            }
        }

        public override void ExitClassDeclaration(MyJavaParser.ClassDeclarationContext context)
        {
            // remove last nesting item, Pop from the nesting stack
            nesting.RemoveAt(nesting.Count() - 1);
        }

        public override void EnterClassOrInterfaceModifier(MyJavaParser.ClassOrInterfaceModifierContext context)
        {
            is_public_static = false;
            var modifiers = context.children.Select(x => x.GetText());
            if (modifiers.Contains("public") && modifiers.Contains("static"))
            {
                is_public_static = true;
            }
        }

        public override void EnterMethodDeclaration(MyJavaParser.MethodDeclarationContext context)
        {
            string name = context.Identifier().GetText();
            if (name == "main" && is_public_static)
            {
                current_meth = new CodeEntryPointMethod();
            }
            else
            {
                current_meth = new CodeMemberMethod();
                current_meth.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                current_meth.Name = context.Identifier().GetText();
                current_meth.ReturnType
                    = new CodeTypeReference(context.type() == null ? "" : context.type().GetText());
            }
            current_class.Members.Add(current_meth);
        }

        public override void EnterFormalParameter(MyJavaParser.FormalParameterContext context)
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
        public override void ExitExpression(MyJavaParser.ExpressionContext context)
        {
            string to_set = "";
            if (context.creator() != null)
            {
                // if we need to add space to new operator, do it here
                to_set = "new " + context.creator().GetText();
            }
            else
            {
                // other expressions are __NOT__ sensitive to whitespace
                to_set = context.GetText();
            }
            SetGlobalData(context, to_set.Replace("System.out.println", "Console.WriteLine"));
        }

        public override void ExitLocalVariableDeclaration(MyJavaParser.LocalVariableDeclarationContext context)
        {
            var expr = context.variableDeclarator().expression();
            if (expr != null)
            {
                current_variable.InitExpression
                    = new CodeSnippetExpression(GetGlobalData<string>(expr));
            }
        }

        public override void ExitFieldDeclaration(MyJavaParser.FieldDeclarationContext context)
        {
            var expr = context.variableDeclarator().expression();
            if (expr != null)
            {
                current_field.InitExpression
                    = new CodeSnippetExpression(GetGlobalData<string>(expr));
            }
        }

        public override void ExitStatement(MyJavaParser.StatementContext context)
        {
            if (context.expression() != null)
            {
                current_meth.Statements.Add(
                    new CodeSnippetExpression(GetGlobalData<string>(context.expression())));
            }
        }


    }
}
