namespace VirtualPM.Services;

public class FileMessageService : IMessageGenerator, IDisposable
{
    private readonly string _filePath;
    private readonly FileSystemWatcher _fileWatcher;
    private readonly Lock _lock = new();
    private List<string> _allMessages = [];
    private readonly HashSet<int> _usedIndices = [];
    private bool _disposed;

    public FileMessageService(IConfiguration configuration)
    {
        _filePath = configuration["VirtualPM:MessagesFilePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "messages.txt");

        LoadMessages();
        string directory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrEmpty(directory))
        {
            directory = Directory.GetCurrentDirectory();
        }
        string fileName = Path.GetFileName(_filePath);

        _fileWatcher = new FileSystemWatcher(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _fileWatcher.Changed += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce multiple change events
        Thread.Sleep(100);

        lock (_lock)
        {
            LoadMessages();
            Console.WriteLine($"Messages file reloaded from: {_filePath}");
        }
    }

    private void LoadMessages()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Messages file not found at: {_filePath}. Creating default file.");
                CreateDefaultMessagesFile();
            }

            string[] lines = File.ReadAllLines(_filePath);
            _allMessages = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();

            if (_allMessages.Count == 0)
            {
                _allMessages = ["You haven't updated your work today. Please do so!"];
            }

            // Reset used indices when messages are reloaded
            _usedIndices.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading messages from file: {ex.Message}");
            _allMessages = ["You haven't updated your work today. Please do so!"];
        }
    }

    private void CreateDefaultMessagesFile()
    {
        string[] defaultMessages =
        [
            "You haven't updated your work today. Please do so!"
        ];

        try
        {
            File.WriteAllLines(_filePath, defaultMessages);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default messages file: {ex.Message}");
        }
    }

    public Task<string> GenerateHumorousMessageAsync()
    {
        lock (_lock)
        {
            if (_allMessages.Count == 0)
            {
                return Task.FromResult("You haven't updated your work today. Please do so!");
            }

            // Reset if all messages have been used
            if (_usedIndices.Count >= _allMessages.Count)
            {
                _usedIndices.Clear();
            }

            // Get list of available indices
            List<int> availableIndices = Enumerable.Range(0, _allMessages.Count)
                .Where(i => !_usedIndices.Contains(i))
                .ToList();

            // Pick a random available message
            int randomIndex = availableIndices[Random.Shared.Next(availableIndices.Count)];
            _usedIndices.Add(randomIndex);

            return Task.FromResult(_allMessages[randomIndex]);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _fileWatcher?.Dispose();
            _disposed = true;
        }
    }
}
