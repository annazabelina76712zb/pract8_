namespace pract_8.Models;

public partial class User
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}
