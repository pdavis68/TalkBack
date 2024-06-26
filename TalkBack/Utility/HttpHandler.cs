using System.Net;
using System.Net.Http.Headers;
using TalkBack.Interfaces;

namespace TalkBack.Utility;

public class HttpHandler : IHttpHandler
{
    private readonly HttpClient _client;
    public HttpHandler(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient();
    }

    public HttpRequestHeaders DefaultRequestHeaders
    {
        get
        {
            return _client.DefaultRequestHeaders;
        }
    }

    public HttpResponseMessage Get(string url)
    {
        return GetAsync(url).Result;
    }

    public HttpResponseMessage Post(string url, HttpContent content)
    {
        return PostAsync(url, content).Result;
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await _client.GetAsync(url);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        return await _client.PostAsync(url, content);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, HttpCompletionOption completion)
    {
        return await _client.SendAsync(msg, completion);
    }
}
