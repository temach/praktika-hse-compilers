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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        CodeCompileUnit resultCompileUnit;
        string savedCSharpFile;
        string exeCompiledFile;

        public CodeCompileUnit TranslateFile(string fname)
        {
            var input = new AntlrInputStream(new FileStream(fname, FileMode.Open));
            var lexer = new MyJavaLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new MyJavaParser(tokens);

            // use the first rule of the parser
            // MyJavaParser.CompilationUnitContext tree = parser.compilationUnit();
            var my_action = new Java2CSharp();
            var tree = parser.compilationUnit();
            var walker = new ParseTreeWalker();
            walker.Walk(my_action, tree);
            return my_action.compileUnit;
        }

        private void SetOutput(IEnumerable<string> data, ListBox target)
        {
            if (data.Count() < 1)
            {
                return;
            }
            target.Items.Clear();
            foreach (var item in data)
            {
                target.Items.Add(item.ToString());
            }
            target.Invalidate();
        }

        private void button_LoadJavaFile_Click(object sender, EventArgs e)
        {
            try
            {
                string fname = OpenFileDialogGetPath();
                resultCompileUnit = TranslateFile(fname);
                SetOutput(File.ReadAllLines(fname), this.listBox_Java);
            }
            catch (Exception ex)
            {
                SetOutput(ex.Message.Split('\n'), this.listBox_errors);
                MessageBox.Show("Could not open file.");
                return;
            }
            MessageBox.Show("Opened file");
        }

        // Generate CSharp source code from the compile unit.
        public void GenerateCSharpCode(string fileName, CodeCompileUnit unit)
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

        public bool CompileCSharpCode(string sourceFile, string exeFile)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();

            // Build the parameters for source compilation.
            CompilerParameters cp = new CompilerParameters();

            // Add an assembly reference.
            cp.ReferencedAssemblies.Add("System.dll");

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

        /// <summary>
        /// Opens a dialog to get path of file to open from te user.
        /// </summary>
        public static string OpenFileDialogGetPath()
        {
            OpenFileDialog file_dialog = new OpenFileDialog
            {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = "All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true,
                Title = "Select a file...",
            };
            if (file_dialog.ShowDialog() == DialogResult.OK)
            {
                return file_dialog.FileName;
            }
            return null;
        }


        /// <summary>
        /// Opens the dialog to get the path at which to save the current data.
        /// </summary>
        /// <returns></returns>
        public static string SaveFileDialogGetPath()
        {
            SaveFileDialog file_dialog = new SaveFileDialog
            {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = "All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true,
                Title = "Select where to save your file...",
            };
            if (file_dialog.ShowDialog() == DialogResult.OK)
            {
                return file_dialog.FileName;
            }
            return null;
        }

        private void button_compile_java2csharp_Click(object sender, EventArgs e)
        {
            try
            {
                exeCompiledFile = SaveFileDialogGetPath();
                GenerateCSharpCode(savedCSharpFile, resultCompileUnit);
                SetOutput(File.ReadAllLines(savedCSharpFile), this.listBox_CSharp);
            }
            catch (Exception ex)
            {
                SetOutput(ex.Message.Split('\n'), this.listBox_errors);
                MessageBox.Show("Could not write to file.");
                return;
            }
            MessageBox.Show("Saved to file");
        }

        private void button_Run_CSharp_Click(object sender, System.EventArgs e)
        {
            CompileCSharpCode(savedCSharpFile, exeCompiledFile);
            Process.Start(exeCompiledFile);
        }


    }
}
