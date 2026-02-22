using LogicLibrary1.AdmCntlrHandler1;
using LogicLibrary1.Models1;
using LogicLibrary1.QueueingHandler1;

namespace QueueingSystem1;

public sealed class ManageQueuesControl1 : UserControl
{
    private readonly Queue1 _queueService;
    private readonly QueueController1 _queueController;

    private readonly Label _lblTitle = new()
    {
        Text = "Manage Queues",
        Dock = DockStyle.Top,
        Height = 46,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        Padding = new Padding(16, 10, 16, 0)
    };

    private readonly Panel _topBar = new()
    {
        Dock = DockStyle.Top,
        Height = 54,
        Padding = new Padding(18, 8, 18, 8),
        BackColor = Color.FromArgb(247, 248, 250)
    };

    private readonly Label _lblFilter = new()
    {
        Text = "Service:",
        AutoSize = true,
        Dock = DockStyle.Left,
        Padding = new Padding(0, 8, 10, 0),
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        ForeColor = Color.FromArgb(17, 24, 39)
    };

    private readonly ComboBox _cmbService = new()
    {
        Dock = DockStyle.Left,
        Width = 220,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly Panel _subTabs = new()
    {
        Dock = DockStyle.Top,
        Height = 48,
        Padding = new Padding(18, 6, 18, 6),
        BackColor = Color.FromArgb(247, 248, 250)
    };

    private readonly FlowLayoutPanel _subTabsInner = new()
    {
        Dock = DockStyle.Left,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false,
        AutoSize = true,
        Margin = new Padding(0)
    };

    private readonly Button _tabPending = new() { Text = "Pending", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat };
    private readonly Button _tabProcessing = new() { Text = "Processing", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat };
    private readonly Button _tabCompleted = new() { Text = "Completed", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat };

    private readonly Panel _page = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(247, 248, 250)
    };

