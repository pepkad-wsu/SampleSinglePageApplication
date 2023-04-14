using Newtonsoft.Json;
namespace SampleSinglePageApplication;
public partial class DataObjects
{


	/// 
	/// Setting
	/// 
	/// WARNING: AUTO GENERATED FILE - DO NOT MODIFY BY HAND
	/// GENERATED BY: SampleSinglePageApplication.Transcriber console application.
	///   To regenerate the file, first update the path varibale in the program.cs then run the console app.
	///
	public class FilterSettingsAuto : Filter
	{
		public Int32? SettingId { get; set; }              
		public String? SettingName { get; set; }              
		public bool SettingNameFilterExact { get; set; }              
		public bool SettingNameIncludeInKeyword { get; set; }              
		public String? SettingType { get; set; }              
		public bool SettingTypeFilterExact { get; set; }              
		public bool SettingTypeIncludeInKeyword { get; set; }              
		public String? SettingNotes { get; set; }              
		public bool SettingNotesFilterExact { get; set; }              
		public bool SettingNotesIncludeInKeyword { get; set; }              
		public String? SettingText { get; set; }              
		public bool SettingTextFilterExact { get; set; }              
		public bool SettingTextIncludeInKeyword { get; set; }              
		public Guid? UserId { get; set; }              
	}

	public class SavedFilterSettingsAuto : ActionResponseObject
	{
		public string? Description { get; set; }
		public FilterSettingsAuto? Filter { get; set; }
		public Guid SavedFilterId { get; set; }
		public Guid TenantId { get; set; }
		public Guid UserId { get; set; }
	}
}