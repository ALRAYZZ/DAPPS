using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AppHub.Models;
using AppHub.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

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

			FileHelper.InitializeWithWindow(filePicker, this);
			filePicker.ViewMode = PickerViewMode.List;
			filePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			filePicker.FileTypeFilter.Add(".exe");

			StorageFile file = await filePicker.PickSingleFileAsync();

			if (file != null)
			{
				string appPath = file.Path;
				string defaultAppName = Path.GetFileNameWithoutExtension(appPath);
				string appName = await DialogHelper.GetAppNameAsync(this, defaultAppName);

				if (!appList.Any(a => a.AppPath == appPath))
				{
					string appIcon = await DialogHelper.GetAppIconAsync(this);


					var app = new AppModel { AppName = appName, AppPath = appPath, AppIcon = appIcon };
					appList.Add(app);
					listApps.ItemsSource = null; // Refresh the list
					listApps.ItemsSource = appList;
					await DialogHelper.ShowMessageBoxAsync(this, "App added successfully");
					FileHelper.SaveAppsList(appList);
				}
				else
				{
					await DialogHelper.ShowMessageBoxAsync(this, "The selected app is already in the list");
				}
			}
			else
			{
				await DialogHelper.ShowMessageBoxAsync(this, "No app selected");
			}
		}
		private async void btnLaunchAppOnClick(object sender, RoutedEventArgs e)
		{
			if (listApps.SelectedItem != null)
			{
				var selectedApp = (AppModel)listApps.SelectedItem;

				if (File.Exists(selectedApp.AppPath))
				{
					try
					{
						Process.Start(new ProcessStartInfo(selectedApp.AppPath) { UseShellExecute = true });
						await DialogHelper.ShowMessageBoxAsync(this, "App launched successfully");
					}
					catch (Exception ex)
					{
						await DialogHelper.ShowMessageBoxAsync(this, $"An error occurred while launching the app {ex.Message}");
					}
				}
				else
				{
					await DialogHelper.ShowMessageBoxAsync(this, "The selected app does not exist");
				}
			}
			else
			{
				await DialogHelper.ShowMessageBoxAsync(this, "Please select an app to launch");
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
					string newIconPath = await FileHelper.AddAppIconAsync(this);
					appToUpdate.AppIcon = newIconPath;
					listApps.ItemsSource = null; // Refresh the list
					listApps.ItemsSource = appList;
					FileHelper.SaveAppsList(appList);
					await DialogHelper.ShowMessageBoxAsync(this, "App icon updated successfully");
				}
			}
		}
		private async void btnDeleteAppOnClick(object sender, RoutedEventArgs e)
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
					FileHelper.SaveAppsList(appList);
					await DialogHelper.ShowMessageBoxAsync(this, "App removed successfully");
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
						FileHelper.SaveAppsList(appList);
						await DialogHelper.ShowMessageBoxAsync(this, "App renamed successfully");
					}
				}
			}
		}


		private void LoadAppsList()
		{
			appList = FileHelper.LoadAppsList();

			foreach (var app in appList)
			{
				if (string.IsNullOrEmpty(app.AppIcon))
				{
					app.AppIcon = "Assets/default_icon.png";
				}
			}
			listApps.ItemsSource = null;
			listApps.ItemsSource = appList;
		}


	}
}
