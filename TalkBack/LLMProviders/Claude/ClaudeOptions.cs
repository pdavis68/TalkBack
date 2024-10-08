﻿using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeOptions : IProviderOptions
{
    /// <summary>
    ///  Model parameters: https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md#valid-parameters-and-values
    /// </summary>

    // Required options
    public string? Model { get; set; }
    public string? AnthropicVersion { get; set; } = "2023-06-01";
    public string? ApiKey { get; set; }
    public string AuthToken { get; set; } = string.Empty;
    public int MaxTokensToSample { get; set; } = 256;


    // Optional options
    public string[]? StopSequences { get; set; }
    public float Temperature { get; set; }
    public float TopP { get; set; }
    public int TopK { get; set; }
}
