// using Scriban;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Emit;

// public static class AdapterGenerator<Target> where Target : notnull
// {
//     private static Dictionary<Type, Dictionary<Type, string>> known_code = new();
//     public static string GenerateCodeString(Type source)
//     {
//         var type = typeof(Target);

//         try
//         {
//             return known_code[source][type]; // return IoC.Get<string>("Adapters.Code.Get", source, type);
//         }
//         catch (KeyNotFoundException)
//         {
//             // var lexemes_dictionary = IoC.Get<IDictionary<string, string>>("Adapters.Source.Lexemes", source);

//             var template = Template.Parse(@"public class {{interface_name}}Adapter : {{interface_name}}
//             {
//                 private readonly {{source_type}} _obj;
//                 public {{interface_name}}Adapter({{source_type}} obj)
//                 {
//                     _obj = obj;
//                 }
//                 {{~ for property in properties ~}}
//                     public {{property.type}} {{property.name}} {
//                         {{if property.can_read }} get => ({{property.type}})_obj.properties.Get(""{{property.name | string.capitalize | string.replace ""_"" "" ""}}""); {{~ end}}
//                         {{if property.can_write }} set => _obj.properties.Set(""{{property.name | string.capitalize | string.replace ""_"" "" ""}}"", value); {{~ end}}
//                     }
//                 {{~ end ~}}
//             }");

//             var render = template.Render(
//                 new
//                 {
//                     interface_name = type.Name,
//                     // lexemes = lexemes_dictionary,
//                     source_type = source.Name,
//                     properties = type.GetProperties().Select(a => new
//                     {
//                         can_read = a.CanRead,
//                         can_write = a.CanWrite,
//                         type = a.PropertyType.ToString(),
//                         name = a.Name
//                     }).ToList()
//                 }
//             );

//             try
//             {
//                 known_code[source][type] = render;
//             }
//             catch (KeyNotFoundException)
//             {
//                 known_code[source] = new Dictionary<Type, string>(){
//                     {type, render}
//                 };
//             }
//             // IoC.Get<ICommand>("Adapters.Code.Add", source, type, render).Execute();

//             return render;
//         }
//     }
// }

// https://stackoverflow.com/questions/826398/is-it-possible-to-dynamically-compile-and-execute-c-sharp-code-fragments
// https://gist.githubusercontent.com/fearofcode/3abb41d0b1a60194765f1fdd81da5269/raw/4213f53a8b7d402e18997d78ef3fd32427880627/dynamiccompilation.cs
// https://simeonpilgrim.com/blog/2007/12/04/compiling-and-running-code-at-runtime/

// public static class RuntimeCompiling{
//     public static SyntaxTree GetSyntaxTree(string code){
//         SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

//         return syntaxTree;
//     }

//     public static MetadataReference[] GetMetadata(){
//         MetadataReference[] references = new MetadataReference[]
//         {
//             MetadataReference.CreateFromFile(typeof(object).Assembly.Location), //mscorlib
//             MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
//         };
        
//         List<PortableExecutableReference> References = 
//         AppDomain.CurrentDomain.GetAssemblies()
//         .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
//         .Select(_ => MetadataReference.CreateFromFile(_.Location))
//         // .Concat(new[]
//         // {
//         //     // add your app/lib specifics, e.g.:                      
//         //     // MetadataReference.CreateFromFile(typeof(MyType).Assembly.Location),
//         // })
//         .ToList();

        
//     var assemblies = new [] 
//         {
//             /*Used for the GeneratedCode attribute*/
//             typeof(System.CodeDom.Compiler.CodeCompiler).Assembly,              //System.CodeDom.Compiler
//         };

//     var refs = from a in assemblies 
//                 select new MetadataFileReference(a.Location);
//     var returnList = refs.ToList();

//     //The location of the .NET assemblies
//     var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

//         /* 
//             * Adding some necessary .NET assemblies
//             * These assemblies couldn't be loaded correctly via the same construction as above,
//             * in specific the System.Runtime.
//             */
//         returnList.Add(new MetadataFileReference(Path.Combine(assemblyPath, "mscorlib.dll")));
//     returnList.Add(new MetadataFileReference(Path.Combine(assemblyPath, "System.dll")));
//     returnList.Add(new MetadataFileReference(Path.Combine(assemblyPath, "System.Core.dll")));
//     returnList.Add(new MetadataFileReference(Path.Combine(assemblyPath, "System.Runtime.dll")));



//         return references;
//     }

//     static void Main()
//     {
//         // define other necessary objects for compilation
//         string assemblyName = Path.GetRandomFileName();
        

//         // analyse and generate IL code from syntax tree
//         CSharpCompilation compilation = CSharpCompilation.Create(
//             assemblyName,
//             syntaxTrees: new[] { syntaxTree },
//             references: references,
//             options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

//         using (var ms = new MemoryStream())
//         {
//             // write IL code into memory
//             EmitResult result = compilation.Emit(ms);

//             if (!result.Success)
//             {
//                 // handle exceptions
//                 IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
//                     diagnostic.IsWarningAsError ||
//                     diagnostic.Severity == DiagnosticSeverity.Error);

//                 foreach (Diagnostic diagnostic in failures)
//                 {
//                     Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
//                 }
//             }
//             else
//             {
//                 // load this 'virtual' DLL so that we can use
//                 ms.Seek(0, SeekOrigin.Begin);
//                 Assembly assembly = Assembly.Load(ms.ToArray());

//                 // create instance of the desired class and call the desired function
//                 Type type = assembly.GetType("RoslynCompileSample.Writer")!;
//                 object obj = Activator.CreateInstance(type);
//                 type.InvokeMember("Write",
//                     BindingFlags.Default | BindingFlags.InvokeMethod,
//                     null,
//                     obj,
//                     new object[] { "Hello World" });
//             }
//         }

//         Console.ReadLine();
//     }
// }
