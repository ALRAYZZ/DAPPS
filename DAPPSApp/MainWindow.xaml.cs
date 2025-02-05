using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DAPPSApp.Models;
using DAPPSApp.Utilities;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media;
using DAPPSApp.Views;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.VoiceCommands;
using Windows.ApplicationModel.Background;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;
using Microsoft.UI.Composition;
using System.Threading.Tasks.Sources;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DAPPSApp
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private DispatcherTimer timer;
		internal static MainWindow Instance { get; private set; }
		internal List<AppModel> appList = new List<AppModel>();
		private Dictionary<UIElement, Vector3> originalOffsets = new Dictionary<UIElement, Vector3>();
		public MainWindow()
		{
			this.InitializeComponent();
			Instance = this;
			SetWindowSize(1200, 800);
			LoadAppsList();

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Timer_Tick;
			timer.Start();
		}
		private void Timer_Tick(object sender, object e)
		{
			CheckRunningApps();
		}
		private void SetWindowSize(int width, int height)
		{
			var hwnd = WindowNative.GetWindowHandle(this);
			var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
			var appWindow = AppWindow.GetFromWindowId(windowId);

			appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
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
					FileHelper.DeleteAppIcon(appToRemove.AppIcon);
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
		private void btnCloseAppOnClick(object sender, RoutedEventArgs e)
		{
			if (listApps.SelectedItem != null)
			{
				var selectedApp = (AppModel)listApps.SelectedItem;
				CloseApp(selectedApp);
				btnCloseApp.IsEnabled = false;
				btnCloseApp.Style = (Style)Application.Current.Resources["DisabledButtonStyle"];
			}
		}
		private void ListApps_ItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var grid = sender as Grid;
			var selectedApp = grid?.DataContext as AppModel;
			if (selectedApp != null)
			{
				var appDetailsWindow = new AppDetailsWindow(selectedApp, appList);
				appDetailsWindow.AppDeleted += AppDetailsWindow_AppDeleted;
				appDetailsWindow.Activate();

				// Minimize the MainWindow
				var hwnd = WindowNative.GetWindowHandle(this);
				var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
				var appWindow = AppWindow.GetFromWindowId(windowId);
				appWindow.Hide();
			}
		}
		private void AppDetailsWindow_AppDeleted(object sender, EventArgs e)
		{
			listApps.ItemsSource = null; // Refresh the list
			listApps.ItemsSource = appList;
		}
		private void ListApps_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listApps.SelectedItem != null)
			{
				var selectedApp = (AppModel)listApps.SelectedItem;

				btnLaunchApp.IsEnabled = true;
				btnCloseApp.IsEnabled = selectedApp.IsRunning;

				var listViewItem = (ListViewItem)listApps.ContainerFromItem(selectedApp);
				if (listViewItem != null)
				{
					if (selectedApp.IsRunning)
					{
						VisualStateManager.GoToState(listViewItem, "SelectedRunning", true);
					}
					else
					{
						VisualStateManager.GoToState(listViewItem, "Selected", true);
					}
				}
			}
			else
			{
				btnLaunchApp.IsEnabled = false;
				btnCloseApp.IsEnabled = false;
			}
		}
		private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			var grid = sender as Grid;
			if (grid != null)
			{
				var visual = ElementCompositionPreview.GetElementVisual(grid);
				if (!originalOffsets.ContainsKey(grid))
				{
					originalOffsets[grid] = visual.Offset;
				}
				var originalOffset = originalOffsets[grid];

				visual.StartAnimation("Offset", CreateOffsetAnimation(visual.Compositor, originalOffset + new Vector3(3, 5, 0)));
			}
		}
		private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			var grid = sender as Grid;
			if (grid != null)
			{
				var visual = ElementCompositionPreview.GetElementVisual(grid);
				if (originalOffsets.ContainsKey(grid))
				{
					var originalOffset = originalOffsets[grid];
					visual.StartAnimation("Offset", CreateOffsetAnimation(visual.Compositor, originalOffset));
				}
			}
		}
		private CompositionAnimation CreateOffsetAnimation(Compositor compositor, Vector3 targetOffset)
		{
			var animation = compositor.CreateVector3KeyFrameAnimation();
			animation.InsertKeyFrame(1.0f, targetOffset);
			animation.Duration = TimeSpan.FromMilliseconds(500);
			return animation;
		}
		private void CheckRunningApps()
		{
			foreach (var app in appList)
			{
				app.IsRunning = IsAppRunning(app.AppPath);
				var listViewItem = (ListViewItem)listApps.ContainerFromItem(app);
				if (listViewItem != null)
				{
					if (app.IsRunning)
					{
						if (listApps.SelectedItem == app)
						{
							VisualStateManager.GoToState(listViewItem, "SelectedRunning", true);
						}
						else
						{
							VisualStateManager.GoToState(listViewItem, "Running", true);
						}
						
					}
					else
					{
						if (listApps.SelectedItem == app)
						{
							VisualStateManager.GoToState(listViewItem, "Selected", true);
						}
						else
						{
							VisualStateManager.GoToState(listViewItem, "Normal", true);
						}

					}
				}
			}
		}
		private bool IsAppRunning(string appPath)
		{
			string appName = Path.GetFileNameWithoutExtension(appPath);
			return Process.GetProcessesByName(appName).Any();
		}
		private void CloseApp(AppModel app)
		{
			string appName = Path.GetFileNameWithoutExtension(app.AppPath);
			var processes = Process.GetProcessesByName(appName);
			foreach (var process in processes)
			{
				process.Kill();
			}
			app.IsRunning = false;
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

