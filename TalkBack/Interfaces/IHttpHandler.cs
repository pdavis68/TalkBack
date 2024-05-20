using System.Net.Http.Headers;

namespace TalkBack.Interfaces;

public interface IHttpHandler
{
    HttpRequestHeaders DefaultRequestHeaders { get; }
    HttpResponseMessage Get(string url);
    HttpResponseMessage Post(string url, HttpContent content);
    Task<HttpResponseMessage> GetAsync(string url);
    Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, HttpCompletionOption competion);
}
