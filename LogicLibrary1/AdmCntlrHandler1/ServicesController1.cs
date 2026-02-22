using ClosedXML.Excel;
using LogicLibrary1.Models1.Services.Consultation;
using LogicLibrary1.QueryHandler1;

namespace LogicLibrary1.AdmCntlrHandler1;

public class ServicesController1
{
    private static readonly object _lock = new();

    public async Task<List<ProffesorModels1>> LoadProfessorsAsync()
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var result = new List<ProffesorModels1>();

                var (_, worksheet) = ExcelDb1.GetExcelDb("TeachersDatabase.xlsx");
                var range = worksheet.RangeUsed();

                if (range is null)
                    return result;

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    result.Add(new ProffesorModels1
                    {
                        UserId = row.Cell(1).GetString(),
                        FirstName = row.Cell(2).GetString(),
                        LastName = row.Cell(3).GetString(),
                        Subject = row.Cell(4).GetString()
                    });
                }

                return result;
            }
        });
    }

    public async Task<bool> AddProfessorAsync(ProffesorModels1 payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var (workbook, worksheet) = ExcelDb1.GetExcelDb("TeachersDatabase.xlsx");
                EnsureHeader(worksheet);

                var range = worksheet.RangeUsed();
                if (range is not null)
                {
                    foreach (var row in range.RowsUsed().Skip(1))
                    {
                        var fn = row.Cell(2).GetString();
                        var ln = row.Cell(3).GetString();
                        var subj = row.Cell(4).GetString();

                        if (fn.Equals(payload.FirstName, StringComparison.OrdinalIgnoreCase) &&
                            ln.Equals(payload.LastName, StringComparison.OrdinalIgnoreCase) &&
                            subj.Equals(payload.Subject, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("This professor already exists in the database.");
                        }
                    }
                }

                var generatedUserId = GenerateTeacherUserId(worksheet, payload.FirstName, payload.LastName, payload.Subject);

                var insertRow = worksheet.LastRowUsed()!.RowNumber() + 1;

                worksheet.Cell(insertRow, 1).Value = generatedUserId;
                worksheet.Cell(insertRow, 2).Value = payload.FirstName.Trim();
                worksheet.Cell(insertRow, 3).Value = payload.LastName.Trim();
                worksheet.Cell(insertRow, 4).Value = payload.Subject.Trim();

                workbook.Save();

                return true;
            }
        });
    }

    public async Task<bool> DeleteProfessorAsync(string teacherUserId)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var (workbook, worksheet) = ExcelDb1.GetExcelDb("TeachersDatabase.xlsx");
                var range = worksheet.RangeUsed()
                    ?? throw new InvalidOperationException("Teachers database is empty.");

                foreach (var row in range.RowsUsed().Skip(1))
                {
                    var existingUserId = row.Cell(1).GetString();
                    if (existingUserId.Equals(teacherUserId, StringComparison.OrdinalIgnoreCase))
                    {
                        row.Delete();
                        workbook.Save();
                        return true;
                    }
                }

                throw new InvalidOperationException($"Teacher '{teacherUserId}' not found.");
            }
        });
    }

    private static void EnsureHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "UserId";
        worksheet.Cell(1, 2).Value = "FirstName";
        worksheet.Cell(1, 3).Value = "LastName";
        worksheet.Cell(1, 4).Value = "Subject";
    }

    private static string GenerateTeacherUserId(IXLWorksheet worksheet, string firstName, string lastName, string subject)
    {
        var fi = char.ToUpperInvariant(firstName.Trim()[0]);
        var li = char.ToUpperInvariant(lastName.Trim()[0]);

        var initials = $"{fi}{li}";
        var subjectCode = ToSubjectCode(subject); 

        var maxN = 0;

        var range = worksheet.RangeUsed();
        if (range is not null)
        {
            foreach (var row in range.RowsUsed().Skip(1))
            {
                var existingId = row.Cell(1).GetString(); 
                if (string.IsNullOrWhiteSpace(existingId)) continue;

                if (!existingId.StartsWith(initials, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dashIndex = existingId.IndexOf('-');
                if (dashIndex <= 0) continue;

                var left = existingId.Substring(0, dashIndex); 
                var numberPart = left.Substring(initials.Length); 

                if (int.TryParse(numberPart, out var n) && n > maxN)
                    maxN = n;
            }
        }

        var nextN = maxN + 1;
        return $"{initials}{nextN}-{subjectCode}";
    }

    private static string ToSubjectCode(string subject)
    {
        var s = subject.Trim();
        var cleaned = new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (cleaned.Length >= 4) return cleaned.Substring(0, 4);

        return cleaned.PadRight(4, 'X'); 
    }
}