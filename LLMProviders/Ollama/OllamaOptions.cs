using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Ollama;

public class OllamaOptions : IModelOptions 
{
    /// <summary>
    ///  Model parameters: https://github.com/jmorganca/ollama/blob/main/docs/modelfile.md#valid-parameters-and-values
    /// </summary>

    // Required options
    public string? ServerUrl { get; set; }
    public string? Model { get; set; }

    // Optional options
    public int? Seed { get; set; }
    public int? NumPredict { get; set; }
    public int? TopK { get; set; }
    public float? TopP { get; set; }
    public float? Temperature { get; set; }
    public float? RepeatPenalty { get; set; }
    public float? PresencePenalty { get; set; }
    public float? FrequencyPenalty { get; set; }
}
