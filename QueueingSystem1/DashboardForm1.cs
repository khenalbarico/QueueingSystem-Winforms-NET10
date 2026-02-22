using LogicLibrary1.AuthHandler1.Interfaces;
using LogicLibrary1.Models1;
using LogicLibrary1.QueueingHandler1;
using Microsoft.Extensions.DependencyInjection;
using static LogicLibrary1.Models1.Constants1;

namespace QueueingSystem1;

public sealed class DashboardForm1 : Form
{
    private readonly ICurrentUser1 _currentUser;
    private readonly Queue1 _queueService;
    private readonly IServiceProvider _sp;

    private AccountControl1? _accountControl;
    private YourQueuesControl1? _yourQueuesControl;
    private ManageQueuesControl1? _manageQueuesControl;
    private ServicesControl1? _servicesControl;

    private readonly Panel _nav = new() { Dock = DockStyle.Left, Width = 240 };
    private readonly Panel _divider = new() { Dock = DockStyle.Left, Width = 1 };
    private readonly Panel _content = new() { Dock = DockStyle.Fill };

    private readonly Panel _navHeader = new() { Dock = DockStyle.Top, Height = 112 };
    private readonly Panel _navMenu = new() { Dock = DockStyle.Fill };
    private readonly Panel _navFooter = new() { Dock = DockStyle.Bottom, Height = 76 };

    private readonly Label _lblApp = new()
    {
        Text = "Queueing System",
        Dock = DockStyle.Top,
        Height = 56,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        Padding = new Padding(14, 12, 14, 0)
    };

    private readonly Button _btnQueue = new()
    {
        Text = "Queue",
        Height = 34,
        Width = 120,
        Anchor = AnchorStyles.Left | AnchorStyles.Top,
        FlatStyle = FlatStyle.Flat,
        Visible = false
    };

    private readonly Button _btnHome = new()
    {
        Text = "Home",
        Dock = DockStyle.Top,
        Height = 46,
        FlatStyle = FlatStyle.Flat
    };

    private readonly Button _btnYourQueues = new()
    {
        Text = "Your Queues",
        Dock = DockStyle.Top,
        Height = 46,
        FlatStyle = FlatStyle.Flat,
        Visible = false
    };

    private readonly Button _btnManageQueues = new()
    {
        Text = "Manage Queues",
        Dock = DockStyle.Top,
        Height = 46,
        FlatStyle = FlatStyle.Flat,
        Visible = false
    };

    private readonly Button _btnServices = new()
    {
        Text = "Services",
        Dock = DockStyle.Top,
        Height = 46,
        FlatStyle = FlatStyle.Flat,
        Visible = false
    };

    private readonly Button _btnAccount = new()
    {
        Text = "Account",
        Dock = DockStyle.Top,
        Height = 46,
        FlatStyle = FlatStyle.Flat
    };

    private readonly Button _btnLogout = new()
    {
        Text = "Logout",
        Dock = DockStyle.Fill,
        FlatStyle = FlatStyle.Flat
    };

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

    private readonly System.Windows.Forms.Timer _refreshTimer = new();
    private bool _loading;
    private ViewMode _activeView = ViewMode.Home;

    private enum ViewMode
    {
        Home,
        YourQueues,
        Account,
        ManageQueues,
        Services
    }

    public DashboardForm1(ICurrentUser1 currentUser, Queue1 queueService, IServiceProvider sp)
    {
        _currentUser = currentUser;
        _queueService = queueService;
        _sp = sp;

        Text = "Queueing System - Dashboard";
        MinimumSize = new Size(950, 620);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();

        StyleNavButton(_btnHome);
        StyleNavButton(_btnYourQueues);
        StyleNavButton(_btnManageQueues);
        StyleNavButton(_btnServices);
        StyleNavButton(_btnAccount);

        StyleActionButton(_btnQueue, isDanger: false);
        StyleActionButton(_btnLogout, isDanger: true);

        WireEvents();
        ApplyRoleVisibility();
        ConfigureGrid();

        SetSelectedViewExtra(_btnHome, true);

        _refreshTimer.Interval = 1500;
        _refreshTimer.Tick += async (_, __) => await RefreshActiveViewAsync();
        _refreshTimer.Start();

        FormClosed += (_, __) => _refreshTimer.Stop();

        _ = ShowHomeAsync();
    }

