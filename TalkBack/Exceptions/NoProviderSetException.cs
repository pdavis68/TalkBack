namespace TalkBack.Exceptions;

[Serializable]
public class NoProviderSetException : Exception
{
	public NoProviderSetException() { }
	public NoProviderSetException(string message) : base(message) { }
	public NoProviderSetException(string message, Exception inner) : base(message, inner) { }
}