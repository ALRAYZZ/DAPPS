using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppHub.Models
{
	public class AppModel
	{
		public string AppName { get; set; }
		public string AppPath { get; set; }
		public string AppIcon { get; set; } = "Assets/default_icon.png";
	}
}
