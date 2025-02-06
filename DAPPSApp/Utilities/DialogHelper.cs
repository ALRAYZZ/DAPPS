using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAPPSApp.Utilities
{
	public static class DialogHelper
	{
		public static async Task<string> GetAppNameAsync(Window window, string defaultAppName)
		{
			var inputTextBox = new TextBox { Text = defaultAppName };

			var dialog = new ContentDialog
			{
				Title = "Enter the app name",
				Content = inputTextBox,
				PrimaryButtonText = "Ok",
				CloseButtonText = "Cancel",
				XamlRoot = window.Content.XamlRoot
			};

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				return inputTextBox.Text;
			}
			return defaultAppName;
		}

		public static async Task<string?> GetAppIconAsync(Window window)
		{
			var dialog = new ContentDialog()
			{
				Title = "Add App Icon",
				Content = "Would you like to add an icon for this app?",
				PrimaryButtonText = "Ok",
				CloseButtonText = "Cancel",
				XamlRoot = window.Content.XamlRoot
			};

			var result = await dialog.ShowAsync();

			if (result == ContentDialogResult.Primary)
			{
				return await FileHelper.AddAppIconAsync(window);
			}

			return "Assets/default_icon.png";
		}

		public static async Task ShowMessageBoxAsync(Window window, string message)
		{
			var dialog = new ContentDialog()
			{
				Title = "Notification",
				Content = message,
				CloseButtonText = "Ok",
				XamlRoot = window.Content.XamlRoot
			};
			await dialog.ShowAsync();
		}
	}
}
