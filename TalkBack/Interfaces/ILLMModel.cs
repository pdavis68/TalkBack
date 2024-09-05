namespace TalkBack.Interfaces;

public interface ILLMModel
{
    string? Name { get; }
    string? Description { get; }
    string? Owner { get; }
    int ContextWindow { get; }
    bool SupportsImages { get; }
}
