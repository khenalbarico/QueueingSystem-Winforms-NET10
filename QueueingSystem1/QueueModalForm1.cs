using LogicLibrary1.AdmCntlrHandler1;
using LogicLibrary1.Models1.Queue1;
using LogicLibrary1.Models1.Services.Consultation;
using LogicLibrary1.QueueingHandler1;
using static LogicLibrary1.Models1.Constants1;

namespace QueueingSystem1;

public sealed class QueueModalForm1 : Form
{
    private readonly Queue1 _queueService;
    private readonly ServicesController1 _services;

    private readonly ComboBox _cmbService = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly ComboBox _cmbProfessor = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Visible = false
    };

    private readonly Label _lblProfessor = new()
    {
        Text = "Select a professor",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Visible = false
    };

    private readonly Button _btnQueueNow = new() { Text = "Queue Now", Width = 120, Enabled = false };
    private readonly Button _btnCancel = new() { Text = "Cancel", Width = 120 };

    private List<ProffesorModels1> _professors = [];

    public QueueModalForm1(Queue1 queueService, ServicesController1 services)
    {
        _queueService = queueService;
        _services = services;

        Text = "Queue";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 260);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = Color.White;

        StyleActionButton(_btnQueueNow, isDanger: false);
        StyleActionButton(_btnCancel, isDanger: true);

        BuildLayout();
        WireEvents();
        LoadServices();
    }

    private void BuildLayout()
    {
        var title = new Label
        {
            Text = "Create Queue",
            Dock = DockStyle.Top,
            Height = 44,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Padding = new Padding(16, 10, 16, 0)
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label
        {
            Text = "Please choose Queue Service",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        }, 0, 0);
        root.Controls.Add(_cmbService, 1, 0);

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(_lblProfessor, 0, 1);
        root.Controls.Add(_cmbProfessor, 1, 1);

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 14));

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        actions.Controls.Add(_btnQueueNow);
        actions.Controls.Add(_btnCancel);

        root.Controls.Add(actions, 0, 3);
        root.SetColumnSpan(actions, 2);

        Controls.Add(root);
        Controls.Add(title);
    }

    private void WireEvents()
    {
        _cmbService.SelectedIndexChanged += async (_, __) => await ServiceChangedAsync();
        _cmbProfessor.SelectedIndexChanged += (_, __) => ValidateInputs();

        _btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };
        _btnQueueNow.Click += async (_, __) => await QueueNowAsync();
    }

    private void LoadServices()
    {
        _cmbService.Items.Clear();
        _cmbService.Items.Add(QueueService.Enroll);
        _cmbService.Items.Add(QueueService.Consultation);
        _cmbService.Items.Add(QueueService.Admission);
        _cmbService.SelectedIndex = 0;
    }

    private async Task ServiceChangedAsync()
    {
        var service = (QueueService)_cmbService.SelectedItem!;

        var showProf = service == QueueService.Consultation;
        _lblProfessor.Visible = showProf;
        _cmbProfessor.Visible = showProf;

        if (showProf)
            await LoadProfessorsAsync();

        ValidateInputs();
    }

    private async Task LoadProfessorsAsync()
    {
        try
        {
            UseWaitCursor = true;
            _cmbProfessor.DataSource = null;

            _professors = await _services.LoadProfessorsAsync();

            var items = _professors
                .Select(p => new ProfessorPickItem(p.UserId, $"{p.FirstName} {p.LastName} ({p.Subject})"))
                .ToList();

            _cmbProfessor.DisplayMember = nameof(ProfessorPickItem.Display);
            _cmbProfessor.ValueMember = nameof(ProfessorPickItem.UserId);
            _cmbProfessor.DataSource = items;

            if (items.Count > 0)
                _cmbProfessor.SelectedIndex = 0;
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void ValidateInputs()
    {
        if (_cmbService.SelectedItem is null)
        {
            _btnQueueNow.Enabled = false;
            return;
        }

        var service = (QueueService)_cmbService.SelectedItem;

        if (service == QueueService.Consultation)
        {
            _btnQueueNow.Enabled = _cmbProfessor.Visible && _cmbProfessor.SelectedItem is not null;
            return;
        }

        _btnQueueNow.Enabled = true;
    }

    private async Task QueueNowAsync()
    {
        try
        {
            UseWaitCursor = true;
            _btnQueueNow.Enabled = false;

            var service = (QueueService)_cmbService.SelectedItem!;

            var payload = new QueueModels1
            {
                QueueService = service,
                Status = Status.Pending
            };

            await _queueService.EnqueueAsync(payload);

            MessageBox.Show($"Queued successfully: {payload.QueueId}", "Queued", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Queue failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ValidateInputs();
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private static void StyleActionButton(Button btn, bool isDanger)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleCenter;
        btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btn.BackColor = isDanger ? Color.FromArgb(248, 113, 113) : Color.FromArgb(34, 197, 94);
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
    }

    private sealed record ProfessorPickItem(string UserId, string Display);
}