using LogicLibrary1.AuthHandler1.Interfaces;

namespace QueueingSystem1;

public sealed class AccountControl1 : UserControl
{
    private readonly ICurrentUser1 _currentUser;

    private readonly Label _lblTitle = new()
    {
        Text = "Account",
        Dock = DockStyle.Top,
        Height = 46,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        Padding = new Padding(16, 10, 16, 0)
    };

    private readonly Panel _card = new()
    {
        Dock = DockStyle.Top,
        Padding = new Padding(18),
        BackColor = Color.White
    };

    private readonly TableLayoutPanel _grid = new()
    {
        Dock = DockStyle.Top,
        ColumnCount = 2,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink
    };

    public AccountControl1(ICurrentUser1 currentUser)
    {
        _currentUser = currentUser;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(247, 248, 250);

        BuildLayout();
        LoadUser();
    }
    private void BuildLayout()
    {
        _grid.ColumnStyles.Clear();
        _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _grid.Dock = DockStyle.Top;
        _grid.AutoSize = true;
        _grid.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _card.MinimumSize = new Size(0, 0);
        _card.Dock = DockStyle.Top;
        _card.AutoSize = true;
        _card.AutoSizeMode = AutoSizeMode.GrowAndShrink;

        _card.Controls.Add(_grid);

        var wrap = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            AutoScroll = true,
            BackColor = Color.FromArgb(247, 248, 250)
        };

        wrap.Controls.Add(_card);
        wrap.Controls.Add(_lblTitle);

        Controls.Add(wrap);
    }

    private void LoadUser()
    {
        var user = _currentUser.User ?? throw new UnauthorizedAccessException("No user is logged in.");

        _grid.Controls.Clear();
        _grid.RowStyles.Clear();
        _grid.RowCount = 0;

        AddRow("Email", user.Email);
        AddRow("First Name", user.FirstName);
        AddRow("Last Name", user.LastName);
        AddRow("Age", user.Age.ToString());
        AddRow("Phone Number", user.PhoneNumber.ToString());
        AddRow("Role", user.UserRole.ToString());
        AddRow("Created", user.CreatedAt == default ? "" : user.CreatedAt.ToLocalTime().ToString("yyyy/MM/dd h:mm tt"));
    }

    private void AddRow(string label, string value)
    {
        var row = _grid.RowCount++;
        _grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lbl = new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Padding = new Padding(0, 6, 0, 6)
        };

        var txt = new TextBox
        {
            Text = value,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill
        };

        _grid.Controls.Add(lbl, 0, row);
        _grid.Controls.Add(txt, 1, row);
    }
}