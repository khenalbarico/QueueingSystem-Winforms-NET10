using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.Models1;
using LogicLibrary1.QueryHandler1;
using static LogicLibrary1.Models1.Constants1;

namespace LogicLibrary1.AdmCntlrHandler1;

public class QueueController1(ICurrentUser1 _currentUser)
{
    private static readonly object _lock = new();

    public Task<bool> MoveToProcessingAsync(string queueId)
        => SetStatusByAdminAsync(queueId, Status.Processing);

    public Task<bool> MoveToCompleteAsync(string queueId)
        => SetStatusByAdminAsync(queueId, Status.Completed);

    public Task<bool> SetBackAsync(string queueId, Status newStatus)
    {
        if (newStatus is not (Status.Pending or Status.Processing))
            throw new InvalidOperationException("SetBackAsync only allows Pending or Processing.");

        return SetStatusByAdminAsync(queueId, newStatus);
    }

    private async Task<bool> SetStatusByAdminAsync(string queueId, Status newStatus)
    {
        EnsureAdmin();

        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var (workbook, worksheet) = ExcelDb1.GetExcelDb("QueueDatabase.xlsx");
                var range = worksheet.RangeUsed()
                    ?? throw new InvalidOperationException("Queue database is empty.");

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    var existingQueueId = row.Cell(2).GetString();

                    if (!existingQueueId.Equals(queueId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    row.Cell(4).Value = newStatus.ToString(); 
                    workbook.Save();
                    return true;
                }

                throw new InvalidOperationException($"Queue '{queueId}' not found.");
            }
        });
    }

    private void EnsureAdmin()
    {
        var user = _currentUser.User ?? throw new UnauthorizedAccessException("No user is logged in.");

        if (user.UserRole != UserRole.Admin)
            throw new UnauthorizedAccessException("Admin access required.");
    }
}