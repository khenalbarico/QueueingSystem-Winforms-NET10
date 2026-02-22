using LogicLibrary1.AuthHandler1;
using LogicLibrary1.Models1.Auth1;
using Microsoft.Extensions.DependencyInjection;

namespace QueueingSystem1;

public sealed class LoginForm1 : Form
{
    private readonly Authentication1 _auth;
    private readonly IServiceProvider _sp;

    private readonly TextBox _txtEmail = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtPassword = new() { Dock = DockStyle.Fill, UseSystemPasswordChar = true };

    private readonly Button _btnLogin = new() { Text = "Login", Width = 120 };
    private readonly LinkLabel _lnkCreate = new() { Text = "Create new account", AutoSize = true };

    private readonly Label _lblTitle = new()
    {
        Text = "Queueing System",
        AutoSize = true,
        Font = new Font("Segoe UI", 16, FontStyle.Bold)
    };

    public LoginForm1(Authentication1 auth, IServiceProvider sp)
    {
        _auth = auth;
        _sp = sp;

        Text = "Queueing System - Login";
        MinimumSize = new Size(560, 320);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(247, 248, 250);

        StyleActionButton(_btnLogin, isDanger: false);

        BuildLayout();
        WireEvents();
    }

    private void BuildLayout()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            BackColor = Color.White
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(_lblTitle, 0, 0);
        root.SetColumnSpan(_lblTitle, 2);

        root.Controls.Add(new Label { Text = "Email", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 2);
        root.Controls.Add(_txtEmail, 1, 2);

        root.Controls.Add(new Label { Text = "Password", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 3);
        root.Controls.Add(_txtPassword, 1, 3);

        var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _lnkCreate.LinkColor = Color.FromArgb(59, 130, 246);
        _lnkCreate.ActiveLinkColor = Color.FromArgb(37, 99, 235);

        actions.Controls.Add(_lnkCreate, 0, 0);
        actions.Controls.Add(_btnLogin, 1, 0);

        root.Controls.Add(actions, 0, 5);
        root.SetColumnSpan(actions, 2);

        card.Controls.Add(root);

        var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };
        wrap.Controls.Add(card);

        Controls.Add(wrap);
    }

    private void WireEvents()
    {
        _btnLogin.Click += async (_, __) => await LoginAsync();
        _lnkCreate.LinkClicked += (_, __) => OpenCreateAccount();

        _txtPassword.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoginAsync();
            }
        };
    }

    private async Task LoginAsync()
    {
        var email = _txtEmail.Text.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Please enter your email and password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            UseWaitCursor = true;
            _btnLogin.Enabled = false;

            await _auth.OnLoginAsync(new LoginModels1
            {
                Email = email,
                Password = password
            });

            var dashboard = _sp.GetRequiredService<DashboardForm1>();
            dashboard.FormClosed += (_, __) =>
            {
                if (!IsDisposed)
                {
                    Show();
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                }
            };

            Hide();
            dashboard.Show();
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _btnLogin.Enabled = true;
        }
    }

    private void OpenCreateAccount()
    {
        using var create = _sp.GetRequiredService<CreateAccountForm1>();
        var result = create.ShowDialog(this);

        if (result == DialogResult.OK)
        {
            _txtEmail.Text = create.CreatedEmail ?? _txtEmail.Text;
            _txtPassword.Focus();
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
}