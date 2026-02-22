using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.Models1.User1;

namespace LogicLibrary1.AuthHandler1;

public sealed class CurrentUser1 : ICurrentUser1
{
    private UserInfoModels1? _user;

    public bool IsLoggedIn => _user is not null;
    public UserInfoModels1? User => _user;

    public void Set(UserInfoModels1 user)
        => _user = user ?? throw new ArgumentNullException(nameof(user));

    public void Clear() => _user = null;

    public string GetUserIdOrThrow()
        => _user?.UserId ?? throw new UnauthorizedAccessException("No user is logged in.");
}
