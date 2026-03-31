namespace IdentityService.API.DTOs;

public class UserDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public List<string> Roles { get; set; } = new();
}
