using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using AppHub.Models;
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
        private List<AppModel> appList = new List<AppModel>();
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
				string appName = Path.GetFileNameWithoutExtension(appPath);

				if (!appList.Any(a => a.AppPath == appPath))
				{
					var iconPicker = new FileOpenPicker();
					InitializeWithWindow(iconPicker);
					iconPicker.ViewMode = PickerViewMode.Thumbnail;
					iconPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
					iconPicker.FileTypeFilter.Add(".png");
					iconPicker.FileTypeFilter.Add(".jpg");
					iconPicker.FileTypeFilter.Add(".jpeg");

					StorageFile iconFile = await iconPicker.PickSingleFileAsync();
					string appIcon = iconFile != null ? iconFile.Path : "Assets/default_icon.png";


					var app = new AppModel { AppName = appName, AppPath = appPath, AppIcon = appIcon };
					appList.Add(app);
					listApps.ItemsSource = null; // Refresh the list
					listApps.ItemsSource = appList;
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
				var selectedApp = (AppModel)listApps.SelectedItem;

				if (File.Exists(selectedApp.AppPath))
				{
					try
					{
						Process.Start(new ProcessStartInfo(selectedApp.AppPath) { UseShellExecute = true });
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
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DAPPS");
			string filePath = Path.Combine(folderPath, "apps.txt");

			if (Directory.Exists(folderPath) && File.Exists(filePath))
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					while (!reader.EndOfStream) // Read the file line by line until the end of the file
					{
						string line = reader.ReadLine().Trim(); // Each line contains the path of an app
						var parts = line.Split('|'); // The path is separated from the app name by a pipe character
						if (parts.Length >= 2)
						{
							string appName = parts[0];
							string appPath = parts[1];
							string appIcon = (parts.Length == 3 && !string.IsNullOrEmpty(parts[2])) ? parts[2] : "Assets/default_icon.png";

							var app = new AppModel { AppName = appName, AppPath = appPath, AppIcon = appIcon };
							if (File.Exists(app.AppPath))
							{
								appList.Add(app);
							}
						}
					}
				}
			}
			else
			{
				Directory.CreateDirectory(folderPath);
			}
			listApps.ItemsSource = null;
			listApps.ItemsSource = appList;
		}
		private void SaveAppsList()
		{
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DAPPS");
			string filePath = Path.Combine(folderPath, "apps.txt");

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			using (StreamWriter writer = new StreamWriter(filePath))
			{
				foreach (var app in appList)
				{
					writer.WriteLine($"{app.AppName}|{app.AppPath}|{app.AppIcon}");
				}
			}
		}

		private void btnDeleteAppOnClick(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			if (button != null)
			{
				string appPath = button.Tag as string;
				var appToRemove = appList.FirstOrDefault(a => a.AppPath == appPath);
				if (appToRemove != null)
				{
					appList.Remove(appToRemove);
					listApps.ItemsSource = null; // Refresh the list
					listApps.ItemsSource = appList;
					SaveAppsList();
					MessageBox("App removed successfully");
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
