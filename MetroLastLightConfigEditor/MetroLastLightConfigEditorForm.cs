using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MetroLastLightConfigEditor
{
    public partial class MetroLastLightConfigEditorForm : Form
    {
        private bool _skipIntroInitialState = false;

        public MetroLastLightConfigEditorForm()
        {
            InitializeComponent();
            AddTooltips();

            // Check for update
            backgroundWorker.RunWorkerAsync();
        }

        private void MetroLastLightConfigEditorForm_Shown(object sender, EventArgs e)
        {
            refreshUI();
        }

        private void MetroLastLightConfigEditorForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (HaveSettingsChanged())
            {
                DialogResult result = MessageBox.Show("You have unsaved changes, do you want to keep them?", "Save",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                    buttonSave.PerformClick();

                // Do not close the form if the user pressed Cancel
                e.Cancel = result == DialogResult.Cancel;
            }

            Logger.WriteToFile();
        }

        private void AddTooltips()
        {
            toolTip.SetToolTip(checkBoxSkipIntro, "Skips intro logos partially.");
            toolTip.SetToolTip(checkBoxShowFPS, "Displays a framerate in the top right corner.");
            toolTip.SetToolTip(spinnerFov, "Changes ingame FOV. Default FOV is 50.625. Increasing it will result in some graphical" +
                " glitches due to the use of 3D-rendered screen overlays in certain parts of the game.");
            toolTip.SetToolTip(checkBoxFullscreen, "Uncheck to play the game in windowed mode. To play borderless fullscreen, change" +
                " your resolution to your native resolution.\nPlease note that the game was never meant to be played windowed so the" +
                " taskbar will still be visible.");
            toolTip.SetToolTip(checkBoxVsync, "By default, Metro: Last Light apparently runs in Stereoscopic 3D which can impact" +
                "performance. \nFor some reason, enabling Vsync will disable stereoscopy, thus boosting your framerate.");
        }

        private void ComboBoxQuality_SelectedLow()
        {
            labelQualityMotionBlurValue.Text        = "Disabled";
            labelSkinShadingValue.Text              = "Disabled";
            labelBumpMappingValue.Text              = "Coarse";
            labelSoftParticlesValue.Text            = "Disabled";
            labelShadowResolutionValue.Text         = "2.35 Mpix";
            labelLightMaterialInteractionValue.Text = "Normal";
            labelGeometricDetailValue.Text          = "Low";
            labelDetailTexturingValue.Text          = "Disabled";
            labelAmbientOcclusionValue.Text         = "Approximate";
            labelImagePostProcessingValue.Text      = "Normal";
            labelParallaxMappingValue.Text          = "Disabled";
            labelShadowFilteringValue.Text          = "Fast";
            labelAnalyticalAntiAliasingValue.Text   = "Enabled";
            labelVolumetricTexturingValue.Text      = "Disabled";
        }

        private void ComboBoxQuality_SelectedMedium()
        {
            labelQualityMotionBlurValue.Text        = "Disabled";
            labelSkinShadingValue.Text              = "Disabled";
            labelBumpMappingValue.Text              = "Coarse";
            labelSoftParticlesValue.Text            = "Disabled";
            labelShadowResolutionValue.Text         = "4.19 Mpix";
            labelLightMaterialInteractionValue.Text = "Normal";
            labelGeometricDetailValue.Text          = "Normal";
            labelDetailTexturingValue.Text          = "Enabled";
            labelAmbientOcclusionValue.Text         = "Approximate";
            labelImagePostProcessingValue.Text      = "Normal";
            labelParallaxMappingValue.Text          = "Disabled";
            labelShadowFilteringValue.Text          = "Normal";
            labelAnalyticalAntiAliasingValue.Text   = "Enabled";
            labelVolumetricTexturingValue.Text      = "Disabled";
        }

        private void ComboBoxQuality_SelectedHigh()
        {
            labelQualityMotionBlurValue.Text        = "Camera";
            labelSkinShadingValue.Text              = "Simple";
            labelBumpMappingValue.Text              = "Precise";
            labelSoftParticlesValue.Text            = "Enabled";
            labelShadowResolutionValue.Text         = "6.55 Mpix";
            labelLightMaterialInteractionValue.Text = "Normal";
            labelGeometricDetailValue.Text          = "High";
            labelDetailTexturingValue.Text          = "Enabled";
            labelAmbientOcclusionValue.Text         = "Precomputed + SSAO";
            labelImagePostProcessingValue.Text      = "Full";
            labelParallaxMappingValue.Text          = "Enabled";
            labelShadowFilteringValue.Text          = "Hi-quality";
            labelAnalyticalAntiAliasingValue.Text   = "Enabled";
            labelVolumetricTexturingValue.Text      = "Low-precision, disabled for sun";
        }

        private void ComboBoxQuality_SelectedVeryHigh()
        {
            labelQualityMotionBlurValue.Text        = "Camera + objects (DX10+)";
            labelSkinShadingValue.Text              = "Sub-scattering";
            labelBumpMappingValue.Text              = "Precise";
            labelSoftParticlesValue.Text            = "Enabled";
            labelShadowResolutionValue.Text         = "9.43 Mpix";
            labelLightMaterialInteractionValue.Text = "Full";
            labelGeometricDetailValue.Text          = "Very high";
            labelDetailTexturingValue.Text          = "Enabled";
            labelAmbientOcclusionValue.Text         = "Precomputed + SSAO";
            labelImagePostProcessingValue.Text      = "Full";
            labelParallaxMappingValue.Text          = "Enabled with occlusion";
            labelShadowFilteringValue.Text          = "Hi-quality";
            labelAnalyticalAntiAliasingValue.Text   = "Enabled";
            labelVolumetricTexturingValue.Text      = "Full quality + sun shafts";
        }

        private bool HaveSettingsChanged()
        {
            // Nothing to compare if the config file wasn't found
            if (Helper.instance.ConfigFilePath == null)
                return false;

            // Write changes in a separate dictionary to detect changes
            WriteSettings(Helper.instance.DictionaryUponClosure);

            // Check if non-dictionary settings have changed
            if (checkBoxSkipIntro.Checked != _skipIntroInitialState)
                return true;

            // Check if settings in dictionaries have changed
            return !Helper.instance.AreDictionariesEqual();
        }

        private void ReadSettings()
        {
            Helper.instance.AddKeysIfMissing();
            _skipIntroInitialState            = Helper.instance.IsNoIntroSkipped;

            // Checkboxes
            checkBoxSubtitles.Checked         = Helper.instance.Dictionary["_show_subtitles"]   == "1";
            checkBoxLaserCrosshair.Checked    = Helper.instance.Dictionary["g_laser"]           == "1";
            checkBoxHints.Checked             = Helper.instance.Dictionary["g_quick_hints"]     == "1";
            checkBoxCrosshair.Checked         = Helper.instance.Dictionary["g_show_crosshair"]  == "on";
            checkBoxShowFPS.Checked           = Helper.instance.Dictionary["fps"]               == "on";
            checkBoxSkipIntro.Checked         = Helper.instance.IsNoIntroSkipped;
            checkBoxInvertYAxis.Checked       = Helper.instance.Dictionary["invert_y_axis"]     == "on";
            checkBoxAimAssistance.Checked     = Helper.instance.Dictionary["aim_assist"]        == "1.";
            checkBoxAdvancedPhysX.Checked     = Helper.instance.Dictionary["ph_advanced_physX"] == "1";
            checkBoxFullscreen.Checked        = Helper.instance.Dictionary["r_fullscreen"]      == "on";
            checkBoxVsync.Checked             = Helper.instance.Dictionary["r_vsync"]           == "on";

            // Comboboxes
            comboBoxDifficulty.Text           = Helper.instance.ConvertNumberToDifficulty(Helper.instance.Dictionary["g_game_difficulty"]);
            comboBoxVoiceLanguage.Text        = Helper.instance.ConvertCodeToLanguage(Helper.instance.Dictionary["lang_sound"]);
            comboBoxTextLanguage.Text         = Helper.instance.ConvertCodeToLanguage(Helper.instance.Dictionary["lang_text"]);
            comboBoxTextureFiltering.Text     = Helper.instance.ConvertNumberToTextureFiltering(Helper.instance.Dictionary["r_af_level"]);
            comboBoxDirectX.Text              = Helper.instance.ConvertNumberToDirectX(Helper.instance.Dictionary["r_api"]);
            comboBoxMotionBlur.Text           = Helper.instance.ConvertNumberToMotionBlurLevel(Helper.instance.Dictionary["r_blur_level"]);
            comboBoxQuality.Text              = Helper.instance.ConvertNumberToQualityLevel(Helper.instance.Dictionary["r_quality_level"]);
            comboBoxSSAA.Text                 = Helper.instance.ConvertNumberToSSAA(Helper.instance.Dictionary["r_supersample"]);
            comboBoxTessellation.Text         = Helper.instance.ConvertNumbersToTessellation(Helper.instance.Dictionary["r_dx11_tess"],
                Helper.instance.Dictionary["r_tess_ss"]);
            string resolution                 = $"{Helper.instance.Dictionary["r_res_hor"]} x {Helper.instance.Dictionary["r_res_vert"]}";
            comboBoxResolution.Text           = comboBoxResolution.Items.Contains(resolution) ? resolution : "Custom resolution";

            // Spinners
            try
            {
                spinnerMouseSensitivity.Value = Decimal.Parse(Helper.instance.Dictionary["sens"]);
                spinnerMasterVolume.Value     = Decimal.Parse(Helper.instance.Dictionary["s_master_volume"]);
                spinnerMusicVolume.Value      = Decimal.Parse(Helper.instance.Dictionary["s_music_volume"]);
                spinnerDialogsVolume.Value    = Decimal.Parse(Helper.instance.Dictionary["s_dialogs_volume"]);
                spinnerEffectsVolume.Value    = Decimal.Parse(Helper.instance.Dictionary["s_effects_volume"]);
                spinnerGamma.Value            = Decimal.Parse(Helper.instance.Dictionary["r_gamma"]);
                spinnerFov.Value              = Decimal.Parse(Helper.instance.Dictionary["r_base_fov"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}\n\nSetting default values for volume, sensitivity, gamma and FOV.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.WriteInformation<Helper>(ex.Message);
            }
        }

        private void refreshUI()
        {
            // Set textboxes
            textBoxSteamInstallPath.Text   = Helper.instance.SteamInstallPath ?? "Steam not found";
            textBoxConfigFilePath.Text     = Helper.instance.ConfigFilePath ?? "Config not found";
            textBoxGameExecutablePath.Text = Helper.instance.GameExecutablePath ?? "Game not found";
            textBoxSavedGamesPath.Text     = Helper.instance.SavedGamesPath;

            // Set button states
            buttonReload.Enabled           = Helper.instance.ConfigFilePath != null;
            buttonSave.Enabled             = Helper.instance.ConfigFilePath != null;
            buttonStartGameNoSteam.Enabled = Helper.instance.GameInstallPath != null;
            buttonStartGameSteam.Enabled   = Helper.instance.SteamInstallPath != null;

            if (Helper.instance.ConfigFilePath != null)
            {
                fileSystemWatcherConfig.Path = new FileInfo(Helper.instance.ConfigFilePath).DirectoryName;

                // Read the config file
                buttonReload.PerformClick();
            }
            else
            {
                string text = String.Format("{0}\n\n{1}\n\n{2}{3}",
                    "We were not able to locate the config file for Metro: Last Light, please run the game at least once to generate it.",
                    "You can also point to its location by using the corresponding Browse button. It should be located here:",
                    @"%LOCALAPPDATA%\4A Games\Metro LL\<user-id>\",
                    Helper.instance.SteamInstallPath != null ? "\n\nDo you want to run the game now?" : "");

                if (MessageBox.Show(text, "Config not found", Helper.instance.SteamInstallPath != null ? MessageBoxButtons.YesNo :
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    buttonStartGameSteam.PerformClick();
            }

            if (Helper.instance.GameInstallPath != null)
                fileSystemWatcherNoIntro.Path = Helper.instance.GameInstallPath;
        }

        private void StartProcess(object path)
        {
            ProcessStartInfo pathStartInfo = path is ProcessStartInfo ? (ProcessStartInfo)path : new ProcessStartInfo(path.ToString());

            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo = pathStartInfo;
                    proc.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a problem opening the following process:\n\n{pathStartInfo.FileName}",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.WriteInformation<Helper>(ex.Message, pathStartInfo.FileName);
            }
        }

        private void WriteSettings(Dictionary<string, string> dictionary)
        {
            // Checkboxes
            dictionary["_show_subtitles"]   = checkBoxSubtitles.Checked ? "1" : "0";
            dictionary["g_laser"]           = checkBoxLaserCrosshair.Checked ? "1" : "0";
            dictionary["g_quick_hints"]     = checkBoxHints.Checked ? "1" : "0";
            dictionary["g_show_crosshair"]  = checkBoxCrosshair.Checked ? "on" : "off";
            dictionary["fps"]               = checkBoxShowFPS.Checked ? "on" : "off";
            dictionary["invert_y_axis"]     = checkBoxInvertYAxis.Checked ? "on" : "off";
            dictionary["aim_assist"]        = checkBoxAimAssistance.Checked ? "1." : "0.";
            dictionary["ph_advanced_physX"] = checkBoxAdvancedPhysX.Checked ? "1" : "0";
            dictionary["r_fullscreen"]      = checkBoxFullscreen.Checked ? "on" : "off";
            dictionary["r_vsync"]           = checkBoxVsync.Checked ? "on" : "off";

            // Comboboxes
            dictionary["g_game_difficulty"] = Helper.instance.ConvertDifficultyToNumber(comboBoxDifficulty.Text);
            dictionary["lang_sound"]        = Helper.instance.ConvertLanguageToCode(comboBoxVoiceLanguage.Text);
            dictionary["lang_text"]         = Helper.instance.ConvertLanguageToCode(comboBoxTextLanguage.Text);
            dictionary["r_af_level"]        = Helper.instance.ConvertTextureFilteringToNumber(comboBoxTextureFiltering.Text);
            dictionary["r_api"]             = Helper.instance.ConvertDirectXToNumber(comboBoxDirectX.Text);
            dictionary["r_blur_level"]      = Helper.instance.ConvertMotionBlurLevelToNumber(comboBoxMotionBlur.Text);
            dictionary["r_quality_level"]   = Helper.instance.ConvertQualityLevelToNumber(comboBoxQuality.Text);
            dictionary["r_supersample"]     = Helper.instance.ConvertSSAAToNumber(comboBoxSSAA.Text);
            dictionary["r_dx11_tess"]       = Helper.instance.ConvertTessellationToNumbers(comboBoxTessellation.Text)[0];
            dictionary["r_tess_ss"]         = Helper.instance.ConvertTessellationToNumbers(comboBoxTessellation.Text)[1];

            // Spinners
            dictionary["sens"]              = spinnerMouseSensitivity.Value.Equals(1) ? "1." : spinnerMouseSensitivity.Value.ToString();
            dictionary["s_master_volume"]   = spinnerMasterVolume.Value.ToString("F2");
            dictionary["s_music_volume"]    = spinnerMusicVolume.Value.ToString("F2");
            dictionary["s_dialogs_volume"]  = spinnerDialogsVolume.Value.ToString("F2");
            dictionary["s_effects_volume"]  = spinnerEffectsVolume.Value.ToString("F2");
            dictionary["r_gamma"]           = spinnerGamma.Value.Equals(1) ? "1." : spinnerGamma.Value.ToString();
            dictionary["r_base_fov"]        = spinnerFov.Value.ToString();

            // Textboxes
            dictionary["r_res_hor"]         = textBoxWidth.Text;
            dictionary["r_res_vert"]        = textBoxHeight.Text;
        }

        // Event handlers
        private void ButtonBrowseSteamInstallPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Steam executable|Steam.exe";
                openFileDialog.InitialDirectory = Helper.instance.SteamInstallPath ??
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                // Show a file browser
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Find config/game paths and reload config automatically
                    Helper.instance.SteamInstallPath = new FileInfo(openFileDialog.FileName).DirectoryName.ToLower();
                    Helper.instance.UpdateConfigAndGamePaths();
                    refreshUI();
                }
            }
        }

        private void ButtonBrowseConfigFilePath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Metro: Last Light config file|user.cfg";

                // Pick config folder, then Steam folder and finally Program Files
                openFileDialog.InitialDirectory = Helper.instance.ConfigFilePath != null ?
                    new FileInfo(Helper.instance.ConfigFilePath).DirectoryName : Helper.instance.SteamInstallPath != null ?
                    Path.Combine(Helper.instance.SteamInstallPath, "userdata") :
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                // Show a file browser
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Reload config automatically
                    Helper.instance.ConfigFilePath = openFileDialog.FileName.ToLower();
                    refreshUI();
                }
            }
        }

        private void ButtonBrowseGameExecutable_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Metro: Last Light executable|MetroLL.exe";
                openFileDialog.InitialDirectory = Helper.instance.GameInstallPath;

                // Show a file browser
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Update UI
                    Helper.instance.GameInstallPath = new FileInfo(openFileDialog.FileName).DirectoryName.ToLower();
                    Helper.instance.GameExecutablePath = openFileDialog.FileName.ToLower();
                    refreshUI();
                }
            }
        }

        private void ButtonOpenSavedGamesPath_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Helper.instance.SavedGamesPath))
                StartProcess(Helper.instance.SavedGamesPath);
            else
                StartProcess(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void ComboBoxResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the content of the width/height textboxes according to selected resolution
            if (comboBoxResolution.Text != "Custom resolution")
            {
                string[] splitResolution = comboBoxResolution.Text.Split(new string[] { " x " }, StringSplitOptions.None);
                textBoxWidth.Text        = splitResolution[0];
                textBoxHeight.Text       = splitResolution[1];
            }
            else
            {
                textBoxWidth.Text = Helper.instance.Dictionary["r_res_hor"];
                textBoxHeight.Text = Helper.instance.Dictionary["r_res_vert"];

                // Automatically give focus to the width textbox when necessary
                if (comboBoxResolution.Focused)
                    textBoxWidth.Focus();
            }
        }

        private void ComboBoxQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxQuality.SelectedIndex == 0)
                ComboBoxQuality_SelectedLow();
            else if (comboBoxQuality.SelectedIndex == 1)
                ComboBoxQuality_SelectedMedium();
            else if (comboBoxQuality.SelectedIndex == 2)
                ComboBoxQuality_SelectedHigh();
            else
                ComboBoxQuality_SelectedVeryHigh();
        }

        private void ComboBoxDirectX_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Disable DX11 features in DX9/10
            comboBoxTessellation.Enabled = comboBoxDirectX.Text == "DirectX 11";
        }

        private void TextBoxResolution_KeyPress(object sender, KeyPressEventArgs e)
        {
            // User can type digits only
            if (e.KeyChar == (char)Keys.Enter || !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
            else
                comboBoxResolution.SelectedItem = "Custom resolution";
        }

        private void ButtonReportBug_Click(object sender, EventArgs e)
        {
            StartProcess("https://github.com/GenesisFR/MetroLastLightConfigEditor/issues");
        }

        private void ButtonDonate_Click(object sender, EventArgs e)
        {
            StartProcess("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=EYBPZ5JT9WUNS");
        }

        private void LinkLabelUpdateAvailable_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelUpdateAvailable.LinkVisited = true;
            StartProcess("https://github.com/GenesisFR/MetroLastLightConfigEditor/releases/latest");
        }

        private void ButtonReload_Click(object sender, EventArgs e)
        {
            Helper.instance.ReadConfigFile();

            // Necessary string formatting for FOV and sensitivity (ex: 50. -> 50.000)
            Helper.instance.Dictionary["r_base_fov"]            = Helper.instance.Dictionary["r_base_fov"].PadRight(6, '0');
            Helper.instance.Dictionary["sens"]                  = Helper.instance.Dictionary["sens"].PadRight(5, '0');
            Helper.instance.DictionaryUponClosure["r_base_fov"] = Helper.instance.Dictionary["r_base_fov"];
            Helper.instance.DictionaryUponClosure["sens"]       = Helper.instance.Dictionary["sens"];

            ReadSettings();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Don't watch for file changes during the saving process
                fileSystemWatcherConfig.EnableRaisingEvents = false;
                fileSystemWatcherNoIntro.EnableRaisingEvents = false;

                WriteSettings(Helper.instance.Dictionary);
                _skipIntroInitialState = checkBoxSkipIntro.Checked;

                if (Helper.instance.WriteConfigFile())
                    MessageBox.Show("The config file has been saved successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Unable to save the config file. Try running the program as admin?",
                        "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (!Helper.instance.BackupIntroFile(checkBoxSkipIntro.Checked))
                    MessageBox.Show(string.Format("{0}{1}{2}{3}{4}",
                        "Unable to ",
                        checkBoxSkipIntro.Checked ? "backup" : "restore",
                        " the intro file. Make sure the game executable path has been specified and there's a file named ",
                        checkBoxSkipIntro.Checked ? "\"legal.ogv\"" : "\"legal.ogv.bak\"",
                        " in the game directory."
                        ),
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message, e.ToString());
            }
            finally
            {
                // Saving process done, watch for file changes again
                fileSystemWatcherConfig.EnableRaisingEvents = true;
                fileSystemWatcherNoIntro.EnableRaisingEvents = Helper.instance.IsNoIntroSkipped;
            }
        }

        private void ButtonStartGameNoSteam_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Helper.instance.GameExecutablePath);
            startInfo.WorkingDirectory = Helper.instance.GameInstallPath;
            StartProcess(startInfo);
        }

        private void ButtonStartGameSteam_Click(object sender, EventArgs e)
        {
            StartProcess("steam://run/43160");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Start timing
            Stopwatch stopwatch = Stopwatch.StartNew();

            e.Result = Helper.instance.IsUpdateAvailable();

            // Report time
            stopwatch.Stop();
            Console.WriteLine($"Update check done in {stopwatch.Elapsed.TotalMilliseconds} ms");
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            linkLabelUpdateAvailable.Visible = (bool)e.Result;
        }

        private void FileSystemWatcherConfig_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Prevents a double firing, known issue for FileSystemWatcher
                fileSystemWatcherConfig.EnableRaisingEvents = false;

                // Wait until the file is accessible
                while (!Helper.instance.IsFileReady(e.FullPath))
                    Console.WriteLine("File locked by another process");

                if (MessageBox.Show("The config file has been modified by another program. Do you want to reload it?", "Reload",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    buttonReload.PerformClick();
            }
            catch (Exception ex)
            {
                Logger.WriteInformation<Helper>(ex.Message, e.ToString());
            }
            finally
            {
                fileSystemWatcherConfig.EnableRaisingEvents = true;
            }
        }

        private void FileSystemWatcherNoIntro_Changed(object sender, FileSystemEventArgs e)
        {
            buttonReload.PerformClick();
        }
    }
}
