using LogicLibrary1.Models1.User1;

namespace LogicLibrary1.AuthHandler1.Interfaces;

public interface ICurrentUser1
{
    bool IsLoggedIn { get; }
    UserInfoModels1? User { get; }

    void Set(UserInfoModels1 user);
    void Clear();
    string GetUserIdOrThrow();
}
