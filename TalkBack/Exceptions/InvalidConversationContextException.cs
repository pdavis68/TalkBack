namespace TalkBack.Exceptions;

[Serializable]
public class InvalidConversationContextException : Exception
{
	public InvalidConversationContextException() { }
	public InvalidConversationContextException(string message) : base(message) { }
	public InvalidConversationContextException(string message, Exception inner) : base(message, inner) { }

}