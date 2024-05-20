namespace TalkBack.Interfaces;

public interface IProviderActivator
{
    ILLMProvider? CreateProvider<T>() where T : ILLMProvider;
}
