namespace API.Errors;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
