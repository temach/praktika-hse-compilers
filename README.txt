=======================================
About the project:
The folder contains a Visual Studio solution with two sub-projects.
Project "java2csharp" is a compiler from java into csharp.
Project "compile_csharp" compiles a any C# file and automatically runs it. (This is just for ease of use, the compile itself does NOT depend on this project.)


=======================================
How to compile:
Open comples_compiler.sln in Visual Studio and hit the "Build" button.


=======================================
About the compiler:
1) Works with a subset of Java language.
2) Supported operators:  =, -, ^, &, |, <, >, <=, >=, !=, ==, *, %, !, ?:, println
3) Complexity: for-each loops, nested class declarations, class property access.
4) Supported types: boolean, char.
5) Also arrays are supported, but only to show that for-each loop works.


=======================================
About the executables:
The executable to translate Java into C# is called "java2csharp.exe"
The executable to compile the C# code into a ".exe" file and run it is called "compile_csharp.exe"


=======================================
About the examples:

Example 1:
java2csharp TestJava1.java Output1.cs
compile_csharp Output1.cs Program1.exe


Example 1:
java2csharp TestJava1.java Output1.cs
compile_csharp Output1.cs Program1.exe


Example 1:
java2csharp TestJava1.java Output1.cs
compile_csharp Output1.cs Program1.exe


=======================================
About other files:
The grammar file is called "MyJava.g4".

