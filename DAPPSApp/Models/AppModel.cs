using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DAPPSApp.Models
{
	public partial class AppModel : INotifyPropertyChanged
	{
		private string appName = string.Empty;
		private string appPath = string.Empty;
		private string appIcon = "Assets/default_icon.png";
		private bool isSelected;
		private bool isRunning;

		public string AppName
		{
			get => appName;
			set
			{
				if (appName != value)
				{
					appName = value;
					OnPropertyChanged(nameof(AppName));
				}
			}
		}

		public string AppPath
		{
			get => appPath;
			set
			{
				if (appPath != value)
				{
					appPath = value;
					OnPropertyChanged(nameof(AppPath));
				}
			}
		}

		public string AppIcon
		{
			get => appIcon;
			set
			{
				if (appIcon != value)
				{
					appIcon = value;
					OnPropertyChanged(nameof(AppIcon));
				}
			}
		}

		public bool IsSelected
		{
			get => isSelected;
			set
			{
				if (isSelected != value)
				{
					isSelected = value;
					OnPropertyChanged(nameof(IsSelected));
				}
			}
		}
		public bool IsRunning
		{
			get => isRunning;
			set
			{
				if (isRunning != value)
				{
					isRunning = value;
					OnPropertyChanged(nameof(IsRunning));
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
