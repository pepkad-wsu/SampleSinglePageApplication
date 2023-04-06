﻿namespace SampleSinglePageApplication;
public partial class DataAccess
{
    /// <summary>
    /// A function used to dynamically execute a block of C# code.
    /// </summary>
    /// <param name="code">The string that contains the code to be executed.</param>
    /// <param name="objects">An array of objects to pass in to the function in the code.</param>
    /// <param name="additionalAssemblies">A list of additional assemblies to load.</param>
    /// <param name="Namespace">The namespace in which the class and invokerFunction reside.</param>
    /// <param name="Classname">The class name in which the invokerFunction resides.</param>
    /// <param name="invokerFunction">The name of the function to invoke from the code.</param>
    /// <returns>An object of type T.</returns>
    public T? ExecuteDynamicCSharpCode<T>(string code, object[] objects, List<string>? additionalAssemblies, string Namespace, string Classname, string invokerFunction)
    {
        T? output = default(T);

        try {
            // Load all references required by the HelpDesk data project to use the DataAccess library.
            // First, get the base .NET6 references from the Basic.Reference.Assemblies package by jaredpar (https://github.com/jaredpar/basic-reference-assemblies)
            var references = Basic.Reference.Assemblies.Net70.References.All.ToList();

            // Then, add our specific references not covered by the base package.
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(DataAccess).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(DataObjects.BooleanResponse).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(SampleSinglePageApplication.EFModels.EFModels.User).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(JWT.JwtEncoder).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(JWT.Algorithms.RS256Algorithm).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(JWT.Serializers.JsonNetSerializer).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpClient).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Net.Http.HttpClientHandler).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Net.Http.Formatting.FormDataCollection).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.ConstructorHandling).Assembly.Location)); } catch { }
            try { references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.Converters.BinaryConverter).Assembly.Location)); } catch { }

            // Add any user-specified references
            if (additionalAssemblies != null && additionalAssemblies.Any()) {
                foreach (var assembly in additionalAssemblies) {
                    references.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(assembly));
                }
            }

            // Create the C# Syntax Tree from the code block passed in.
            Microsoft.CodeAnalysis.SyntaxTree syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);

            // Create a compiler instance.
            Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                Path.GetRandomFileName(),
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary)
            );

            using (var ms = new MemoryStream()) {
                Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(ms);

                if (result.Success) {
                    ms.Seek(0, SeekOrigin.Begin);
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(ms.ToArray());

                    var type = assembly.GetType(Namespace + "." + Classname);
                    if (type != null) {
                        var obj = Activator.CreateInstance(type);

                        // Invoke the function and pass in the necessary objects required by that method
                        var r = type.InvokeMember(invokerFunction,
                            System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                            null,
                            obj,
                            objects);

                        // See if we received valid results.
                        if (r != null) {
                            output = (T)r;
                        }
                    }
                } else {
                    IEnumerable<Microsoft.CodeAnalysis.Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

                    foreach (Microsoft.CodeAnalysis.Diagnostic diagnostic in failures) {
                        Console.WriteLine(diagnostic.Id.ToString() + ": " + diagnostic.GetMessage());
                    }
                }
            }
        } catch (Exception ex) {
            // When an error is encountered the output object will be null, so create and return a new object.
            //return new DataObjects.DynamicWorkflowResponse { Message = ex.Message };
            Console.WriteLine("Exception: " + ex.Message);
        }

        return output;
    }
}