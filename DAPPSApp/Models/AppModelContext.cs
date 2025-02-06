using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DAPPSApp.Models
{
	[JsonSourceGenerationOptions(WriteIndented = true)]
	[JsonSerializable(typeof(AppModel))]
	[JsonSerializable(typeof(List<AppModel>))]
	public partial class AppModelContext : JsonSerializerContext
	{

	}
}
