using Microsoft.JSInterop;

namespace BlazorApp1.Helpers
{
    public class CacheStorageAccessor : IAsyncDisposable
    {
        private Lazy<IJSObjectReference> _accessorJsRef = new();
        private readonly IJSRuntime _jsRuntime;

        public CacheStorageAccessor(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        private async Task WaitForReference()
        {
            if (_accessorJsRef.IsValueCreated is false)
            {
                _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/myjs/CacheStorageAccessor.js"));
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_accessorJsRef.IsValueCreated)
            {
                await _accessorJsRef.Value.DisposeAsync();
            }
        }

        public async Task StoreAsync(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
        {
            await WaitForReference();
            string requestMethod = requestMessage.Method.Method;
            string requestBody = await GetRequestBodyAsync(requestMessage);
            string responseBody = await responseMessage.Content.ReadAsStringAsync();

            await _accessorJsRef.Value.InvokeVoidAsync("store", requestMessage.RequestUri, requestMethod, requestBody, responseBody);
        }

        public async Task StoreSqliteAsync(HttpRequestMessage requestMessage, byte[] sqliteBytes)
        {
            await WaitForReference();
            string requestMethod = requestMessage.Method.Method;
            string requestBody = await GetRequestBodyAsync(requestMessage);
           // string responseBody = await responseMessage.Content.ReadAsStringAsync();

            await _accessorJsRef.Value.InvokeVoidAsync("storesqlite", requestMessage.RequestUri, requestMethod, requestBody, sqliteBytes);
        }



        public async Task<string> GetAsync(HttpRequestMessage requestMessage)
        {
            await WaitForReference();
            string requestMethod = requestMessage.Method.Method;
            string requestBody = await GetRequestBodyAsync(requestMessage);
            string result = await _accessorJsRef.Value.InvokeAsync<string>("get", requestMessage.RequestUri, requestMethod, requestBody);

            return result;


        }



        public async Task<byte[]> RetrieveFileFromCache(HttpRequestMessage requestMessage)
        {
            try
            {
                await WaitForReference();

                string requestMethod = requestMessage.Method.Method;
                string requestBody = await GetRequestBodyAsync(requestMessage);

                // Invoke the JavaScript function to retrieve the file as a byte array
                byte[] result = await _accessorJsRef.Value.InvokeAsync<byte[]>("retrieveFileFromCache", requestMessage.RequestUri, requestMethod, requestBody);

                return result;
            }
            catch (Exception ex)
            {
                // Handle and log any exceptions
                Console.WriteLine($"Error retrieving file from cache: {ex.Message}");
                throw;
            }
        }



        public async Task RemoveAsync(HttpRequestMessage requestMessage)
        {
            await WaitForReference();
            string requestMethod = requestMessage.Method.Method;
            string requestBody = await GetRequestBodyAsync(requestMessage);
            await _accessorJsRef.Value.InvokeVoidAsync("remove", requestMessage.RequestUri, requestMethod, requestBody);
        }

        public async Task RemoveAllAsync()
        {
            await WaitForReference();
            await _accessorJsRef.Value.InvokeVoidAsync("removeAll");
        }

        private static async Task<string> GetRequestBodyAsync(HttpRequestMessage requestMessage)
        {
            string requestBody = "";

            if (requestMessage.Content is not null)
            {
                requestBody = await requestMessage.Content.ReadAsStringAsync() ?? "";
            }

            return requestBody;
        }
    }
}
