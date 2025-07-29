
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace MiniRedis.Tests.Integration
{
    public class ZSetsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ZSetsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ZAdd_ShouldAddMember()
        {
            var uniqueKey = "myzset_" + Guid.NewGuid().ToString("N");

            var payload = new
            {
                members = new[]
                {
                    new { score = 1.0, member = "a" }
                }
            };

            var response = await _client.PostAsJsonAsync($"/zsets/{uniqueKey}", payload);

            Assert.True(response.IsSuccessStatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response: " + content);

            var result = await response.Content.ReadFromJsonAsync<GenericAddedResponse>();
            Assert.NotNull(result);
            Assert.Equal(1, result.added);
        }

        [Fact]
        public async Task ZCard_ShouldReturnCorrectCount()
        {
            var key = "zcard_" + Guid.NewGuid().ToString("N");

            var payload1 = new
            {
                members = new[] { new { score = 1.0, member = "a" } }
            };
            await _client.PostAsJsonAsync($"/zsets/{key}", payload1);

            var payload2 = new
            {
                members = new[] { new { score = 2.0, member = "b" } }
            };
            await _client.PostAsJsonAsync($"/zsets/{key}", payload2);

            var response = await _client.GetAsync($"/zsets/{key}/count");
            var result = await response.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("2", result?.result);
        }

        [Fact]
        public async Task ZRange_ShouldReturnExpectedRange()
        {
            var payload = new
            {
                members = new[]
                {
                    new { score = 1.0, member = "a" },
                    new { score = 2.0, member = "b" }
                }
            };
            await _client.PostAsJsonAsync("/zsets/rangetest", payload);

            var response = await _client.GetAsync("/zsets/rangetest/range?start=0&stop=1");
            var result = await response.Content.ReadFromJsonAsync<GenericListResponse>();

            Assert.Contains("a", result?.result);
            Assert.Contains("b", result?.result);
        }

        [Fact]
        public async Task ZRange_ShouldReturnAll_WhenStopIsNegativeOne()
        {
            var payload = new
            {
                members = new[]
                {
                    new { score = 1, member = "a" },
                    new { score = 2, member = "b" },
                    new { score = 3, member = "c" }
                }
            };
            await _client.PostAsJsonAsync("/zsets/myzset", payload);

            var response = await _client.GetAsync("/zsets/myzset/range?start=0&stop=-1");
            var content = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("a", content);
            Assert.Contains("b", content);
            Assert.Contains("c", content);
        }

        [Fact]
        public async Task ZAdd_ShouldRejectInvalidCharacters()
        {
            var payload = new
            {
                members = new[]
                {
                    new { score = 1.0, member = "a" }
                }
            };

            var response = await _client.PostAsJsonAsync("/zsets/invalid@key", payload);
            var content = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.Contains("Invalid key", content);
        }

        [Fact]
        public async Task ZRange_ShouldReturnEmptyForInvalidRange()
        {
            var response = await _client.GetAsync("/zsets/empty/range?start=2&stop=1");
            var result = await response.Content.ReadFromJsonAsync<GenericListResponse>();

            Assert.Empty(result?.result ?? new());
        }

        [Fact]
        public async Task ZAdd_ShouldHandleConcurrentAdds()
        {
            var key = "concurrent_" + Guid.NewGuid().ToString("N");

            var tasks = Enumerable.Range(0, 10).Select(i =>
            {
                var payload = new
                {
                    members = new[]
                    {
                        new { score = i, member = $"m{i}" }
                    }
                };
                return _client.PostAsJsonAsync($"/zsets/{key}", payload);
            });

            await Task.WhenAll(tasks);

            var response = await _client.GetAsync($"/zsets/{key}/count");
            var result = await response.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("10", result?.result);
        }

        [Fact]
        public async Task ZRange_ShouldSupportNegativeIndexes()
        {
            var key = "zset_neg_" + Guid.NewGuid().ToString("N");

            var payload = new
            {
                members = new[]
                {
                    new { score = 10.0, member = "a" },
                    new { score = 20.0, member = "b" },
                    new { score = 30.0, member = "c" },
                    new { score = 40.0, member = "d" },
                    new { score = 50.0, member = "e" }
                }
            };

            await _client.PostAsJsonAsync($"/zsets/{key}", payload);

            var response = await _client.GetAsync($"/zsets/{key}/range?start=-2&stop=-1");

            var result = await response.Content.ReadFromJsonAsync<GenericListResponse>();

            Assert.NotNull(result?.result);
            Assert.Equal(new List<string> { "d", "e" }, result.result);
        }

        [Fact]
        public async Task ZRank_ShouldReturnCorrectRank_WhenMemberExists()
        {
            var key = "rank_" + Guid.NewGuid().ToString("N");

            await _client.PostAsJsonAsync($"/zsets/{key}", new { score = 10, member = "alice" });
            await _client.PostAsJsonAsync($"/zsets/{key}", new { score = 20, member = "bob" });
            await _client.PostAsJsonAsync($"/zsets/{key}", new { score = 15, member = "charlie" });

            var response = await _client.GetAsync($"/zsets/{key}/rank/charlie");
            var result = await response.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("1", result?.result); // alice=0, charlie=1, bob=2
        }

        [Fact]
        public async Task ZRank_ShouldReturnZero_WhenMemberIsFirst()
        {
            var key = "first_" + Guid.NewGuid().ToString("N");

            await _client.PostAsJsonAsync($"/zsets/{key}", new { score = 5, member = "first" });
            await _client.PostAsJsonAsync($"/zsets/{key}", new { score = 10, member = "second" });

            var response = await _client.GetAsync($"/zsets/{key}/rank/first");
            var result = await response.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("0", result?.result);
        }

        [Fact]
        public async Task ZRank_ShouldReturnNil_WhenMemberDoesNotExist()
        {
            await _client.PostAsJsonAsync("/zsets/ghost_"+ Guid.NewGuid().ToString("N") + "", new { score = 100, member = "someone" });

            var response = await _client.GetAsync("/zsets/ghost_"+ Guid.NewGuid().ToString("N") + "/rank/missing");
            var result = await response.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("(nil)", result?.result);
        }

        [Fact]
        public async Task ZRank_ShouldReturnNil_WhenKeyDoesNotExist()
        {
            var response = await _client.GetAsync("/zsets/noexist_"+ Guid.NewGuid().ToString("N") + "/rank/anyone");
            var result = await response.Content.ReadFromJsonAsync<GenericResponse>();

            Assert.Equal("(nil)", result?.result);
        }


        private class GenericResponse
        {
            public string? result { get; set; }
        }

        private class GenericListResponse
        {
            public List<string>? result { get; set; }
        }

        private class GenericAddedResponse
        {
            public int added { get; set; }
        }
    }
}