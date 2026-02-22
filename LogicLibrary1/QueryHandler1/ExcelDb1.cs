using ClosedXML.Excel;

namespace LogicLibrary1.QueryHandler1;

public class ExcelDb1
{
    public static (XLWorkbook Workbook, IXLWorksheet Worksheet) GetExcelDb(string dbName)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName);
        var workbook = new XLWorkbook(dbPath);
        var worksheet = workbook.Worksheet(1);

        return (workbook, worksheet);
    }
}