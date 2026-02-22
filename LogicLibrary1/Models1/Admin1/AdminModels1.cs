using LogicLibrary1.Models1.Queue1;
using LogicLibrary1.Models1.User1;

namespace LogicLibrary1.Models1.Admin1;

public class AdminModels1
{
    public UserInfoModels1 UserInfo { get; set; } = new UserInfoModels1();
    public QueueModels1 QueueInfo { get; set; } = new QueueModels1();

}
