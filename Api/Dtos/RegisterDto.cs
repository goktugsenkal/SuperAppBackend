namespace Api.Dtos;

public class RegisterDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string FullName { get; set; } = string.Empty;
}