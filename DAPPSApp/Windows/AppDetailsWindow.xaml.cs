using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DAPPSApp.Models;
using DAPPSApp.Utilities;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using WinRT.Interop;
using Microsoft.UI;
using Microsoft.UI.Windowing;

namespace DAPPSApp.Views
{
	public sealed partial class AppDetailsWindow : Window
	{
		private AppModel app;
		private List<AppModel> appList;

		public event EventHandler<EventArgs> AppDeleted = (_, _) => { };

		public AppDetailsWindow(AppModel app, List<AppModel> appList)
		{
			this.InitializeComponent();
			this.app = app;
			this.appList = appList;

			if (this.Content is FrameworkElement rootElement)
			{
				rootElement.DataContext = app;
			}
			this.Activated += AppDetailsWindow_Activated;
			this.Closed += AppDetailsWindow_Closed;
			this.Activate();
		}
		private void AppDetailsWindow_Activated(object sender, WindowActivatedEventArgs e)
		{

		}
		private void AppDetailsWindow_Closed(object sender, WindowEventArgs e)
		{
			btnLibraryOnClick(null, null);
		}
		private async void btnLaunchAppOnClick(object sender, RoutedEventArgs e)
		{
			if (File.Exists(app.AppPath))
			{
				try
				{
					Process.Start(new ProcessStartInfo(app.AppPath) { UseShellExecute = true });
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

		private async void btnAddChangeIconOnClick(object sender, RoutedEventArgs e)
		{
			string? newIconPath = await FileHelper.AddAppIconAsync(this);
			if (!string.IsNullOrEmpty(newIconPath))
			{
				app.AppIcon = newIconPath;
				FileHelper.SaveAppsList(appList);
				await DialogHelper.ShowMessageBoxAsync(this, "App icon updated successfully");
			}
			else
			{
				await DialogHelper.ShowMessageBoxAsync(this, "No icon selected");
			}
		}

		private async void btnRenameAppOnClick(object sender, RoutedEventArgs e)
		{
			var inputTextBox = new TextBox { Text = app.AppName };

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
				app.AppName = inputTextBox.Text;
				FileHelper.SaveAppsList(appList);
				await DialogHelper.ShowMessageBoxAsync(this, "App renamed successfully");
			}
		}

		private async void btnDeleteAppOnClick(object sender, RoutedEventArgs e)
		{
			FileHelper.DeleteAppIcon(app.AppIcon);
			appList.Remove(app);
			FileHelper.SaveAppsList(appList);
			await DialogHelper.ShowMessageBoxAsync(this, "App removed successfully");

			AppDeleted?.Invoke(this, EventArgs.Empty);

			this.Close();
		}

		private void btnLibraryOnClick(object? sender, RoutedEventArgs? e)
		{
			var mainWindow = MainWindow.Instance;
			if (mainWindow != null)
			{
				var hwnd = WindowNative.GetWindowHandle(mainWindow);
				var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
				var appWindow = AppWindow.GetFromWindowId(windowId);
				appWindow.Show();
			}

			this.Close();
		}
	}
}

