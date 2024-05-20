﻿using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.OpenAI;

public class OpenAIOptions : IModelOptions
{
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
    public float Temperature { get; set; }
    public int MaxTokens { get; set; }
    public float TopP { get; set; }
    public int FrequencyPenalty { get; set; }
    public int PresencePenalty { get; set; }
    public int Stop { get; set; }
    public bool Stream { get; set; }
}