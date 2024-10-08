﻿using Microsoft.Extensions.DependencyInjection;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Ollama;
using TalkBack.LLMProviders.OpenAI;
using TalkBack.LLMProviders.Claude;
using TalkBack.LLMProviders.Groq;

namespace TalkBack.Utility;


public static class TalkBackServiceRegistration
{
    public static IServiceCollection RegisterTalkBack(this IServiceCollection services)
    {
        // Register dependencies required by TalkBack
        services.AddHttpClient();

        services.AddTransient<IProviderActivator, ProviderActivator>();
        services.AddTransient<ILLM, LLM>();
        services.AddTransient<IHttpHandler, HttpHandler>();

        services.AddTransient(typeof(OllamaProvider));
        services.AddTransient(typeof(OpenAIProvider));
        services.AddTransient(typeof(GroqProvider));
        services.AddTransient(typeof(ClaudeProvider));

        return services;
    }
}