    private readonly Panel _pageInner = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(18, 6, 18, 18),
        BackColor = Color.FromArgb(247, 248, 250)
    };

    private readonly Panel _card = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.White,
        Padding = new Padding(12)
    };

    private readonly TableLayoutPanel _cardLayout = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        Margin = new Padding(0)
    };

    private readonly FlowLayoutPanel _actions = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.RightToLeft,
        WrapContents = true,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Padding = new Padding(0, 2, 0, 8),
        Margin = new Padding(0)
    };

    private readonly Button _btnToProcessing = new() { Text = "Move to Processing", Width = 170, Height = 36 };
    private readonly Button _btnToCompleted = new() { Text = "Move to Completed", Width = 170, Height = 36 };
    private readonly Button _btnSetPending = new() { Text = "Set back to Pending", Width = 170, Height = 36 };
    private readonly Button _btnSetProcessing = new() { Text = "Set back to Processing", Width = 170, Height = 36 };

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
    private Constants1.Status _activeStatus = Constants1.Status.Pending;
    private string? _nextQueueId;

    public ManageQueuesControl1(Queue1 queueService, QueueController1 queueController)
    {
        _queueService = queueService;
        _queueController = queueController;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(247, 248, 250);

        BuildLayout();
        Style();
        WireEvents();

        LoadServices();
        SetActiveStatus(Constants1.Status.Pending);
        _ = LoadAsync();
    }

    private void BuildLayout()
    {
        _topBar.Controls.Add(_cmbService);
        _topBar.Controls.Add(_lblFilter);

        _subTabsInner.Controls.Add(_tabPending);
        _subTabsInner.Controls.Add(_tabProcessing);
        _subTabsInner.Controls.Add(_tabCompleted);
        _subTabs.Controls.Add(_subTabsInner);

        _actions.Controls.Add(_btnToCompleted);
        _actions.Controls.Add(_btnToProcessing);
        _actions.Controls.Add(_btnSetProcessing);
        _actions.Controls.Add(_btnSetPending);

        _cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _cardLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _cardLayout.Controls.Add(_actions, 0, 0);
        _cardLayout.Controls.Add(_grid, 0, 1);

        _card.Controls.Add(_cardLayout);

        _pageInner.Controls.Add(_card);
        _page.Controls.Add(_pageInner);

        Controls.Add(_page);
        Controls.Add(_subTabs);
        Controls.Add(_topBar);
        Controls.Add(_lblTitle);
    }

    private void Style()
    {
        StyleSubTab(_tabPending, false);
        StyleSubTab(_tabProcessing, false);
        StyleSubTab(_tabCompleted, false);

        StyleActionButton(_btnToProcessing, Color.FromArgb(59, 130, 246));
        StyleActionButton(_btnToCompleted, Color.FromArgb(34, 197, 94));
        StyleActionButton(_btnSetPending, Color.FromArgb(107, 114, 128));
        StyleActionButton(_btnSetProcessing, Color.FromArgb(99, 102, 241));

        ConfigureGrid();
    }

    private void WireEvents()
    {
        _cmbService.SelectedIndexChanged += async (_, __) => await LoadAsync();

        _tabPending.Click += async (_, __) => { SetActiveStatus(Constants1.Status.Pending); await LoadAsync(); };
        _tabProcessing.Click += async (_, __) => { SetActiveStatus(Constants1.Status.Processing); await LoadAsync(); };
        _tabCompleted.Click += async (_, __) => { SetActiveStatus(Constants1.Status.Completed); await LoadAsync(); };

        _btnToProcessing.Click += async (_, __) => await MoveSelectedAsync(Constants1.Status.Processing, Constants1.Status.Processing);
        _btnToCompleted.Click += async (_, __) => await MoveSelectedAsync(Constants1.Status.Completed, Constants1.Status.Completed);
        _btnSetPending.Click += async (_, __) => await SetBackSelectedAsync(Constants1.Status.Pending, Constants1.Status.Pending);
        _btnSetProcessing.Click += async (_, __) => await SetBackSelectedAsync(Constants1.Status.Processing, Constants1.Status.Processing);

        _grid.DataBindingComplete += (_, __) =>
        {
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            ApplyNextIndicatorStyling();
        };

        _grid.CellMouseDown += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            _grid.ClearSelection();
            _grid.Rows[e.RowIndex].Selected = true;
            _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
        };
    }

    private void LoadServices()
    {
        _cmbService.Items.Clear();
        _cmbService.Items.Add(Constants1.QueueService.Enroll);
        _cmbService.Items.Add(Constants1.QueueService.Admission);
        _cmbService.Items.Add(Constants1.QueueService.Consultation);
        _cmbService.SelectedIndex = 0;
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowTemplate.Height = 36;

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
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.EditMode = DataGridViewEditMode.EditProgrammatically;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", DataPropertyName = "Priority", FillWeight = 12, SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UserId", DataPropertyName = "UserId", FillWeight = 30, SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QueueId", DataPropertyName = "QueueId", FillWeight = 24, SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = "Created", FillWeight = 34, SortMode = DataGridViewColumnSortMode.NotSortable });
    }

    private void SetActiveStatus(Constants1.Status status)
    {
        _activeStatus = status;

        StyleSubTab(_tabPending, status == Constants1.Status.Pending);
        StyleSubTab(_tabProcessing, status == Constants1.Status.Processing);
        StyleSubTab(_tabCompleted, status == Constants1.Status.Completed);
    }

    public async Task LoadAsync()
    {
        if (_loading) return;

        var previousSelectedQueueId = GetSelectedQueueId();

        try
        {
            _loading = true;
            UseWaitCursor = true;

            var service = (Constants1.QueueService)_cmbService.SelectedItem!;
            var all = await _queueService.DisplayAllQueuesAsync();

            var filtered = all
                .Where(q => q.QueueService == service && q.Status == _activeStatus)
                .OrderBy(q => q.CreatedAt)
                .ToList();

            _nextQueueId = filtered.FirstOrDefault()?.QueueId;

            _grid.DataSource = filtered.Select(q => new
            {
                Priority = (!string.IsNullOrWhiteSpace(_nextQueueId) && q.QueueId.Equals(_nextQueueId, StringComparison.OrdinalIgnoreCase)) ? "NEXT" : "",
                q.UserId,
                q.QueueId,
                Created = q.CreatedAt == default ? "" : q.CreatedAt.ToLocalTime().ToString("yyyy/MM/dd h:mm tt")
            }).ToList();

            SetButtonsForStatus(hasRows: _grid.Rows.Count > 0);
            RestoreSelection(previousSelectedQueueId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _loading = false;
        }
    }

    private void RestoreSelection(string? previousQueueId)
    {
        _grid.ClearSelection();
        _grid.CurrentCell = null;

        if (string.IsNullOrWhiteSpace(previousQueueId))
            return;

        foreach (DataGridViewRow r in _grid.Rows)
        {
            var qid = r.Cells[2].Value?.ToString();
            if (string.IsNullOrWhiteSpace(qid)) continue;

            if (qid.Equals(previousQueueId, StringComparison.OrdinalIgnoreCase))
            {
                r.Selected = true;
                _grid.CurrentCell = r.Cells[2];
                if (r.Index >= 0)
                    _grid.FirstDisplayedScrollingRowIndex = Math.Max(0, r.Index);
                return;
            }
        }
    }

    private void ApplyNextIndicatorStyling()
    {
        foreach (DataGridViewRow r in _grid.Rows)
        {
            r.DefaultCellStyle.BackColor = Color.White;
            r.Cells[0].Style.ForeColor = _grid.DefaultCellStyle.ForeColor;
            r.Cells[0].Style.Font = _grid.DefaultCellStyle.Font;
        }

        if (string.IsNullOrWhiteSpace(_nextQueueId)) return;

        foreach (DataGridViewRow r in _grid.Rows)
        {
            var qid = r.Cells[2].Value?.ToString();
            if (string.IsNullOrWhiteSpace(qid)) continue;

            if (!qid.Equals(_nextQueueId, StringComparison.OrdinalIgnoreCase))
                continue;

            r.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 195);
            r.Cells[0].Style.ForeColor = Color.FromArgb(180, 83, 9);
            r.Cells[0].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            break;
        }
    }

    private void SetButtonsForStatus(bool hasRows)
    {
        _btnToProcessing.Visible = _activeStatus == Constants1.Status.Pending;
        _btnToCompleted.Visible = _activeStatus == Constants1.Status.Processing;

        _btnSetPending.Visible = _activeStatus != Constants1.Status.Pending;
        _btnSetProcessing.Visible = _activeStatus == Constants1.Status.Completed;

        _btnToProcessing.Enabled = hasRows && GetSelectedQueueId() is not null;
        _btnToCompleted.Enabled = hasRows && GetSelectedQueueId() is not null;
        _btnSetPending.Enabled = hasRows && GetSelectedQueueId() is not null;
        _btnSetProcessing.Enabled = hasRows && GetSelectedQueueId() is not null;

        _grid.SelectionChanged -= GridSelectionChanged;
        _grid.SelectionChanged += GridSelectionChanged;
    }

    private void GridSelectionChanged(object? sender, EventArgs e)
    {
        var hasSel = GetSelectedQueueId() is not null;

        if (_activeStatus == Constants1.Status.Pending)
            _btnToProcessing.Enabled = hasSel;
        if (_activeStatus == Constants1.Status.Processing)
            _btnToCompleted.Enabled = hasSel;

        if (_activeStatus != Constants1.Status.Pending)
            _btnSetPending.Enabled = hasSel;
        if (_activeStatus == Constants1.Status.Completed)
            _btnSetProcessing.Enabled = hasSel;
    }

    private string? GetSelectedQueueId()
    {
        if (_grid.SelectedRows.Count > 0)
            return _grid.SelectedRows[0].Cells[2].Value?.ToString();

        if (_grid.CurrentRow is null) return null;
        return _grid.CurrentRow.Cells[2].Value?.ToString();
    }

    private async Task MoveSelectedAsync(Constants1.Status newStatus, Constants1.Status switchTo)
    {
        var queueId = GetSelectedQueueId();
        if (string.IsNullOrWhiteSpace(queueId))
        {
            MessageBox.Show("Please select a queue.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            UseWaitCursor = true;

            if (newStatus == Constants1.Status.Processing)
                await _queueController.MoveToProcessingAsync(queueId);
            else if (newStatus == Constants1.Status.Completed)
                await _queueController.MoveToCompleteAsync(queueId);

            SetActiveStatus(switchTo);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task SetBackSelectedAsync(Constants1.Status status, Constants1.Status switchTo)
    {
        var queueId = GetSelectedQueueId();
        if (string.IsNullOrWhiteSpace(queueId))
        {
            MessageBox.Show("Please select a queue.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            UseWaitCursor = true;
            await _queueController.SetBackAsync(queueId, status);

            SetActiveStatus(switchTo);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private static void StyleSubTab(Button btn, bool selected)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleCenter;
        btn.Font = new Font("Segoe UI", 10, selected ? FontStyle.Bold : FontStyle.Regular);
        btn.BackColor = selected ? Color.FromArgb(229, 231, 235) : Color.Transparent;
        btn.ForeColor = Color.FromArgb(17, 24, 39);
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(0, 0, 10, 0);
    }

    private static void StyleActionButton(Button btn, Color bg)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleCenter;
        btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btn.BackColor = bg;
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(10, 0, 0, 0);
    }
}