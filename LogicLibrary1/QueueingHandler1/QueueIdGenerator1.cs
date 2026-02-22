using LogicLibrary1.QueryHandler1;
using LogicLibrary1.QueueingHandler1.Interfaces;
using static LogicLibrary1.Models1.Constants1;

namespace LogicLibrary1.QueueingHandler1;

public sealed class QueueIdGenerator1 : IQueueIdGenerator1
{
    public string GetNext(QueueService service)
    {
        var prefix = service switch
        {
            QueueService.Consultation => "CST",
            QueueService.Enroll => "ERL",
            QueueService.Admission => "ADS",
            _ => throw new ArgumentOutOfRangeException(nameof(service))
        };

        var (_, worksheet) = ExcelDb1.GetExcelDb("QueueDatabase.xlsx");
        var range = worksheet.RangeUsed();

        var max = 0;

        if (range is not null)
        {
            foreach (var row in range.RowsUsed().Skip(1))
            {
                var qid = row.Cell(2).GetString(); 

                if (!qid.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase))
                    continue;

                var parts = qid.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;

                if (int.TryParse(parts[1], out var n) && n > max)
                    max = n;
            }
        }

        var next = max + 1;
        return $"{prefix}-{next:D4}";
    }
}