using RepoParser.Core.Enums;
using RepoParser.Core.Interfaces;

namespace RepoParser.Infrastructure.Services;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly HashSet<string> _pendingEvents = new();
    private readonly Timer? _debounceTimer;
    private const int DebounceMs = 500;

    public event EventHandler<(string FilePath, FileChangeType ChangeType)>? FileChanged;

    public FileWatcherService()
    {
        _debounceTimer = new Timer(DebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void StartWatching(string directoryPath)
    {
        StopWatching();

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        _watcher = new FileSystemWatcher(directoryPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            Filter = "*.*"
        };

        _watcher.Created += (_, e) => QueueEvent(e.FullPath, FileChangeType.Created);
        _watcher.Changed += (_, e) => QueueEvent(e.FullPath, FileChangeType.Modified);
        _watcher.Deleted += (_, e) => QueueEvent(e.FullPath, FileChangeType.Deleted);
        _watcher.Renamed += (_, e) =>
        {
            QueueEvent(e.OldFullPath, FileChangeType.Deleted);
            QueueEvent(e.FullPath, FileChangeType.Created);
        };

        _watcher.EnableRaisingEvents = true;
    }

    public void StopWatching()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    private void QueueEvent(string filePath, FileChangeType changeType)
    {
        lock (_pendingEvents)
        {
            _pendingEvents.Add($"{filePath}:{changeType}");
            _debounceTimer?.Change(DebounceMs, Timeout.Infinite);
        }
    }

    private void DebounceElapsed(object? state)
    {
        string[] events;
        lock (_pendingEvents)
        {
            events = _pendingEvents.ToArray();
            _pendingEvents.Clear();
        }

        foreach (var eventStr in events)
        {
            var parts = eventStr.Split(':');
            if (parts.Length == 2 && Enum.TryParse<FileChangeType>(parts[1], out var changeType))
            {
                FileChanged?.Invoke(this, (parts[0], changeType));
            }
        }
    }

    public void Dispose()
    {
        StopWatching();
        _debounceTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
