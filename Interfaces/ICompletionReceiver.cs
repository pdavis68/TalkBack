namespace TalkBack.Interfaces;

public interface ICompletionReceiver
{
    public Task ReceiveCompletionPartAsync(IModelResponse response, bool final);
}