    private void BuildLayout()
    {
        BackColor = Color.White;

        _nav.BackColor = Color.FromArgb(247, 248, 250);
        _divider.BackColor = Color.FromArgb(210, 210, 210);
        _content.BackColor = Color.White;

        _btnQueue.FlatAppearance.BorderSize = 0;
        _btnHome.FlatAppearance.BorderSize = 0;
        _btnYourQueues.FlatAppearance.BorderSize = 0;
        _btnManageQueues.FlatAppearance.BorderSize = 0;
        _btnServices.FlatAppearance.BorderSize = 0;
        _btnAccount.FlatAppearance.BorderSize = 0;
        _btnLogout.FlatAppearance.BorderSize = 0;

        _navHeader.Padding = new Padding(8, 0, 8, 8);
        _navMenu.Padding = new Padding(8, 8, 8, 8);
        _navFooter.Padding = new Padding(8, 8, 8, 8);

        var queueWrap = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6, 0, 6, 0) };
        _btnQueue.Location = new Point(6, 0);
        queueWrap.Controls.Add(_btnQueue);

        _navHeader.Controls.Add(queueWrap);
        _navHeader.Controls.Add(_lblApp);

        _navMenu.Controls.Add(_btnAccount);
        _navMenu.Controls.Add(_btnServices);
        _navMenu.Controls.Add(_btnYourQueues);
        _navMenu.Controls.Add(_btnManageQueues);
        _navMenu.Controls.Add(_btnHome);

        _navFooter.Controls.Add(_btnLogout);

        _nav.Controls.Add(_navMenu);
        _nav.Controls.Add(_navFooter);
        _nav.Controls.Add(_navHeader);

        _content.Controls.Add(_grid);

        Controls.Add(_content);
        Controls.Add(_divider);
        Controls.Add(_nav);
    }

    private void WireEvents()
    {
        _btnHome.Click += async (_, __) => await ShowHomeAsync();
        _btnYourQueues.Click += async (_, __) => await ShowYourQueuesAsync();
        _btnManageQueues.Click += async (_, __) => await ShowManageQueuesAsync();
        _btnServices.Click += async (_, __) => await ShowServicesAsync();
        _btnQueue.Click += async (_, __) => await OpenQueueModalAsync();
        _btnAccount.Click += (_, __) => ShowAccount();
        _btnLogout.Click += (_, __) => Logout();
    }

    private void ApplyRoleVisibility()
    {
        var user = _currentUser.User ?? throw new UnauthorizedAccessException("No user is logged in.");
        var isMember = user.UserRole == UserRole.Member;
        var isAdmin = user.UserRole == UserRole.Admin;

        _btnQueue.Visible = isMember;
        _btnYourQueues.Visible = isMember;

        _btnManageQueues.Visible = isAdmin;
        _btnServices.Visible = isAdmin;
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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QueueId", DataPropertyName = "QueueId" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QueueService", DataPropertyName = "QueueService" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status" });
    }

    private async Task RefreshActiveViewAsync()
    {
        if (_loading) return;

        if (_activeView == ViewMode.Home)
            await LoadAllQueuesIntoGridAsync();
        else if (_activeView == ViewMode.YourQueues)
            await ReloadYourQueuesAsync();
        else if (_activeView == ViewMode.ManageQueues && _manageQueuesControl is not null)
            await _manageQueuesControl.LoadAsync();
        else if (_activeView == ViewMode.Services && _servicesControl is not null)
            await _servicesControl.LoadAsync();
    }

    private async Task ShowHomeAsync()
    {
        _activeView = ViewMode.Home;
        SetSelectedViewExtra(_btnHome, true);

        if (_content.Controls.Count != 1 || _content.Controls[0] != _grid)
        {
            _content.Controls.Clear();
            _content.Controls.Add(_grid);
        }

        await LoadAllQueuesIntoGridAsync();
    }

    private async Task ShowYourQueuesAsync()
    {
        _activeView = ViewMode.YourQueues;
        SetSelectedViewExtra(_btnYourQueues, true);

        _yourQueuesControl ??= new YourQueuesControl1(_queueService);

        if (_content.Controls.Count != 1 || _content.Controls[0] != _yourQueuesControl)
        {
            _content.Controls.Clear();
            _content.Controls.Add(_yourQueuesControl);
        }

        await _yourQueuesControl.LoadAsync();
    }

    private async Task ShowManageQueuesAsync()
    {
        _activeView = ViewMode.ManageQueues;
        SetSelectedViewExtra(_btnManageQueues, true);

        _manageQueuesControl ??= _sp.GetRequiredService<ManageQueuesControl1>();

        if (_content.Controls.Count != 1 || _content.Controls[0] != _manageQueuesControl)
        {
            _content.Controls.Clear();
            _content.Controls.Add(_manageQueuesControl);
        }

        await _manageQueuesControl.LoadAsync();
    }

    private async Task ShowServicesAsync()
    {
        _activeView = ViewMode.Services;
        SetSelectedViewExtra(_btnServices, true);

        _servicesControl ??= _sp.GetRequiredService<ServicesControl1>();

        if (_content.Controls.Count != 1 || _content.Controls[0] != _servicesControl)
        {
            _content.Controls.Clear();
            _content.Controls.Add(_servicesControl);
        }

        await _servicesControl.LoadAsync();
    }

    private async Task ReloadYourQueuesAsync()
    {
        if (_yourQueuesControl is null) return;
        await _yourQueuesControl.LoadAsync();
    }

    private async Task LoadAllQueuesIntoGridAsync()
    {
        if (_loading) return;

        try
        {
            _loading = true;

            var queues = await _queueService.DisplayAllQueuesAsync();

            _grid.DataSource = queues
                .Select(q => new
                {
                    q.QueueId,
                    QueueService = q.QueueService.ToString(),
                    Status = q.Status.ToString()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error loading queues", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OpenQueueModalAsync()
    {
        using var modal = _sp.GetRequiredService<QueueModalForm1>();
        var result = modal.ShowDialog(this);

        if (result == DialogResult.OK)
            await RefreshActiveViewAsync();
    }

    private void ShowAccount()
    {
        _activeView = ViewMode.Account;
        SetSelectedViewExtra(_btnAccount, true);

        _accountControl ??= new AccountControl1(_currentUser);

        if (_content.Controls.Count != 1 || _content.Controls[0] != _accountControl)
        {
            _content.Controls.Clear();
            _content.Controls.Add(_accountControl);
        }
    }

    private void Logout()
    {
        _currentUser.Clear();
        Close();
    }

    private void SetSelectedViewExtra(Button btn, bool selected)
    {
        ApplySelectedStyle(btn, selected);
        if (btn != _btnHome) ApplySelectedStyle(_btnHome, false);
        if (btn != _btnYourQueues) ApplySelectedStyle(_btnYourQueues, false);
        if (btn != _btnManageQueues) ApplySelectedStyle(_btnManageQueues, false);
        if (btn != _btnServices) ApplySelectedStyle(_btnServices, false);
        if (btn != _btnAccount) ApplySelectedStyle(_btnAccount, false);
    }

    private static void StyleNavButton(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(12, 0, 12, 0);
        btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        btn.BackColor = Color.Transparent;
        btn.ForeColor = Color.FromArgb(17, 24, 39);
        btn.Cursor = Cursors.Hand;
    }

    private static void ApplySelectedStyle(Button btn, bool selected)
    {
        btn.BackColor = selected ? Color.FromArgb(229, 231, 235) : Color.Transparent;
        btn.ForeColor = Color.FromArgb(17, 24, 39);
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
}