using LogicLibrary1.Models1.Queue1;
using LogicLibrary1.QueueingHandler1;

namespace QueueingSystem1;

public sealed class YourQueuesControl1 : UserControl
{
    private readonly Queue1 _queueService;

    private const string ColDelete = "__delete";
    private const string ColQueueId = "QueueId";

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        ScrollBars = ScrollBars.Both,
        RowHeadersVisible = false
    };

    private bool _loading;

    public YourQueuesControl1(Queue1 queueService)
    {
        _queueService = queueService;

        Dock = DockStyle.Fill;
        BackColor = Color.White;

        Controls.Add(_grid);
        ConfigureGrid();
        WireEvents();

        _ = LoadAsync();
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowTemplate.Height = 38;

        _grid.BackgroundColor = Color.White;
        _grid.GridColor = Color.FromArgb(230, 230, 230);
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 244, 246);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _grid.ColumnHeadersHeight = 42;

        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(229, 231, 235);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);

        var trashIcon = CreateTrashIcon(16, Color.FromArgb(239, 68, 68));

        var deleteCol = new DataGridViewImageColumn
        {
            Name = ColDelete,
            HeaderText = "",
            Image = trashIcon,
            ImageLayout = DataGridViewImageCellLayout.Normal,
            Width = 36,
            MinimumWidth = 36,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        };

        _grid.Columns.Add(deleteCol);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = ColQueueId, HeaderText = "QueueId", DataPropertyName = "QueueId" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QueueService", DataPropertyName = "QueueService" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = "Created" });

        _grid.Columns?[ColDelete]?.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grid.Columns?[ColDelete]?.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (_grid.Columns?[e.ColumnIndex].Name != ColDelete) return;

            _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = "Delete this queue";
        };
    }

    private void WireEvents()
    {
        _grid.CellContentClick += async (_, e) => await HandleDeleteClickAsync(e);
        _grid.CellMouseEnter += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            Cursor = _grid.Columns[e.ColumnIndex].Name == ColDelete ? Cursors.Hand : Cursors.Default;
        };
        _grid.MouseLeave += (_, __) => Cursor = Cursors.Default;
    }

    private static Bitmap CreateTrashIcon(int size, Color color)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var pen = new Pen(color, 2);

        var bodyRect = new Rectangle(size / 4, size / 3, size / 2, size / 2);
        g.DrawRectangle(pen, bodyRect);

        g.DrawLine(pen, size / 4, size / 3, size * 3 / 4, size / 3);

        g.DrawLine(pen, size / 3, size / 4, size * 2 / 3, size / 4);

        g.DrawLine(pen, size / 3, size / 2, size / 3, size * 3 / 4);
        g.DrawLine(pen, size / 2, size / 2, size / 2, size * 3 / 4);
        g.DrawLine(pen, size * 2 / 3, size / 2, size * 2 / 3, size * 3 / 4);

        return bmp;
    }

    private async Task HandleDeleteClickAsync(DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (_grid.Columns[e.ColumnIndex].Name != ColDelete) return;
        if (_loading) return;

        var queueIdObj = _grid.Rows[e.RowIndex].Cells[ColQueueId].Value;
        var queueId = queueIdObj?.ToString();

        if (string.IsNullOrWhiteSpace(queueId)) return;

        var confirm = MessageBox.Show(
            $"Delete queue '{queueId}'?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        try
        {
            UseWaitCursor = true;

            await _queueService.DequeueAsync(new QueueModels1
            {
                QueueId = queueId
            });

            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    public async Task LoadAsync()
    {
        if (_loading) return;

        try
        {
            _loading = true;

            var queues = await _queueService.DisplayCurrentUserQueuesAsync();

            _grid.DataSource = queues
                .Select(q => new
                {
                    q.QueueId,
                    QueueService = q.QueueService.ToString(),
                    Status = q.Status.ToString(),
                    Created = q.CreatedAt == default ? "" : q.CreatedAt.ToLocalTime().ToString("yyyy/MM/dd h:mm tt")
                })
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error loading your queues", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loading = false;
        }
    }
}