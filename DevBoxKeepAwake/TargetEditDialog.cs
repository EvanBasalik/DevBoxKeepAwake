namespace DevBoxKeepAwake;

internal partial class TargetEditDialog : Form
{
    private readonly KeepAliveTargetSettings _target;

    public TargetEditDialog(KeepAliveTargetSettings? existingTarget = null)
    {
        _target = existingTarget ?? new KeepAliveTargetSettings();
        InitializeComponent();
        PopulateForm();
    }

    public KeepAliveTargetSettings Target => _target;

    private void InitializeComponent()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(12),
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < 7; i++)
        {
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        var lblName = new Label { Text = Localization.Text("TargetNameLabel"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, TabStop = false };
        var txtName = new TextBox { Name = "txtName", Dock = DockStyle.Fill, TabIndex = 0, AccessibleName = Localization.Text("TargetNameAccessibleName"), AccessibleDescription = Localization.Text("TargetNameAccessibleDescription") };

        var lblFileName = new Label { Text = Localization.Text("TargetFileNameLabel"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, TabStop = false };
        var txtFileName = new TextBox { Name = "txtFileName", Dock = DockStyle.Fill, TabIndex = 1, AccessibleName = Localization.Text("FileNameAccessibleName"), AccessibleDescription = Localization.Text("FileNameAccessibleDescription") };

        var lblArguments = new Label { Text = Localization.Text("TargetArgumentsLabel"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, TabStop = false };
        var txtArguments = new TextBox { Name = "txtArguments", Dock = DockStyle.Fill, Multiline = false, TabIndex = 2, AccessibleName = Localization.Text("ArgumentsAccessibleName"), AccessibleDescription = Localization.Text("ArgumentsAccessibleDescription") };

        var lblProcessName = new Label { Text = Localization.Text("TargetProcessNameLabel"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, TabStop = false };
        var txtProcessName = new TextBox { Name = "txtProcessName", Dock = DockStyle.Fill, TabIndex = 3, AccessibleName = Localization.Text("ProcessNameAccessibleName"), AccessibleDescription = Localization.Text("ProcessNameAccessibleDescription") };

        var lblWorkingDir = new Label { Text = Localization.Text("TargetWorkingDirLabel"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, TabStop = false };
        var txtWorkingDir = new TextBox { Name = "txtWorkingDir", Dock = DockStyle.Fill, TabIndex = 4, AccessibleName = Localization.Text("WorkingDirAccessibleName"), AccessibleDescription = Localization.Text("WorkingDirAccessibleDescription") };

        var lblEnabled = new Label { Text = Localization.Text("TargetEnabledLabel"), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, TabStop = false };
        var chkEnabled = new CheckBox { Name = "chkEnabled", Dock = DockStyle.Fill, AutoSize = true, TabIndex = 5, AccessibleName = Localization.Text("EnabledAccessibleName"), AccessibleDescription = Localization.Text("EnabledAccessibleDescription") };

        var chkCreateNoWindow = new CheckBox
        {
            Name = "chkCreateNoWindow",
            Text = Localization.Text("CreateNoWindowLabel"),
            Dock = DockStyle.Fill,
            AutoSize = true,
            TabIndex = 6,
            AccessibleName = Localization.Text("CreateNoWindowAccessibleName"),
            AccessibleDescription = Localization.Text("CreateNoWindowAccessibleDescription"),
        };

        mainLayout.Controls.Add(lblName, 0, 0);
        mainLayout.Controls.Add(txtName, 1, 0);
        mainLayout.Controls.Add(lblFileName, 0, 1);
        mainLayout.Controls.Add(txtFileName, 1, 1);
        mainLayout.Controls.Add(lblArguments, 0, 2);
        mainLayout.Controls.Add(txtArguments, 1, 2);
        mainLayout.Controls.Add(lblProcessName, 0, 3);
        mainLayout.Controls.Add(txtProcessName, 1, 3);
        mainLayout.Controls.Add(lblWorkingDir, 0, 4);
        mainLayout.Controls.Add(txtWorkingDir, 1, 4);
        mainLayout.Controls.Add(lblEnabled, 0, 5);
        mainLayout.Controls.Add(chkEnabled, 1, 5);
        mainLayout.Controls.Add(chkCreateNoWindow, 1, 6);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0),
            WrapContents = false,
        };
        var btnOK = new Button
        {
            Text = Localization.Text("ButtonOk"),
            DialogResult = DialogResult.OK,
            Width = 80,
            TabIndex = 7,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
        };
        var btnCancel = new Button
        {
            Text = Localization.Text("ButtonCancel"),
            DialogResult = DialogResult.Cancel,
            Width = 80,
            TabIndex = 8,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
        };

        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnOK);

        Controls.Add(mainLayout);
        Controls.Add(buttonPanel);

        Text = Localization.Text("TargetEditTitle");
        Width = 500;
        Height = 360;
        MinimumSize = new Size(460, 340);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        Icon = AppIconProvider.Load();
        ShowIcon = true;
        AccessibleName = Localization.Text("TargetEditAccessibleName");
        AccessibleDescription = Localization.Text("TargetEditAccessibleDescription");

        AcceptButton = btnOK;
        CancelButton = btnCancel;
    }

    private void PopulateForm()
    {
        var txtName = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtName");
        var txtFileName = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtFileName");
        var txtArguments = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtArguments");
        var txtProcessName = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtProcessName");
        var txtWorkingDir = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtWorkingDir");
        var chkEnabled = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkEnabled");
        var chkCreateNoWindow = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkCreateNoWindow");

        if (txtName is not null) txtName.Text = _target.Name;
        if (txtFileName is not null) txtFileName.Text = _target.FileName;
        if (txtArguments is not null) txtArguments.Text = _target.Arguments;
        if (txtProcessName is not null) txtProcessName.Text = _target.ProcessName ?? string.Empty;
        if (txtWorkingDir is not null) txtWorkingDir.Text = _target.WorkingDirectory ?? string.Empty;
        if (chkEnabled is not null) chkEnabled.Checked = _target.Enabled;
        if (chkCreateNoWindow is not null) chkCreateNoWindow.Checked = _target.CreateNoWindow;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            try
            {
                var txtName = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtName");
                var txtFileName = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtFileName");
                var txtArguments = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtArguments");
                var txtProcessName = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtProcessName");
                var txtWorkingDir = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<TextBox>().FirstOrDefault(c => c.Name == "txtWorkingDir");
                var chkEnabled = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkEnabled");
                var chkCreateNoWindow = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                    .OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkCreateNoWindow");

                if (string.IsNullOrWhiteSpace(txtFileName?.Text))
                {
                    MessageBox.Show(Localization.Text("FileNameRequiredMessage"), Localization.Text("ValidationErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }

                _target.Name = txtName?.Text ?? Localization.Text("DefaultTargetName");
                _target.FileName = txtFileName?.Text ?? string.Empty;
                _target.Arguments = txtArguments?.Text ?? string.Empty;
                _target.ProcessName = string.IsNullOrWhiteSpace(txtProcessName?.Text) ? null : txtProcessName.Text;
                _target.WorkingDirectory = string.IsNullOrWhiteSpace(txtWorkingDir?.Text) ? null : txtWorkingDir.Text;
                _target.Enabled = chkEnabled?.Checked ?? true;
                _target.CreateNoWindow = chkCreateNoWindow?.Checked ?? true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localization.Format("ErrorSavingTargetMessage", ex.Message), Localization.Text("ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        base.OnFormClosing(e);
    }

    private static IEnumerable<Control> GetAllControls(Control container)
    {
        foreach (Control control in container.Controls)
        {
            yield return control;
            foreach (var child in GetAllControls(control))
            {
                yield return child;
            }
        }
    }
}
