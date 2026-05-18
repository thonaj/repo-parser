using RepoParser.Core.Entities;

namespace RepoParser.Core.Interfaces;

public interface IAstParserService
{
    Task<List<MethodDefinition>> ParseFileAsync(string filePath);
    string DetectLanguage(string filePath);
}
