namespace TalkBack.Interfaces;

public interface IModelResponse
{
    string? Response { get; }
    IConversationContext? Context { get; }
}
