namespace TalkBack.Exceptions;

[Serializable]
public class InvalidOptionsException : Exception
{
	public InvalidOptionsException() { }
	public InvalidOptionsException(string message) : base(message) { }
	public InvalidOptionsException(string message, Exception inner) : base(message, inner) { }
}