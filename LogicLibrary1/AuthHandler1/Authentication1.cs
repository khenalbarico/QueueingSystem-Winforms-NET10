using ClosedXML.Excel;
using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.Models1;
using LogicLibrary1.Models1.Auth1;
using LogicLibrary1.Models1.User1;
using LogicLibrary1.QueryHandler1;

namespace LogicLibrary1.AuthHandler1;

public class Authentication1(ICurrentUser1 _currentUser)
{
    public async Task<UserInfoModels1> OnLoginAsync(LoginModels1 payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var user = await Task.Run(() =>
        {
            var (workbook, worksheet) = ExcelDb1.GetExcelDb("UsersDatabase.xlsx");
            var range = worksheet.RangeUsed() ?? throw new InvalidOperationException("Database is empty.");
            var rows = range.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var email = row.Cell(2).GetString();
                var password = row.Cell(3).GetString();

                if (email.Equals(payload.Email, StringComparison.OrdinalIgnoreCase)
                    && password == payload.Password)
                {
                    _ = int.TryParse(row.Cell(7).GetString(), out int age);
                    _ = int.TryParse(row.Cell(8).GetString(), out int phone);

                    var createdCell = row.Cell(9);
                    DateTime createdAt =
                        createdCell.DataType == XLDataType.DateTime
                            ? createdCell.GetDateTime()
                            : (DateTime.TryParse(createdCell.GetString(), out var dt) ? dt : default);

                    return new UserInfoModels1
                    {
                        UserId = row.Cell(1).GetString(),
                        Email = email,
                        Password = password,
                        FirstName = row.Cell(5).GetString(),
                        LastName = row.Cell(6).GetString(),
                        Age = age,
                        PhoneNumber = phone,
                        CreatedAt = createdAt,
                        UserRole = Enum.TryParse(row.Cell(4).GetString(), out Constants1.UserRole role)
                            ? role
                            : throw new InvalidOperationException("Invalid user role in database.")
                    };
                }
            }

            throw new UnauthorizedAccessException("Invalid email or password.");
        });

        _currentUser.Set(user);

        return user;
    }

    public async Task<bool> OnCreateNewAccount(UserInfoModels1 payload)
    {
        try
        {
            return await Task.Run(() =>
            {
                var (workbook, worksheet) = ExcelDb1.GetExcelDb("UsersDatabase.xlsx");
                var range = worksheet.RangeUsed() ?? throw new InvalidOperationException("Database is empty.");
                var rows = range.RowsUsed().Skip(1);

                foreach (var row in rows!)
                {
                    var existingEmail = row.Cell(2).GetString();

                    if (existingEmail.Equals(payload.Email, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Email already registered.");
                }

                string generatedUserId = GenerateUserId(
                    worksheet,
                    payload.FirstName,
                    payload.LastName
                );

                int insertRow =  worksheet!.LastRowUsed()!.RowNumber() + 1;

                payload.CreatedAt = DateTime.Now;

                worksheet.Cell(insertRow, 1).Value = generatedUserId;
                worksheet.Cell(insertRow, 2).Value = payload.Email;
                worksheet.Cell(insertRow, 3).Value = payload.Password;
                worksheet.Cell(insertRow, 4).Value = payload.UserRole.ToString();
                worksheet.Cell(insertRow, 5).Value = payload.FirstName;
                worksheet.Cell(insertRow, 6).Value = payload.LastName;
                worksheet.Cell(insertRow, 7).Value = payload.Age;
                worksheet.Cell(insertRow, 8).Value = payload.PhoneNumber;
                worksheet.Cell(insertRow, 9).Value = payload.CreatedAt
                    .ToString("yyyy/MM/dd h:mm tt");

                workbook.Save();

                return true;
            });
        }
        catch
        {
            throw;
        }
    }

    private string GenerateUserId(IXLWorksheet worksheet, string firstName, string lastName)
    {
        string prefix =
            $"{char.ToUpper(firstName[0])}{char.ToUpper(lastName[0])}";

        var rows = worksheet?.RangeUsed()?.RowsUsed().Skip(1);

        int maxNumber = 0;

        foreach (var row in rows!)
        {
            var existingUserId = row.Cell(1).GetString();

            if (existingUserId.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase))
            {
                var numberPart = existingUserId.Split('-')[1];

                if (int.TryParse(numberPart, out int number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }
        }

        int newNumber = maxNumber + 1;

        return $"{prefix}-{newNumber:D4}";
    }

    public string GetCurrentUserId()
    {
        return _currentUser.GetUserIdOrThrow();
    }

    public void Logout()
    {
        _currentUser.Clear();
    }
}
