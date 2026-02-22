using LogicLibrary1.Models1;
using static LogicLibrary1.Models1.Constants1;


namespace LogicLibrary1.QueueingHandler1.Interfaces
{
    public interface IQueueIdGenerator1
    {
        string GetNext(QueueService service);
    }

}
