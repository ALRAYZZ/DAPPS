using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AppHub
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private List<string> appPaths = new List<string>();
		public MainWindow()
        {
            this.InitializeComponent();
            LoadAppsList();
        }

		private async void btnAddAppOnClick(object sender, RoutedEventArgs e)
        {
			var filePicker = new FileOpenPicker();

			InitializeWithWindow(filePicker);

			filePicker.ViewMode = PickerViewMode.List;
			filePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			filePicker.FileTypeFilter.Add(".exe");

			StorageFile file = await filePicker.PickSingleFileAsync();

			if (file != null)
			{
				string appPath = file.Path;

				if (!appPaths.Contains(appPath))
				{
					appPaths.Add(appPath);
					listApps.ItemsSource = null; // Refresh the list
					listApps.ItemsSource = appPaths;
					MessageBox("App added successfully");
					SaveAppsList();
				}
				else
				{
					MessageBox("The selected app is already in the list");
				}
			}
			else
			{
				MessageBox("No app selected");
			}
		}

		private void btnLaunchAppOnClick(object sender, RoutedEventArgs e)
		{
			if (listApps.SelectedItem != null)
			{
				string selectedAppPath = listApps.SelectedItem.ToString();

				if (File.Exists(selectedAppPath))
				{
					try
					{
						Process.Start(new ProcessStartInfo(selectedAppPath) { UseShellExecute = true });
						MessageBox("App launched successfully");
					}
					catch (Exception ex)
					{
						MessageBox($"An error occurred while launching the app {ex.Message}");
					}
				}
				else
				{
					MessageBox("The selected app does not exist");
				}
			}
			else
			{
				MessageBox("Please select an app to launch");
			}
		}

		private async void MessageBox(string message)
		{
			var dialog = new ContentDialog
			{
				Title = "Notification",
				Content = message,
				CloseButtonText = "Ok",
				XamlRoot = this.Content.XamlRoot
			};

			await dialog.ShowAsync();
		}
		private void LoadAppsList()
		{
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "apps.txt");

			if (File.Exists(filePath))
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					while (!reader.EndOfStream) // Read the file line by line until the end of the file
					{
						string appPath = reader.ReadLine().Trim(); // Each line contains the path of an app

						if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
						{
							appPaths.Add(appPath); // If the path is valid, add it to the list
						}
					}
				}
			}
		}
		private void SaveAppsList()
		{
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "apps.txt");

			using (StreamWriter writer = new StreamWriter(filePath))
			{
				foreach (string appPath in appPaths)
				{
					writer.WriteLine(appPath);
				}
			}
		}
		private void InitializeWithWindow(FileOpenPicker filePicker)
		{
			// We need to initialize the FileOpenPicker with the window handle of the current window
			// So the file picker knows which window to attach to
			IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
		}

	}
}
