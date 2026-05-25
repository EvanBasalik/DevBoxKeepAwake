using System.Diagnostics;

namespace DevBoxKeepAwake;

internal static class PythonInstaller
{
    public static bool EnsurePythonInstalledWithPrompt(IWin32Window? owner, FileLogger logger)
    {
        if (TargetAvailabilityService.IsPythonAvailable())
        {
            return true;
        }

        var result = MessageBox.Show(
            owner,
            Localization.Text("PythonInstallPromptMessage"),
            AppConstants.DisplayName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            logger.LogInfo("Python installation prompt declined by user.");
            return false;
        }

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = "install --id Python.Python.3.12 -e --source winget --accept-source-agreements --accept-package-agreements",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal,
            });

            if (process is null)
            {
                logger.LogError("Failed to start winget process for Python install.");
                MessageBox.Show(owner, Localization.Text("PythonInstallFailedMessage"), Localization.Text("ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            process.WaitForExit();

            if (TargetAvailabilityService.IsPythonAvailable())
            {
                logger.LogInfo("Python detected after installation attempt.");
                MessageBox.Show(owner, Localization.Text("PythonInstallSuccessMessage"), AppConstants.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }

            logger.LogError("Python not found after winget installation attempt.");
            MessageBox.Show(owner, Localization.Text("PythonInstallFailedMessage"), Localization.Text("ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("Python installation attempt failed.", ex);
            MessageBox.Show(owner, Localization.Format("PythonInstallErrorMessage", ex.Message), Localization.Text("ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
}