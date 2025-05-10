namespace Bookstore.Mobile.Helpers
{
    // Helper class to prevent multiple rapid executions of an action
    public static class DebounceHelper
    {
        private static CancellationTokenSource _cancellationTokenSource;
        public static async Task Debounce(Func<Task> action, int milliseconds = 300)
        {
            try
            {
                // Cancel previous execution
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                var token = _cancellationTokenSource.Token;
                await Task.Delay(milliseconds, token);

                // If cancellation was not requested, execute the action
                if (!token.IsCancellationRequested)
                {
                    await action();
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, which is expected
            }
            catch (Exception)
            {
                // Handle or rethrow other exceptions as needed
                throw;
            }
        }
    }
}