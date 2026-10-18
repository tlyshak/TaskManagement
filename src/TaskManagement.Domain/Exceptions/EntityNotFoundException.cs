namespace TaskManagement.Domain.Exceptions;

public class EntityNotFoundException(string entityName, int id) : Exception($"{entityName} with id {id} was not found.")
{
    public string EntityName { get; } = entityName;
    public int Id { get; } = id;
}
