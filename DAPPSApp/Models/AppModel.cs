using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAPPSApp.Models
{
	public class AppModel
	{
		private bool isSelected;
		public string AppName { get; set; }
		public string AppPath { get; set; }
		public string AppIcon { get; set; } = "Assets/default_icon.png";

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
