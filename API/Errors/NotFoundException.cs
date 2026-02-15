namespace API.Errors;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, int id) 
        : base($"{entityName} with ID {id} was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}
