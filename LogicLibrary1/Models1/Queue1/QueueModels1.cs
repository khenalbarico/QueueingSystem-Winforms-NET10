using static LogicLibrary1.Models1.Constants1;

namespace LogicLibrary1.Models1.Queue1;

public class QueueModels1
{
    public string UserId { get; set; } = string.Empty;
    public string QueueId { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.Pending;
    public QueueService QueueService { get; set; } = QueueService.Enroll;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
