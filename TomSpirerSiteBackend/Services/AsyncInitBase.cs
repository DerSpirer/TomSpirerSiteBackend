namespace TomSpirerSiteBackend.Services;

public abstract class AsyncInitBase(ILogger<AsyncInitBase> logger) : IAsyncDisposable, IDisposable
{
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private readonly ILogger<AsyncInitBase> _logger = logger;
    private bool _isInitialized = false;

    protected abstract Task InitAsync();

    public async Task AwaitInitAsync()
    {
        if (_isInitialized)
            return;

        _logger.LogDebug("Waiting for init semaphore...");
        await _initSemaphore.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                _logger.LogDebug("Already initialized, skipping...");
                return;
            }

            _logger.LogDebug("Init semaphore acquired, calling InitAsync...");
            await InitAsync();
            _isInitialized = true;
            _logger.LogDebug("InitAsync completed successfully");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error initializing {GetType().Name}");
            throw;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    public virtual void Dispose()
    {
        _initSemaphore.Dispose();
    }
    public virtual async ValueTask DisposeAsync()
    {
        _initSemaphore.Dispose();
        await Task.CompletedTask;
    }
}