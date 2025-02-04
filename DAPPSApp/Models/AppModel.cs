﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAPPSApp.Models
{
	public class AppModel : INotifyPropertyChanged
	{
		private string appName;
		private string appPath;
		private string appIcon = "Assets/default_icon.png";
		private bool isSelected;

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

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
