using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RepoParser.Core.Entities;
using RepoParser.Core.Interfaces;

namespace RepoParser.Infrastructure.Services;

public class AstParserService : IAstParserService
{
    public Task<List<MethodDefinition>> ParseFileAsync(string filePath)
    {
        var language = DetectLanguage(filePath);
        if (language == "C#")
            return Task.FromResult(ParseCSharpFile(filePath));

        // Future: Add tree-sitter support for other languages
        return Task.FromResult(new List<MethodDefinition>());
    }

    public string DetectLanguage(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".cs" => "C#",
            ".js" or ".jsx" => "JavaScript",
            ".ts" or ".tsx" => "TypeScript",
            ".py" => "Python",
            ".go" => "Go",
            ".java" => "Java",
            ".rs" => "Rust",
            _ => "Unknown"
        };
    }

    private List<MethodDefinition> ParseCSharpFile(string filePath)
    {
        var code = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var methods = new List<MethodDefinition>();

        var methodDeclarations = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methodDeclarations)
        {
            var docComment = GetDocComment(method);
            var parameters = string.Join(", ",
                method.ParameterList.Parameters.Select(p =>
                    $"{p.Type} {p.Identifier}"));

            methods.Add(new MethodDefinition
            {
                Name = method.Identifier.Text,
                Signature = method.ToString().Split('{')[0].Trim(),
                Parameters = $"[{parameters}]",
                ReturnType = method.ReturnType.ToString(),
                DocComment = docComment,
                LineStart = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                LineEnd = method.GetLocation().GetLineSpan().EndLinePosition.Line + 1
            });
        }

        return methods;
    }

    private static string GetDocComment(MethodDeclarationSyntax method)
    {
        var trivia = method.GetLeadingTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                        t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                        t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                        t.IsKind(SyntaxKind.MultiLineCommentTrivia));

        var comments = trivia
            .Select(t => t.ToString())
            .Where(s => !string.IsNullOrWhiteSpace(s));

        return string.Join("\n", comments);
    }
}
