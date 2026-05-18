using RepoParser.Core.Enums;

namespace RepoParser.Core.Interfaces;

public interface IFileWatcherService
{
    event EventHandler<(string FilePath, FileChangeType ChangeType)>? FileChanged;
    void StartWatching(string directoryPath);
    void StopWatching();
}
