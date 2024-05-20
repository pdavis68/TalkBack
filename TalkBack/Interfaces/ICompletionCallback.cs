namespace TalkBack.Interfaces;

public interface ICompletionCallback
{
    void Complete(string provider, string options, string prompt, string response);
}
