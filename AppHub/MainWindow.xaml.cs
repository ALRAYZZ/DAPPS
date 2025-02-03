using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.Services.Maps;
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
					var dialog = new ContentDialog
					{
						Title = "Add App Icon",
						Content = "Would you like to add an icon for this app?",
						PrimaryButtonText = "Ok",
						CloseButtonText = "Cancel",
						XamlRoot = this.Content.XamlRoot
					};

					var result = await dialog.ShowAsync();

					string appIcon = "Assets/default_icon.png";

					if (result == ContentDialogResult.Primary)
					{
						appIcon = await AddAppIconAsync();
					}

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
		private async void btnAddChangeIconOnClick(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			if (button != null)
			{
				string appPath = button.Tag as string; // This works because on the XAML we set the Tag property of the button to the app path Tag="{Binding AppPath}
				var appToUpdate = appList.FirstOrDefault(a => a.AppPath == appPath);
				if (appToUpdate != null)
				{
					string newIconPath = await AddAppIconAsync();
					appToUpdate.AppIcon = newIconPath;
					listApps.ItemsSource = null; // Refresh the list
					listApps.ItemsSource = appList;
					SaveAppsList();
					MessageBox("App icon updated successfully");
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
		private async void btnRenameAppOnClick(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			if (button != null)
			{
				string appPath = button.Tag as string;
				var appToRename = appList.FirstOrDefault(a => a.AppPath == appPath);
				if (appToRename != null)
				{
					var inputTextBox = new TextBox { Text = appToRename.AppName };

					var dialog = new ContentDialog
					{
						Title = "Rename App",
						Content = inputTextBox,
						PrimaryButtonText = "Ok",
						CloseButtonText = "Cancel",
						XamlRoot = this.Content.XamlRoot
					};

					var result = await dialog.ShowAsync();

					if (result == ContentDialogResult.Primary)
					{
						appToRename.AppName = inputTextBox.Text;
						listApps.ItemsSource = null; // Refresh the list
						listApps.ItemsSource = appList;
						SaveAppsList();
						MessageBox("App renamed successfully");
					}
				}
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
			string filePath = Path.Combine(folderPath, "apps.json");

			if (Directory.Exists(folderPath) && File.Exists(filePath))
			{
				string json = File.ReadAllText(filePath);
				appList = System.Text.Json.JsonSerializer.Deserialize<List<AppModel>>(json) ?? new List<AppModel>();


				foreach (var app in appList)
				{
					if (string.IsNullOrEmpty(app.AppIcon))
					{
						app.AppIcon = "Assets/default_icon.png";
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
			string filePath = Path.Combine(folderPath, "apps.json");

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			string json = System.Text.Json.JsonSerializer.Serialize(appList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(filePath, json);
		}
		private async Task<string> AddAppIconAsync()
		{
			var iconPicker = new FileOpenPicker();
			InitializeWithWindow(iconPicker);
			iconPicker.ViewMode = PickerViewMode.Thumbnail;
			iconPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			iconPicker.FileTypeFilter.Add(".png");
			iconPicker.FileTypeFilter.Add(".jpg");
			iconPicker.FileTypeFilter.Add(".jpeg");

			StorageFile iconFile = await iconPicker.PickSingleFileAsync();

			if (iconFile != null)
			{
				string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DAPPS", "AppIcons");
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				string destinationPath = Path.Combine(folderPath, iconFile.Name);


				try
				{
					using (var sourceStream = await iconFile.OpenStreamForReadAsync())
					using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
					{
						await sourceStream.CopyToAsync(destinationStream);
					}
				}
				catch (Exception ex)
				{
					MessageBox($"Error copying the icon file {ex.Message}");
					return "Assets/default_icon.png";
				}

				return destinationPath;
			}
			return "Assets/default_icon.png";
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
