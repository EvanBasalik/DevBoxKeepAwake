namespace DevBoxKeepAwake;

internal partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly string _configPath;
    private readonly FileLogger _logger;
    private readonly Func<AppSettings, string, Task> _saveSettingsCallback;
    private bool _pythonInstallPrompted;

    public SettingsForm(AppSettings settings, string configPath, FileLogger logger, Func<AppSettings, string, Task> saveSettingsCallback)
    {
        _settings = settings;
        _configPath = configPath;
        _logger = logger;
        _saveSettingsCallback = saveSettingsCallback;
        InitializeComponent();
        PopulateForm();
        ValidateConfiguredTargets(showDialog: true);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        var footerPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0),
            WrapContents = false,
        };

        var btnOK = new Button
        {
            Text = Localization.Text("ButtonOk"),
            DialogResult = DialogResult.OK,
            Width = 100,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
            TabIndex = 0,
        };
        var btnCancel = new Button
        {
            Text = Localization.Text("ButtonCancel"),
            DialogResult = DialogResult.Cancel,
            Width = 100,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
            TabIndex = 1,
        };

        footerPanel.Controls.Add(btnCancel);
        footerPanel.Controls.Add(btnOK);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            ColumnCount = 1,
            RowCount = 5,
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 10),
        };
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lblMousePoll = new Label
        {
            Text = Localization.Text("MousePollLabel"),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 10, 0),
            TabStop = false,
        };
        var numMousePoll = new NumericUpDown
        {
            Name = "numMousePoll",
            Minimum = 1,
            Maximum = 300,
            Width = 100,
            Height = 25,
            Margin = new Padding(0, 0, 0, 8),
            TabIndex = 2,
            AccessibleName = Localization.Text("MousePollAccessibleName"),
            AccessibleDescription = Localization.Text("MousePollAccessibleDescription"),
        };

        var lblCheckTime = new Label
        {
            Text = Localization.Text("CheckTimeLabel"),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 10, 0),
            TabStop = false,
        };
        var numCheckTime = new NumericUpDown
        {
            Name = "numCheckTime",
            Minimum = 1,
            Maximum = 1440,
            Width = 100,
            Height = 25,
            TabIndex = 3,
            AccessibleName = Localization.Text("CheckTimeAccessibleName"),
            AccessibleDescription = Localization.Text("CheckTimeAccessibleDescription"),
        };

        settingsLayout.Controls.Add(lblMousePoll, 0, 0);
        settingsLayout.Controls.Add(numMousePoll, 1, 0);
        settingsLayout.Controls.Add(lblCheckTime, 0, 1);
        settingsLayout.Controls.Add(numCheckTime, 1, 1);

        var lblTargets = new Label
        {
            Text = Localization.Text("KeepaliveTargetsLabel"),
            AutoSize = true,
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 6),
            TabStop = false,
        };

        var lblTargetWarning = new Label
        {
            Name = "lblTargetWarning",
            Dock = DockStyle.Top,
            AutoSize = true,
            ForeColor = Color.DarkRed,
            Margin = new Padding(0, 0, 0, 8),
            Visible = false,
        };

        var listTargets = new DataGridView
        {
            Name = "listTargets",
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            MinimumSize = new Size(300, 220),
            Margin = new Padding(0),
            TabIndex = 7,
            AccessibleName = Localization.Text("TargetsGridAccessibleName"),
            AccessibleDescription = Localization.Text("TargetsGridAccessibleDescription"),
        };

        listTargets.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = Localization.Text("ColumnName"), ReadOnly = true });
        listTargets.Columns.Add(new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = Localization.Text("ColumnFileName"), ReadOnly = true });
        listTargets.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProcessName", HeaderText = Localization.Text("ColumnProcessName"), ReadOnly = true });
        listTargets.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = Localization.Text("ColumnEnabled"), ReadOnly = true });

        listTargets.DoubleClick += (_, _) => EditSelectedTarget();

        var targetButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 0, 0, 8),
            Margin = new Padding(0),
        };

        var btnAdd = new Button
        {
            Text = Localization.Text("ButtonAddTarget"),
            Width = 120,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
            TabIndex = 4,
            AccessibleName = Localization.Text("AddTargetAccessibleName"),
            AccessibleDescription = Localization.Text("AddTargetAccessibleDescription"),
        };
        var btnEdit = new Button
        {
            Text = Localization.Text("ButtonEditTarget"),
            Width = 120,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
            TabIndex = 5,
            AccessibleName = Localization.Text("EditTargetAccessibleName"),
            AccessibleDescription = Localization.Text("EditTargetAccessibleDescription"),
        };
        var btnDelete = new Button
        {
            Text = Localization.Text("ButtonDeleteTarget"),
            Width = 120,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 4),
            TabIndex = 6,
            AccessibleName = Localization.Text("DeleteTargetAccessibleName"),
            AccessibleDescription = Localization.Text("DeleteTargetAccessibleDescription"),
        };

        btnAdd.Click += (_, _) => AddTarget();
        btnEdit.Click += (_, _) => EditSelectedTarget();
        btnDelete.Click += (_, _) => DeleteSelectedTarget();

        targetButtonsPanel.Controls.Add(btnAdd);
        targetButtonsPanel.Controls.Add(btnEdit);
        targetButtonsPanel.Controls.Add(btnDelete);

        mainLayout.Controls.Add(settingsLayout, 0, 0);
        mainLayout.Controls.Add(lblTargets, 0, 1);
        mainLayout.Controls.Add(targetButtonsPanel, 0, 2);
        mainLayout.Controls.Add(lblTargetWarning, 0, 3);
        mainLayout.Controls.Add(listTargets, 0, 4);

        Controls.Add(mainLayout);
        Controls.Add(footerPanel);

        Text = $"{AppConstants.DisplayName} - {Localization.Text("SettingsTitle")}";
        Width = 980;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(900, 620);
        AutoScaleMode = AutoScaleMode.Dpi;
        Icon = AppIconProvider.Load();
        ShowIcon = true;
        AccessibleName = Localization.Text("SettingsFormAccessibleName");
        AccessibleDescription = Localization.Text("SettingsFormAccessibleDescription");

        AcceptButton = btnOK;
        CancelButton = btnCancel;

        btnOK.Click += BtnOK_Click;
        ResumeLayout(false);
    }

    private void PopulateForm()
    {
        var numMousePoll = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "numMousePoll");
        var numCheckTime = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "numCheckTime");
        var listTargets = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<DataGridView>().FirstOrDefault(c => c.Name == "listTargets");

        if (numMousePoll is not null) numMousePoll.Value = _settings.MousePollSeconds;
        if (numCheckTime is not null) numCheckTime.Value = _settings.ActivityEvaluationMinutes;

        if (listTargets is not null)
        {
            listTargets.Rows.Clear();
            foreach (var target in _settings.Targets)
            {
                listTargets.Rows.Add(
                    target.Name,
                    target.FileName,
                    target.ProcessName ?? Localization.Text("AutoProcessName"),
                    target.Enabled
                );
            }
        }
    }

    private void AddTarget()
    {
        using var dialog = new TargetEditDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings.Targets.Add(dialog.Target);
            RefreshTargetList();
        }
    }

    private void EditSelectedTarget()
    {
        var listTargets = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<DataGridView>().FirstOrDefault(c => c.Name == "listTargets");

        if (listTargets?.SelectedRows.Count != 1)
        {
            MessageBox.Show(Localization.Text("SelectTargetToEditMessage"), Localization.Text("SelectTargetCaption"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedIndex = listTargets.SelectedRows[0].Index;
        if (selectedIndex < 0 || selectedIndex >= _settings.Targets.Count)
        {
            return;
        }

        var targetToEdit = _settings.Targets[selectedIndex];
        using var dialog = new TargetEditDialog(targetToEdit);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings.Targets[selectedIndex] = dialog.Target;
            RefreshTargetList();
        }
    }

    private void DeleteSelectedTarget()
    {
        var listTargets = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<DataGridView>().FirstOrDefault(c => c.Name == "listTargets");

        if (listTargets?.SelectedRows.Count != 1)
        {
            MessageBox.Show(Localization.Text("SelectTargetToDeleteMessage"), Localization.Text("SelectTargetCaption"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedIndex = listTargets.SelectedRows[0].Index;
        if (selectedIndex < 0 || selectedIndex >= _settings.Targets.Count)
        {
            return;
        }

        var selectedTarget = _settings.Targets[selectedIndex];
        if (_settings.Targets.Count == 1 && TargetAvailabilityService.IsPythonTarget(selectedTarget))
        {
            MessageBox.Show(Localization.Text("ProtectedPythonDeleteMessage"), Localization.Text("ProtectedTargetCaption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var targetName = selectedTarget.Name;
        if (MessageBox.Show(
            Localization.Format("ConfirmDeleteMessage", targetName),
            Localization.Text("ConfirmDeleteCaption"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _settings.Targets.RemoveAt(selectedIndex);
            RefreshTargetList();
        }
    }

    private void RefreshTargetList()
    {
        var listTargets = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
            .OfType<DataGridView>().FirstOrDefault(c => c.Name == "listTargets");

        if (listTargets is not null)
        {
            listTargets.Rows.Clear();
            foreach (var target in _settings.Targets)
            {
                var rowIndex = listTargets.Rows.Add(
                    target.Name,
                    target.FileName,
                    target.ProcessName ?? Localization.Text("AutoProcessName"),
                    target.Enabled
                );

                if (!TargetAvailabilityService.IsTargetAvailable(target))
                {
                    var row = listTargets.Rows[rowIndex];
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    row.ErrorText = Localization.Text("TargetAvailabilityCaption");
                }
            }
        }

        ValidateConfiguredTargets(showDialog: false);
    }

    private async void BtnOK_Click(object? sender, EventArgs e)
    {
        try
        {
            var numMousePoll = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                .OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "numMousePoll");
            var numCheckTime = Controls.Cast<Control>().SelectMany(c => GetAllControls(c))
                .OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "numCheckTime");

            if (numMousePoll is not null) _settings.MousePollSeconds = (int)numMousePoll.Value;
            if (numCheckTime is not null) _settings.ActivityEvaluationMinutes = (int)numCheckTime.Value;

            await _saveSettingsCallback(_settings, _configPath);
            _logger.LogInfo("Settings saved from UI.");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving settings.", ex);
            MessageBox.Show(Localization.Format("ErrorSavingSettingsMessage", ex.Message), Localization.Text("ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ValidateConfiguredTargets(bool showDialog)
    {
        var missing = TargetAvailabilityService.GetMissingTargets(_settings.Targets);
        var missingPython = missing.Any(target => target.IsPython);

        if (missingPython && !_pythonInstallPrompted)
        {
            _pythonInstallPrompted = true;
            if (PythonInstaller.EnsurePythonInstalledWithPrompt(this, _logger))
            {
                RefreshTargetList();
                missing = TargetAvailabilityService.GetMissingTargets(_settings.Targets);
            }
        }

        var warningLabel = Controls.Cast<Control>().SelectMany(GetAllControls)
            .OfType<Label>().FirstOrDefault(label => label.Name == "lblTargetWarning");

        if (warningLabel is not null)
        {
            if (missing.Count == 0)
            {
                warningLabel.Visible = false;
                warningLabel.Text = string.Empty;
            }
            else
            {
                var summary = string.Join(", ", missing.Select(target => $"{target.Name} ({target.FileName})"));
                warningLabel.Text = Localization.Format("TargetsUnavailableWarningFormat", summary);
                warningLabel.Visible = true;
            }
        }

        if (showDialog && missing.Count > 0)
        {
            var summary = string.Join(", ", missing.Select(target => $"{target.Name} ({target.FileName})"));
            MessageBox.Show(
                this,
                Localization.Format("TargetsUnavailableDialogFormat", summary),
                Localization.Text("TargetAvailabilityCaption"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
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
