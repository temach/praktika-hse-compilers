namespace JavaCompiler
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_LoadJavaFile = new System.Windows.Forms.Button();
            this.button_compile_java2csharp = new System.Windows.Forms.Button();
            this.button_Test_One_Java = new System.Windows.Forms.Button();
            this.button_Test_Two_File = new System.Windows.Forms.Button();
            this.button_Test_Three_File = new System.Windows.Forms.Button();
            this.button_Test_File_Four = new System.Windows.Forms.Button();
            this.listBox_Java = new System.Windows.Forms.ListBox();
            this.listBox_CSharp = new System.Windows.Forms.ListBox();
            this.button_Run_CSharp = new System.Windows.Forms.Button();
            this.listBox_errors = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // button_LoadJavaFile
            // 
            this.button_LoadJavaFile.Location = new System.Drawing.Point(43, 42);
            this.button_LoadJavaFile.Name = "button_LoadJavaFile";
            this.button_LoadJavaFile.Size = new System.Drawing.Size(135, 23);
            this.button_LoadJavaFile.TabIndex = 0;
            this.button_LoadJavaFile.Text = "Load Java File";
            this.button_LoadJavaFile.UseVisualStyleBackColor = true;
            this.button_LoadJavaFile.Click += new System.EventHandler(this.button_LoadJavaFile_Click);
            // 
            // button_compile_java2csharp
            // 
            this.button_compile_java2csharp.Location = new System.Drawing.Point(43, 89);
            this.button_compile_java2csharp.Name = "button_compile_java2csharp";
            this.button_compile_java2csharp.Size = new System.Drawing.Size(135, 23);
            this.button_compile_java2csharp.TabIndex = 1;
            this.button_compile_java2csharp.Text = "Translate to CSharp";
            this.button_compile_java2csharp.UseVisualStyleBackColor = true;
            this.button_compile_java2csharp.Click += new System.EventHandler(this.button_compile_java2csharp_Click);
            // 
            // button_Test_One_Java
            // 
            this.button_Test_One_Java.Location = new System.Drawing.Point(233, 12);
            this.button_Test_One_Java.Name = "button_Test_One_Java";
            this.button_Test_One_Java.Size = new System.Drawing.Size(95, 23);
            this.button_Test_One_Java.TabIndex = 2;
            this.button_Test_One_Java.Text = "Test file one";
            this.button_Test_One_Java.UseVisualStyleBackColor = true;
            // 
            // button_Test_Two_File
            // 
            this.button_Test_Two_File.Location = new System.Drawing.Point(233, 41);
            this.button_Test_Two_File.Name = "button_Test_Two_File";
            this.button_Test_Two_File.Size = new System.Drawing.Size(95, 23);
            this.button_Test_Two_File.TabIndex = 3;
            this.button_Test_Two_File.Text = "Test file two";
            this.button_Test_Two_File.UseVisualStyleBackColor = true;
            // 
            // button_Test_Three_File
            // 
            this.button_Test_Three_File.Location = new System.Drawing.Point(233, 70);
            this.button_Test_Three_File.Name = "button_Test_Three_File";
            this.button_Test_Three_File.Size = new System.Drawing.Size(95, 23);
            this.button_Test_Three_File.TabIndex = 4;
            this.button_Test_Three_File.Text = "Test file three";
            this.button_Test_Three_File.UseVisualStyleBackColor = true;
            // 
            // button_Test_File_Four
            // 
            this.button_Test_File_Four.Location = new System.Drawing.Point(233, 99);
            this.button_Test_File_Four.Name = "button_Test_File_Four";
            this.button_Test_File_Four.Size = new System.Drawing.Size(95, 23);
            this.button_Test_File_Four.TabIndex = 5;
            this.button_Test_File_Four.Text = "Test file four";
            this.button_Test_File_Four.UseVisualStyleBackColor = true;
            // 
            // listBox_Java
            // 
            this.listBox_Java.FormattingEnabled = true;
            this.listBox_Java.Location = new System.Drawing.Point(12, 202);
            this.listBox_Java.Name = "listBox_Java";
            this.listBox_Java.Size = new System.Drawing.Size(316, 316);
            this.listBox_Java.TabIndex = 6;
            // 
            // listBox_CSharp
            // 
            this.listBox_CSharp.FormattingEnabled = true;
            this.listBox_CSharp.Location = new System.Drawing.Point(369, 202);
            this.listBox_CSharp.Name = "listBox_CSharp";
            this.listBox_CSharp.Size = new System.Drawing.Size(362, 316);
            this.listBox_CSharp.TabIndex = 7;
            // 
            // button_Run_CSharp
            // 
            this.button_Run_CSharp.Location = new System.Drawing.Point(43, 119);
            this.button_Run_CSharp.Name = "button_Run_CSharp";
            this.button_Run_CSharp.Size = new System.Drawing.Size(135, 23);
            this.button_Run_CSharp.TabIndex = 8;
            this.button_Run_CSharp.Text = "Compile and run CSharp";
            this.button_Run_CSharp.UseVisualStyleBackColor = true;
            this.button_Run_CSharp.Click += new System.EventHandler(this.button_Run_CSharp_Click);
            // 
            // listBox_errors
            // 
            this.listBox_errors.FormattingEnabled = true;
            this.listBox_errors.Location = new System.Drawing.Point(369, 30);
            this.listBox_errors.Name = "listBox_errors";
            this.listBox_errors.Size = new System.Drawing.Size(362, 121);
            this.listBox_errors.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 530);
            this.Controls.Add(this.listBox_errors);
            this.Controls.Add(this.button_Run_CSharp);
            this.Controls.Add(this.listBox_CSharp);
            this.Controls.Add(this.listBox_Java);
            this.Controls.Add(this.button_Test_File_Four);
            this.Controls.Add(this.button_Test_Three_File);
            this.Controls.Add(this.button_Test_Two_File);
            this.Controls.Add(this.button_Test_One_Java);
            this.Controls.Add(this.button_compile_java2csharp);
            this.Controls.Add(this.button_LoadJavaFile);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_LoadJavaFile;
        private System.Windows.Forms.Button button_compile_java2csharp;
        private System.Windows.Forms.Button button_Test_One_Java;
        private System.Windows.Forms.Button button_Test_Two_File;
        private System.Windows.Forms.Button button_Test_Three_File;
        private System.Windows.Forms.Button button_Test_File_Four;
        private System.Windows.Forms.ListBox listBox_Java;
        private System.Windows.Forms.ListBox listBox_CSharp;
        private System.Windows.Forms.Button button_Run_CSharp;
        private System.Windows.Forms.ListBox listBox_errors;
    }
}

