using Newtonsoft.Json;
namespace SampleSinglePageApplication;
public partial class DataObjects
{


	/// 
	/// Department
	/// 
	/// WARNING: AUTO GENERATED FILE - DO NOT MODIFY BY HAND
	/// GENERATED BY: SampleSinglePageApplication.Transcriber console application.
	///   To regenerate the file, first update the path varibale in the program.cs then run the console app.
	///
	public class FilterDepartmentsAuto : Filter
	{
		public Guid? DepartmentId { get; set; }              
		public String? DepartmentName { get; set; }              
		public bool DepartmentNameFilterExact { get; set; }              
		public bool DepartmentNameIncludeInKeyword { get; set; }              
		public String? ActiveDirectoryNames { get; set; }              
		public bool ActiveDirectoryNamesFilterExact { get; set; }              
		public bool ActiveDirectoryNamesIncludeInKeyword { get; set; }              
		public Boolean? Enabled { get; set; }              
		public Guid? DepartmentGroupId { get; set; }              
	}

	public class SavedFilterDepartmentsAuto : ActionResponseObject
	{
		public string? Description { get; set; }
		public FilterDepartmentsAuto? Filter { get; set; }
		public Guid SavedFilterId { get; set; }
		public Guid TenantId { get; set; }
		public Guid UserId { get; set; }
	}
}