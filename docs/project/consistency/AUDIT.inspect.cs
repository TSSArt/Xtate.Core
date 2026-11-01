using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var root = Path.GetFullPath(args[0]);
var inventoryPath = Path.GetFullPath(args[1]);
var outputPath = Path.GetFullPath(args[2]);
using var inventory = JsonDocument.Parse(File.ReadAllText(inventoryPath));
var files = new List<object>();
var variants = new Dictionary<string, string[]>
{
    ["net10"] = ["NET", "NET10_0", "NETCOREAPP", "NETCOREAPP3_0_OR_GREATER", "NET5_0_OR_GREATER", "NET6_0_OR_GREATER", "NET7_0_OR_GREATER", "NET8_0_OR_GREATER", "NET9_0_OR_GREATER", "NET10_0_OR_GREATER", "DEBUG", "TRACE"],
    ["net462"] = ["NETFRAMEWORK", "NET462", "NET462_OR_GREATER", "DEBUG", "TRACE"]
};
CSharpCompilation? compilation = null;
var semanticTrees = new Dictionary<string, SyntaxTree>(StringComparer.OrdinalIgnoreCase);
if (args.Length > 3)
{
    using var buildInputs = JsonDocument.Parse(File.ReadAllText(args[3]));
    var items = buildInputs.RootElement.GetProperty("Items");
    var projectDirectory = Path.Combine(root, "src", "Xtate.Core");
    foreach (var input in items.GetProperty("Compile").EnumerateArray())
    {
        var path = Path.GetFullPath(input.GetProperty("Identity").GetString()!, projectDirectory);
        if (!File.Exists(path)) continue;
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), new CSharpParseOptions(LanguageVersion.Preview, DocumentationMode.Parse, preprocessorSymbols: variants["net10"]), path);
        semanticTrees[path] = tree;
    }
    var references = items.GetProperty("ReferencePath").EnumerateArray().Select(x => MetadataReference.CreateFromFile(x.GetProperty("Identity").GetString()!)).ToArray();
    compilation = CSharpCompilation.Create("Xtate.Core", semanticTrees.Values, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable));
}
foreach (var entry in inventory.RootElement.GetProperty("files").EnumerateArray())
{
    var path = entry.GetProperty("path").GetString()!;
    if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;
    var content = File.ReadAllText(Path.Combine(root, path));
    var parsedVariants = new List<object>();
    foreach (var (name, symbols) in variants)
    {
        var tree = CSharpSyntaxTree.ParseText(content, new CSharpParseOptions(LanguageVersion.Preview, DocumentationMode.Parse, preprocessorSymbols: symbols), path);
        var syntax = tree.GetRoot();
        int Line(SyntaxNode n) => tree.GetLineSpan(n.Span).StartLinePosition.Line + 1;
        int End(SyntaxNode n) => tree.GetLineSpan(n.Span).EndLinePosition.Line + 1;
        string Ns(SyntaxNode n) => string.Join(".", n.AncestorsAndSelf().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(x => x.Name.ToString()));
        string Owner(SyntaxNode n) => string.Join(".", n.Ancestors().OfType<TypeDeclarationSyntax>().Reverse().Select(x => x.Identifier.ValueText));
        bool Visible(SyntaxNode n)
        {
            foreach (var ancestor in n.AncestorsAndSelf().Where(x => x is BaseTypeDeclarationSyntax or MemberDeclarationSyntax))
            {
                if (ancestor is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax) continue;
                var tokens = ancestor.ChildTokens().Where(t => t.IsKind(SyntaxKind.PublicKeyword) || t.IsKind(SyntaxKind.ProtectedKeyword) || t.IsKind(SyntaxKind.InternalKeyword) || t.IsKind(SyntaxKind.PrivateKeyword)).Select(t => t.ValueText).ToArray();
                if (tokens.Contains("private")) return false;
                if (tokens.Contains("internal") && !tokens.Contains("protected")) return false;
                if (tokens.Contains("public") || tokens.Contains("protected")) continue;
                if (ancestor.Parent is InterfaceDeclarationSyntax || ancestor is EnumMemberDeclarationSyntax) continue;
                if (ancestor is GlobalStatementSyntax) return false;
                if (ancestor is BaseTypeDeclarationSyntax || ancestor is DelegateDeclarationSyntax) return false;
                if (ancestor is MemberDeclarationSyntax && ancestor is not NamespaceDeclarationSyntax && ancestor is not FileScopedNamespaceDeclarationSyntax) return false;
            }
            return true;
        }
        var types = syntax.DescendantNodes().Where(n => n is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax).Select(n => new
        {
            kind = n.Kind().ToString(), name = n switch { BaseTypeDeclarationSyntax t => t.Identifier.ValueText, DelegateDeclarationSyntax d => d.Identifier.ValueText, _ => "" },
            ns = Ns(n), owner = Owner(n), line = Line(n), end_line = End(n), visible = Visible(n),
            modifiers = string.Join(" ", n.ChildTokens().Where(t => SyntaxFacts.IsKeywordKind(t.Kind())).Select(t => t.ValueText)),
            bases = (n as BaseTypeDeclarationSyntax)?.BaseList?.ToString(),
            documentation = n.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
        }).ToArray();
        var members = syntax.DescendantNodes().OfType<MemberDeclarationSyntax>().Where(n => n is BaseMethodDeclarationSyntax or PropertyDeclarationSyntax or FieldDeclarationSyntax or EventDeclarationSyntax or EventFieldDeclarationSyntax or IndexerDeclarationSyntax or EnumMemberDeclarationSyntax).Select(n => new
        {
            kind = n.Kind().ToString(), line = Line(n), end_line = End(n), owner = Owner(n), visible = Visible(n),
            signature = n switch
            {
                BaseMethodDeclarationSyntax m => m.WithBody(null).WithExpressionBody(null).ToString().Trim(),
                PropertyDeclarationSyntax p => p.WithAccessorList(null).WithExpressionBody(null).WithInitializer(null).ToString().Trim(),
                FieldDeclarationSyntax f => string.Join(" ", f.Modifiers) + " " + f.Declaration.Type + " " + string.Join(",", f.Declaration.Variables.Select(v => v.Identifier.ValueText)),
                EventFieldDeclarationSyntax e => e.Declaration.Type + " " + string.Join(",", e.Declaration.Variables.Select(v => v.Identifier.ValueText)),
                _ => n.ToString().Split('\n')[0].Trim()
            },
            names = n switch
            {
                MethodDeclarationSyntax m => new[] { m.Identifier.ValueText }, ConstructorDeclarationSyntax c => new[] { c.Identifier.ValueText },
                PropertyDeclarationSyntax p => new[] { p.Identifier.ValueText }, FieldDeclarationSyntax f => f.Declaration.Variables.Select(v => v.Identifier.ValueText).ToArray(),
                EventDeclarationSyntax e => new[] { e.Identifier.ValueText }, EventFieldDeclarationSyntax e => e.Declaration.Variables.Select(v => v.Identifier.ValueText).ToArray(),
                EnumMemberDeclarationSyntax e => new[] { e.Identifier.ValueText }, _ => Array.Empty<string>()
            },
            modifiers = n.ChildTokens().Where(t => SyntaxFacts.IsKeywordKind(t.Kind())).Select(t => t.ValueText).ToArray(),
            documentation = n.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)).Select(t => t.ToFullString()).ToArray(),
            attributes = n.AttributeLists.Select(a => a.ToString()).ToArray()
        }).ToArray();
        var invocations = syntax.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(n => new { line = Line(n), expression = n.ToString(), discarded_statement = n.Parent is ExpressionStatementSyntax, owner = Owner(n), in_async = n.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault()?.Modifiers.Any(SyntaxKind.AsyncKeyword) == true }).ToArray();
        var awaits = syntax.DescendantNodes().OfType<AwaitExpressionSyntax>().Select(n => new { line = Line(n), expression = n.Expression.ToString() }).ToArray();
        var usings = syntax.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(n => new { line = Line(n), text = n.ToString(), name = n.Name?.ToString(), alias = n.Alias?.Name.ToString(), global = n.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) }).ToArray();
        var parameters = syntax.DescendantNodes().OfType<ParameterSyntax>().Select(n => new { line = Line(n), name = n.Identifier.ValueText, type = n.Type?.ToString(), owner = Owner(n) }).ToArray();
        var comments = syntax.DescendantTrivia(descendIntoTrivia: true).Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia) || t.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia)).Select(t => new { line = tree.GetLineSpan(t.Span).StartLinePosition.Line + 1, text = t.ToString() }).ToArray();
        object[] semanticCalls = [];
        object[] publicApi = [];
        string[] dependencyNamespaces = [];
        if (name == "net10" && compilation is not null && semanticTrees.TryGetValue(Path.GetFullPath(Path.Combine(root, path)), out var semanticTree))
        {
            var semanticRoot = semanticTree.GetRoot();
            var model = compilation.GetSemanticModel(semanticTree);
            semanticCalls = semanticRoot.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(n => (object)new
            {
                line = semanticTree.GetLineSpan(n.Span).StartLinePosition.Line + 1,
                expression = n.ToString(),
                result_type = model.GetTypeInfo(n).Type?.ToDisplayString(),
                symbol = model.GetSymbolInfo(n).Symbol?.ToDisplayString(),
                discarded_statement = n.Parent is ExpressionStatementSyntax,
                discarded_assignment = n.Parent is AssignmentExpressionSyntax assignment && assignment.Left.ToString() == "_"
            }).ToArray();
            bool ExternallyVisible(ISymbol symbol)
            {
                for (ISymbol? current = symbol; current is not null && current is not INamespaceSymbol && current is not IAssemblySymbol; current = current.ContainingType)
                {
                    if (current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)) return false;
                }
                return true;
            }
            publicApi = semanticRoot.DescendantNodes().Where(n => n is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or BaseMethodDeclarationSyntax or PropertyDeclarationSyntax or EventDeclarationSyntax or IndexerDeclarationSyntax or EnumMemberDeclarationSyntax || n is VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax or EventFieldDeclarationSyntax })
                .Select(n => (node: n, symbol: model.GetDeclaredSymbol(n)))
                .Where(x => x.symbol is not null && ExternallyVisible(x.symbol))
                .Select(x => (object)new
                {
                    line = semanticTree.GetLineSpan(x.node.Span).StartLinePosition.Line + 1,
                    kind = x.symbol!.Kind.ToString(), name = x.symbol.Name, symbol = x.symbol.ToDisplayString(), accessibility = x.symbol.DeclaredAccessibility.ToString(),
                    documentation = x.symbol.GetDocumentationCommentXml(),
                    overridden = (x.symbol as IMethodSymbol)?.OverriddenMethod?.ToDisplayString() ?? (x.symbol as IPropertySymbol)?.OverriddenProperty?.ToDisplayString(),
                    attributes = x.symbol.GetAttributes().Select(a => a.AttributeClass?.ToDisplayString()).ToArray()
                }).ToArray();
            dependencyNamespaces = semanticRoot.DescendantNodes().OfType<IdentifierNameSyntax>().Select(n => model.GetSymbolInfo(n).Symbol)
                .Where(s => s is INamedTypeSymbol && s.ContainingNamespace is not null).Select(s => s!.ContainingNamespace.ToDisplayString()).Where(s => s.StartsWith("Xtate", StringComparison.Ordinal)).Distinct().Order().ToArray();
        }
        parsedVariants.Add(new { name, types, members, invocations, awaits, usings, parameters, comments, semantic_calls = semanticCalls, public_api = publicApi, dependency_namespaces = dependencyNamespaces, parse_errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => new { id = d.Id, message = d.GetMessage(), line = d.Location.GetLineSpan().StartLinePosition.Line + 1 }).ToArray() });
    }
    files.Add(new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, path)))), line_count = content.Count(c => c == '\n') + 1, variants = parsedVariants });
}
object? historical = null;
if (File.Exists(outputPath))
{
    using var old = JsonDocument.Parse(File.ReadAllText(outputPath));
    historical = old.RootElement.ValueKind == JsonValueKind.Array ? old.RootElement.Clone() : old.RootElement.TryGetProperty("historical_c47", out var previous) ? previous.Clone() : null;
}
var compilationErrors = compilation?.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => new { id = d.Id, message = d.GetMessage(), path = d.Location.SourceTree?.FilePath, line = d.Location.GetLineSpan().StartLinePosition.Line + 1 }).ToArray();
File.WriteAllText(outputPath, JsonSerializer.Serialize(new { schema_version = 2, purpose = "Declaration/call inventory; conformance and semantic control-flow judgments are recorded separately", method = "Roslyn preview syntax with modern and legacy symbol variants; library net10 symbol binding from MSBuild Compile and ReferencePath; inherited XML resolution requires separate verification", compilation_errors = compilationErrors, historical_c47 = historical, files }, new JsonSerializerOptions { WriteIndented = false }));
Console.WriteLine(JsonSerializer.Serialize(new { files = files.Count, compilation_errors = compilationErrors?.Length, output = outputPath }));
