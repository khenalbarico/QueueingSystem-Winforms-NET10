using LogicLibrary1.AdmCntlrHandler1;
using LogicLibrary1.Models1.Services.Consultation;

namespace QueueingSystem1;

public sealed class ServicesControl1 : UserControl
{
    private readonly ServicesController1 _services;

    private readonly Label _lblTitle = new()
    {
        Text = "Services",
        Dock = DockStyle.Top,
        Height = 46,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        Padding = new Padding(16, 10, 16, 0)
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

    private readonly Button _tabEnroll = new() { Text = "Enroll", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat };
    private readonly Button _tabConsultation = new() { Text = "Consultation", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat };
    private readonly Button _tabAdmission = new() { Text = "Admission", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat };

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

    private readonly TableLayoutPanel _layout = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        Margin = new Padding(0)
    };

    private readonly TableLayoutPanel _toolbar = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Padding = new Padding(8, 10, 8, 8),
        BackColor = Color.White,
        Margin = new Padding(0)
    };

    private readonly FlowLayoutPanel _inputs = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = true,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Margin = new Padding(0),
        Padding = new Padding(0)
    };

    private readonly FlowLayoutPanel _actions = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = true,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Margin = new Padding(0),
        Padding = new Padding(0),
        Anchor = AnchorStyles.Right
    };

    private readonly Button _btnAdd = new() { Text = "Add Professor", Width = 150, Height = 36 };
    private readonly Button _btnDelete = new() { Text = "Delete Selected", Width = 150, Height = 36, Enabled = false };

    private readonly TextBox _txtFirstName = new() { PlaceholderText = "First name", Width = 180 };
    private readonly TextBox _txtLastName = new() { PlaceholderText = "Last name", Width = 180 };
    private readonly TextBox _txtSubject = new() { PlaceholderText = "Subject (e.g., Mathematics)", Width = 240 };

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
    private bool _suppressSelectionEvents;
    private bool _needsReload;
    private int? _savedRowIndex;
    private string? _savedUserId;

    private ServiceTab _activeTab = ServiceTab.Consultation;

    private enum ServiceTab
    {
        Enroll,
        Consultation,
        Admission
    }

    public ServicesControl1(ServicesController1 services)
    {
        _services = services;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(247, 248, 250);

        BuildLayout();
        Style();
        WireEvents();

        SetActiveTab(ServiceTab.Consultation);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        if (_activeTab != ServiceTab.Consultation) return;

        if (_loading)
        {
            _needsReload = true;
            return;
        }

        try
        {
            _loading = true;
            UseWaitCursor = true;

            SaveSelection();

            var professors = await _services.LoadProfessorsAsync();

            _suppressSelectionEvents = true;

            _grid.DataSource = professors
                .Select(p => new
                {
                    p.UserId,
                    p.FirstName,
                    p.LastName,
                    p.Subject
                })
                .ToList();

            RestoreSelection();

            _suppressSelectionEvents = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error loading professors", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _loading = false;
            ValidateInputs();

            if (_needsReload)
            {
                _needsReload = false;
                await LoadAsync();
            }
        }
    }

    private void BuildLayout()
    {
        _subTabsInner.Controls.Add(_tabEnroll);
        _subTabsInner.Controls.Add(_tabConsultation);
        _subTabsInner.Controls.Add(_tabAdmission);
        _subTabs.Controls.Add(_subTabsInner);

        _inputs.Controls.Add(_txtFirstName);
        _inputs.Controls.Add(_txtLastName);
        _inputs.Controls.Add(_txtSubject);

        _actions.Controls.Add(_btnAdd);
        _actions.Controls.Add(_btnDelete);

        _toolbar.ColumnCount = 2;
        _toolbar.RowCount = 1;
        _toolbar.ColumnStyles.Clear();
        _toolbar.RowStyles.Clear();
        _toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _toolbar.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _toolbar.Controls.Add(_inputs, 0, 0);
        _toolbar.Controls.Add(_actions, 1, 0);

        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _layout.Controls.Add(_toolbar, 0, 0);
        _layout.Controls.Add(_grid, 0, 1);

        _card.Controls.Add(_layout);

        _pageInner.Controls.Add(_card);
        _page.Controls.Add(_pageInner);

        Controls.Add(_page);
        Controls.Add(_subTabs);
        Controls.Add(_lblTitle);
    }

    private void Style()
    {
        StyleSubTab(_tabEnroll, false);
        StyleSubTab(_tabConsultation, false);
        StyleSubTab(_tabAdmission, false);

        StyleActionButton(_btnAdd, Color.FromArgb(34, 197, 94));
        StyleActionButton(_btnDelete, Color.FromArgb(248, 113, 113));

        _txtFirstName.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        _txtLastName.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        _txtSubject.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        _txtFirstName.Margin = new Padding(0, 0, 10, 8);
        _txtLastName.Margin = new Padding(0, 0, 10, 8);
        _txtSubject.Margin = new Padding(0, 0, 10, 8);

        _btnAdd.Margin = new Padding(10, 0, 10, 8);
        _btnDelete.Margin = new Padding(0, 0, 0, 8);

        ConfigureGrid();
        ApplyTabVisibility();
        ValidateInputs();
    }

    private void WireEvents()
    {
        _tabEnroll.Click += (_, __) => SetActiveTab(ServiceTab.Enroll);
        _tabAdmission.Click += (_, __) => SetActiveTab(ServiceTab.Admission);

        _tabConsultation.Click += async (_, __) =>
        {
            SetActiveTab(ServiceTab.Consultation);
            await LoadAsync();
        };

        _btnAdd.Click += async (_, __) => await AddProfessorAsync();
        _btnDelete.Click += async (_, __) => await DeleteSelectedAsync();

        _txtFirstName.TextChanged += (_, __) => ValidateInputs();
        _txtLastName.TextChanged += (_, __) => ValidateInputs();
        _txtSubject.TextChanged += (_, __) => ValidateInputs();

        _grid.SelectionChanged += (_, __) =>
        {
            if (_suppressSelectionEvents) return;
            ValidateInputs();
        };

        _grid.CellClick += (_, __) =>
        {
            if (_suppressSelectionEvents) return;
            ValidateInputs();
        };

        _grid.DataBindingComplete += (_, __) =>
        {
            if (_suppressSelectionEvents) return;
            ValidateInputs();
        };
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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UserId", DataPropertyName = "UserId", SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "FirstName", DataPropertyName = "FirstName", SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "LastName", DataPropertyName = "LastName", SortMode = DataGridViewColumnSortMode.NotSortable });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Subject", DataPropertyName = "Subject", SortMode = DataGridViewColumnSortMode.NotSortable });
    }

    private void SetActiveTab(ServiceTab tab)
    {
        _activeTab = tab;

        StyleSubTab(_tabEnroll, tab == ServiceTab.Enroll);
        StyleSubTab(_tabConsultation, tab == ServiceTab.Consultation);
        StyleSubTab(_tabAdmission, tab == ServiceTab.Admission);

        ApplyTabVisibility();

        if (tab != ServiceTab.Consultation)
        {
            _suppressSelectionEvents = true;
            _grid.DataSource = Array.Empty<object>();
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _suppressSelectionEvents = false;
        }

        ValidateInputs();
    }

    private void ApplyTabVisibility()
    {
        var isConsultation = _activeTab == ServiceTab.Consultation;

        _toolbar.Visible = isConsultation;
        _grid.Visible = isConsultation;

        if (!isConsultation) return;

        _txtFirstName.Visible = true;
        _txtLastName.Visible = true;
        _txtSubject.Visible = true;

        _btnAdd.Visible = true;
        _btnDelete.Visible = true;
    }

    private void ValidateInputs()
    {
        if (_activeTab != ServiceTab.Consultation)
        {
            _btnAdd.Enabled = false;
            _btnDelete.Enabled = false;
            return;
        }

        var fnOk = !string.IsNullOrWhiteSpace(_txtFirstName.Text);
        var lnOk = !string.IsNullOrWhiteSpace(_txtLastName.Text);
        var subjOk = !string.IsNullOrWhiteSpace(_txtSubject.Text);

        _btnAdd.Enabled = fnOk && lnOk && subjOk;
        _btnDelete.Enabled = GetSelectedProfessorUserId() is not null;
    }

    private async Task AddProfessorAsync()
    {
        if (_activeTab != ServiceTab.Consultation) return;

        try
        {
            UseWaitCursor = true;
            _btnAdd.Enabled = false;

            var payload = new ProffesorModels1
            {
                FirstName = _txtFirstName.Text.Trim(),
                LastName = _txtLastName.Text.Trim(),
                Subject = _txtSubject.Text.Trim()
            };

            await _services.AddProfessorAsync(payload);

            _txtFirstName.Clear();
            _txtLastName.Clear();
            _txtSubject.Clear();

            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Add professor failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ValidateInputs();
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_activeTab != ServiceTab.Consultation) return;

        var userId = GetSelectedProfessorUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            MessageBox.Show("Please select a professor to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show($"Delete professor '{userId}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            UseWaitCursor = true;
            _btnDelete.Enabled = false;

            await _services.DeleteProfessorAsync(userId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ValidateInputs();
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void SaveSelection()
    {
        _savedUserId = GetSelectedProfessorUserId();

        if (_savedUserId is not null)
            return;

        if (_grid.CurrentRow is not null && _grid.CurrentRow.Index >= 0)
            _savedRowIndex = _grid.CurrentRow.Index;
        else
            _savedRowIndex = null;
    }

    private void RestoreSelection()
    {
        _grid.ClearSelection();
        _grid.CurrentCell = null;

        if (_savedUserId is not null)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Cells.Count == 0) continue;
                var id = row.Cells[0].Value?.ToString();
                if (id is null) continue;

                if (id.Equals(_savedUserId, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[0];
                    _grid.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index);
                    return;
                }
            }
        }

        if (_savedRowIndex is not null && _savedRowIndex.Value >= 0 && _savedRowIndex.Value < _grid.Rows.Count)
        {
            var row = _grid.Rows[_savedRowIndex.Value];
            row.Selected = true;
            _grid.CurrentCell = row.Cells[0];
            _grid.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index);
        }
    }

    private string? GetSelectedProfessorUserId()
    {
        if (_grid.SelectedRows.Count > 0)
            return _grid.SelectedRows[0].Cells[0].Value?.ToString();

        if (_grid.CurrentRow is null) return null;
        return _grid.CurrentRow.Cells[0].Value?.ToString();
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
    }
}