using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.Putty
{
	/// <summary>
	/// Interaction logic for PuttySettings.xaml
	/// </summary>
	public partial class PuttySettings : UserControl
	{
		private Settings settings;

		public PuttySettings(Settings settings) {
			InitializeComponent();
			this.settings = settings;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			BindBooleanToCheckbox(AddPuttyExeInResults, () => settings.AddPuttyExeToResults, (v) => settings.AddPuttyExeToResults = v);
			BindBooleanToCheckbox(AlwaysStartsSessionMaximized, () => settings.AlwaysStartsSessionMaximized, (v) => settings.AlwaysStartsSessionMaximized = v);
			PuttyFilePath.Text = settings.PuttyPath;
		}

		private void BindBooleanToCheckbox(CheckBox checkBox, Func<bool> readBool, Action<bool> writeBool) {
			checkBox.IsChecked = readBool();

			checkBox.Checked += (o, ev) => {
				writeBool(true);
				settings.OnSettingsChanged?.Invoke(settings);
			};

			checkBox.Unchecked += (o, ev) => {
				writeBool(false);
				settings.OnSettingsChanged?.Invoke(settings);
			};
		}

		private void btnOpenFile_Click(object sender, RoutedEventArgs e) {
			// Create OpenFileDialog
			OpenFileDialog openFileDlg = new OpenFileDialog();

			// Launch OpenFileDialog by calling ShowDialog method
			var result = openFileDlg.ShowDialog();
			// Get the selected file name and display in a TextBox.
			// Load content of file in a TextBlock
			if(result == true)
			{
				PuttyFilePath.Text = openFileDlg.FileName;
				settings.PuttyPath = openFileDlg.FileName;
				settings.Save();
			}
		}
	}
}