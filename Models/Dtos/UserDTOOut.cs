namespace IRacingLeague.Models.Dtos;

public class UserDTOOut
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public static UserDTOOut FromEntity(User user) => new()
    {
        UserId = user.UserId,
        UserName = user.UserName,
        Email = user.Email,
        Role = user.Role
    };
}
