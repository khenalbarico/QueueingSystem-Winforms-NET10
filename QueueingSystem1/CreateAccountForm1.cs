using LogicLibrary1.AuthHandler1;
using LogicLibrary1.Models1;
using LogicLibrary1.Models1.User1;

namespace QueueingSystem1;

public sealed class CreateAccountForm1 : Form
{
    private readonly Authentication1 _auth;

    public string? CreatedEmail { get; private set; }

    private readonly TextBox _txtEmail = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtPassword = new() { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
    private readonly TextBox _txtFirstName = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtLastName = new() { Dock = DockStyle.Fill };

    private readonly NumericUpDown _numAge = new()
    {
        Dock = DockStyle.Left,
        Minimum = 1,
        Maximum = 120,
        Width = 120
    };

    private readonly TextBox _txtPhone = new() { Dock = DockStyle.Left, Width = 180 };

    private readonly Button _btnCreate = new() { Text = "Create", Width = 120, Enabled = false };
    private readonly Button _btnCancel = new() { Text = "Cancel", Width = 120 };

    private readonly Label _lblTitle = new()
    {
        Text = "Create Account",
        AutoSize = true,
        Font = new Font("Segoe UI", 14, FontStyle.Bold)
    };

    public CreateAccountForm1(Authentication1 auth)
    {
        _auth = auth;

        Text = "Queueing System - Create Account";
        MinimumSize = new Size(620, 420);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(247, 248, 250);

        StyleActionButton(_btnCreate, isDanger: false);
        StyleActionButton(_btnCancel, isDanger: true);

        BuildLayout();
        WireEvents();
        ValidateInputs();
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
            Padding = new Padding(6),
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(_lblTitle, 0, 0);
        root.SetColumnSpan(_lblTitle, 2);

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));

        AddRow(root, 2, "Email", _txtEmail);
        AddRow(root, 3, "Password", _txtPassword);
        AddRow(root, 4, "First Name", _txtFirstName);
        AddRow(root, 5, "Last Name", _txtLastName);

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = "Age", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 6);
        root.Controls.Add(_numAge, 1, 6);

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = "Phone Number", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, 7);
        root.Controls.Add(_txtPhone, 1, 7);

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        actions.Controls.Add(_btnCreate);
        actions.Controls.Add(_btnCancel);

        root.Controls.Add(actions, 0, 8);
        root.SetColumnSpan(actions, 2);

        card.Controls.Add(root);

        var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18) };
        wrap.Controls.Add(card);

        Controls.Add(wrap);
    }

    private static void AddRow(TableLayoutPanel root, int rowIndex, string label, Control input)
    {
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) }, 0, rowIndex);
        root.Controls.Add(input, 1, rowIndex);
    }

    private void WireEvents()
    {
        _txtEmail.TextChanged += (_, __) => ValidateInputs();
        _txtPassword.TextChanged += (_, __) => ValidateInputs();
        _txtFirstName.TextChanged += (_, __) => ValidateInputs();
        _txtLastName.TextChanged += (_, __) => ValidateInputs();
        _txtPhone.TextChanged += (_, __) => ValidateInputs();
        _numAge.ValueChanged += (_, __) => ValidateInputs();

        _btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };
        _btnCreate.Click += async (_, __) => await CreateAsync();
    }

    private void ValidateInputs()
    {
        var emailOk = !string.IsNullOrWhiteSpace(_txtEmail.Text);
        var passOk = !string.IsNullOrWhiteSpace(_txtPassword.Text);
        var fnOk = !string.IsNullOrWhiteSpace(_txtFirstName.Text);
        var lnOk = !string.IsNullOrWhiteSpace(_txtLastName.Text);
        var phoneOk = int.TryParse(_txtPhone.Text.Trim(), out var phone) && phone > 0;
        var ageOk = _numAge.Value > 0;

        _btnCreate.Enabled = emailOk && passOk && fnOk && lnOk && phoneOk && ageOk;
    }

    private async Task CreateAsync()
    {
        try
        {
            UseWaitCursor = true;
            _btnCreate.Enabled = false;

            if (!int.TryParse(_txtPhone.Text.Trim(), out var phone))
                throw new InvalidOperationException("Phone number must be numeric.");

            var payload = new UserInfoModels1
            {
                Email = _txtEmail.Text.Trim(),
                Password = _txtPassword.Text,
                FirstName = _txtFirstName.Text.Trim(),
                LastName = _txtLastName.Text.Trim(),
                Age = (int)_numAge.Value,
                PhoneNumber = phone,
                UserRole = Constants1.UserRole.Member
            };

            var ok = await _auth.OnCreateNewAccount(payload);

            if (ok)
            {
                CreatedEmail = payload.Email;
                MessageBox.Show("Account created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Create account failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}