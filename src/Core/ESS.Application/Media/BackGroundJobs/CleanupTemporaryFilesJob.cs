public class CleanupTemporaryFilesJob : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupTemporaryFilesJob> _logger;

    public CleanupTemporaryFilesJob(IServiceScopeFactory scopeFactory, ILogger<CleanupTemporaryFilesJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(Cleanup, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private void Cleanup(object? state)
    {
        using var scope = _scopeFactory.CreateScope();
        var storageProvider = scope.ServiceProvider.GetRequiredService<IStorageProvider>();

        var tempDir = "temp";
        if (Directory.Exists(tempDir))
        {
            foreach (var dir in Directory.GetDirectories(tempDir))
            {
                var createdDate = Directory.GetCreationTimeUtc(dir);
                if ((DateTime.UtcNow - createdDate).TotalHours > 24)
                {
                    Directory.Delete(dir, true);
                    _logger.LogInformation($"Deleted expired temp folder: {dir}");
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
