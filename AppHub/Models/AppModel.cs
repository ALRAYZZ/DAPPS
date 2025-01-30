using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHub.Models
{
	public class AppModel
	{
		private string appName;
		public string AppName
		{
			get => appName;
			set => appName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower()); // Capitalize the first letter of the app name
		}
		public string AppPath { get; set; }
		public string AppIcon { get; set; } = "Assets/default_icon.png";
	}
}
