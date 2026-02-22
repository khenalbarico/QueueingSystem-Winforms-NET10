using LogicLibrary1.Models1.Auth1;
using static LogicLibrary1.Models1.Constants1;

namespace LogicLibrary1.Models1.User1;

public class UserInfoModels1 : LoginModels1
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public int PhoneNumber { get; set; }

    public UserRole UserRole = UserRole.Member;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
