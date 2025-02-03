using AppHub.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AppHub.Utilities
{
	public static class FileHelper
	{
		public static async Task<string> AddAppIconAsync(Window window)
		{
			var iconPicker = new FileOpenPicker();
			InitializeWithWindow(iconPicker, window);
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
					throw new Exception($"Error c opying the icon file: {ex.Message}");
				}

				return destinationPath;
			}
			return "Assets/default_icon.png";
		}

		public static void SaveAppsList(List<AppModel> appList)
		{
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DAPPS");
			string filePath = Path.Combine(folderPath, "apps.json");

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			string json = JsonSerializer.Serialize(appList, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(filePath, json);
		}

		public static List<AppModel> LoadAppsList()
		{
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DAPPS");
			string filePath = Path.Combine(folderPath, "apps.json");

			if (Directory.Exists(folderPath) && File.Exists(filePath))
			{
				string json = File.ReadAllText(filePath);
				return JsonSerializer.Deserialize<List<AppModel>>(json) ?? new List<AppModel>();

			}
			else
			{
				Directory.CreateDirectory(folderPath);
				return new List<AppModel>();
			}
		}

		public static void InitializeWithWindow(FileOpenPicker filePicker, Window window)
		{
			IntPtr hwnd = WindowNative.GetWindowHandle(window);
			WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
		}
	}
}
