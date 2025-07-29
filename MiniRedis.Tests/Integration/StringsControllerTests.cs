using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniRedis.Tests.Integration
{
    public class StringsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public StringsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostSet_ShouldStoreValue()
        {
            var key = "foo";
            var value = "bar";

            var setResponse = await _client.PostAsJsonAsync($"/strings/{key}", new { value });
            setResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/strings/{key}");
            var result = await getResponse.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal(value, result?.result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveKey()
        {
            var key = "deleteme";
            var value = "to-delete";

            await _client.PostAsJsonAsync($"/strings/{key}", new { value });

            var deleteResponse = await _client.DeleteAsync($"/strings/{key}");
            var result = await deleteResponse.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.True(deleteResponse.IsSuccessStatusCode);
            Assert.Equal("OK", result?.result);

            var getResponse = await _client.GetAsync($"/strings/{key}");
            var getResult = await getResponse.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("(nil)", getResult?.result);
        }

        [Fact]
        public async Task Set_ShouldFailWithInvalidCharacters()
        {
            var key = "foo!";
            var body = new { value = "bar@" };

            var response = await _client.PostAsJsonAsync($"/strings/{key}", body);
            var result = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("Invalid characters", result);
        }
        [Fact]
        public async Task Set_ShouldFailWithEmptyValue()
        {
            var response = await _client.PostAsJsonAsync("/strings/emptyvalue", new { value = "" });
            var result = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("Key or value missing", result);
        }

        [Fact]
        public async Task Set_ShouldFailWithMissingValue()
        {
            var content = JsonContent.Create(new { }); // Sin 'value'
            var response = await _client.PostAsync("/strings/missingvalue", content);
            var result = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("Key or value missing", result);
        }

        [Fact]
        public async Task Get_ShouldReturnErrorForInvalidKey()
        {
            var response = await _client.GetAsync("/strings/invalid$key");
            var result = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("Invalid key", result);
        }

        [Fact]
        public async Task Delete_ShouldReturnErrorForInvalidKey()
        {
            var response = await _client.DeleteAsync("/strings/invalid@key");
            var result = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("Invalid key", result);
        }

        [Fact]
        public async Task Incr_ShouldHandleConcurrency()
        {
            await _client.PostAsJsonAsync("/strings/concurrent", new { value = "0" });

            var tasks = Enumerable.Range(0, 10).Select(_ =>
                _client.PostAsync("/strings/concurrent/increment", null)
            );

            await Task.WhenAll(tasks);

            var getResponse = await _client.GetAsync("/strings/concurrent");
            var result = await getResponse.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("10", result?.result);
        }

        [Fact]
        public async Task SetWithTtl_ShouldExpireAfterTime()
        {
            await _client.PostAsJsonAsync("/strings/temp", new { value = "short", ttlSeconds = 1 });
            await Task.Delay(1500);

            var getResponse = await _client.GetAsync("/strings/temp");
            var result = await getResponse.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("(nil)", result?.result);
        }

        [Fact]
        public async Task Incr_ShouldIncreaseValue()
        {
            await _client.PostAsJsonAsync("/strings/counter", new { value = "1" });
            var incrResponse = await _client.PostAsync("/strings/counter/increment", null);
            var result = await incrResponse.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("2", result?.result);
        }

        private class GenericResponse
        {
            public string? result { get; set; }
        }
    }
}