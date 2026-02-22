using ClosedXML.Excel;
using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.Models1;
using LogicLibrary1.Models1.Queue1;
using LogicLibrary1.QueryHandler1;
using LogicLibrary1.QueueingHandler1.Interfaces;

namespace LogicLibrary1.QueueingHandler1;

public class Queue1(ICurrentUser1 _currentUser, IQueueIdGenerator1 _idGen)
{
    private static readonly object _lock = new();

    public async Task<List<QueueModels1>> DisplayAllQueuesAsync()
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var result = new List<QueueModels1>();

                var (workbook, worksheet) = ExcelDb1.GetExcelDb("QueueDatabase.xlsx");
                var range = worksheet.RangeUsed();

                if (range is null)
                    return result; 

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    var userId = row.Cell(1).GetString();
                    var queueId = row.Cell(2).GetString();
                    var serviceStr = row.Cell(3).GetString();
                    var statusStr = row.Cell(4).GetString();
                    var createdStr = row.Cell(5).GetString();

                    Enum.TryParse(serviceStr, out Constants1.QueueService service);
                    Enum.TryParse(statusStr, out Constants1.Status status);
                    DateTime.TryParse(createdStr, out var created);

                    result.Add(new QueueModels1
                    {
                        UserId = userId,
                        QueueId = queueId,
                        QueueService = service,
                        Status = status,
                        CreatedAt = created
                    });
                }

                return result;
            }
        });
    }

    public async Task<List<QueueModels1>> DisplayCurrentUserQueuesAsync()
    {
        var currentUserId = _currentUser.GetUserIdOrThrow();

        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var result = new List<QueueModels1>();

                var (_, worksheet) = ExcelDb1.GetExcelDb("QueueDatabase.xlsx");
                var range = worksheet.RangeUsed();

                if (range is null)
                    return result;

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    var userId = row.Cell(1).GetString();

                    if (!userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var queueId = row.Cell(2).GetString();
                    var serviceStr = row.Cell(3).GetString();
                    var statusStr = row.Cell(4).GetString();
                    var createdStr = row.Cell(5).GetString();

                    Enum.TryParse(serviceStr, out Constants1.QueueService service);
                    Enum.TryParse(statusStr, out Constants1.Status status);
                    DateTime.TryParse(createdStr, out var created);

                    result.Add(new QueueModels1
                    {
                        UserId = userId,
                        QueueId = queueId,
                        QueueService = service,
                        Status = status,
                        CreatedAt = created
                    });
                }

                return result;
            }
        });
    }

    public async Task<bool> EnqueueAsync(QueueModels1 payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var userId = _currentUser.GetUserIdOrThrow();

        await Task.Run(() =>
        {
            lock (_lock)
            {
                var (workbook, worksheet) = ExcelDb1.GetExcelDb("QueueDatabase.xlsx");

                EnsureHeader(worksheet);

                var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var existingUserId = row.Cell(1).GetString();
                    var existingService = row.Cell(3).GetString();

                    if (existingUserId.Equals(userId, StringComparison.OrdinalIgnoreCase) &&
                        existingService.Equals(payload.QueueService.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"You are already queued for {payload.QueueService}. You can only queue once per service.");
                    }
                }

                var queueId = _idGen.GetNext(payload.QueueService);
                var insertRow = worksheet.LastRowUsed()!.RowNumber() + 1;

                var now = DateTime.UtcNow;

                worksheet.Cell(insertRow, 1).Value = userId;
                worksheet.Cell(insertRow, 2).Value = queueId;
                worksheet.Cell(insertRow, 3).Value = payload.QueueService.ToString();
                worksheet.Cell(insertRow, 4).Value = payload.Status.ToString();
                worksheet.Cell(insertRow, 5).Value = now.ToString("yyyy/MM/dd h:mm tt");

                workbook.Save();

                payload.QueueId = queueId;
                payload.CreatedAt = now;
            }
        });

        return true;
    }
    public async Task<bool> DequeueAsync(QueueModels1 payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var userId = _currentUser.GetUserIdOrThrow();

        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var (workbook, worksheet) = ExcelDb1.GetExcelDb("QueueDatabase.xlsx");
                var range = worksheet.RangeUsed()
                    ?? throw new InvalidOperationException("Queue database is empty.");

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    var existingUserId = row.Cell(1).GetString(); 
                    var existingQueueId = row.Cell(2).GetString(); 

                    if (existingUserId.Equals(userId, StringComparison.OrdinalIgnoreCase) &&
                        existingQueueId.Equals(payload.QueueId, StringComparison.OrdinalIgnoreCase))
                    {
                        row.Delete();      
                        workbook.Save();
                        return true;
                    }
                }

                throw new InvalidOperationException(
                    $"Queue '{payload.QueueId}' was not found for user '{userId}'.");
            }
        });
    }

    private static void EnsureHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "UserId";
        worksheet.Cell(1, 2).Value = "QueueId";
        worksheet.Cell(1, 3).Value = "QueueService";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "Created";
    }
}