/// 
/// Setting
/// 
/// WARNING: AUTO GENERATED FILE - DO NOT MODIFY BY HAND
/// GENERATED BY: SampleSinglePageApplication.Transcriber console application.
///   To regenerate the file, first update the path varibale in the program.cs then run the console app.
///
class SettingsModelAuto {
	MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
	Filter: KnockoutObservable<filterSettingsAuto> = ko.observable(new filterSettingsAuto);
	Setting: KnockoutObservable<setting> = ko.observable(new setting);
	Settings: KnockoutObservableArray<setting> = ko.observableArray([]);
	Loading: KnockoutObservable<boolean> = ko.observable(false);
	ConfirmDeleteSetting: KnockoutObservable<string> = ko.observable("");
	ShowSettingDetails: KnockoutObservable<boolean> = ko.observable(false);
	FilterViewSettingType: KnockoutObservable<string> = ko.observable("list");
	GettingSavedSettingFilterName: KnockoutObservable<boolean> = ko.observable(false);
	SavedSettingFilterName: KnockoutObservable<string> = ko.observable("");

	constructor() {
		this.MainModel().View.subscribe(() => {
			this.ViewChanged();
		});

		this.MainModel().SignalRUpdate.subscribe(() => {
			this.SignalrUpdate();
		});
		//setTimeout("setupUserPhotoDropZone()", 0);
		setTimeout(() => this.StartFilterMonitoring(), 1000);
	}
	//TODO: click: DeleteSavedSettingFilter
	DeleteSavedSettingFilter(): void {
		this.MainModel().Message_Hide();
		let filterId: string = this.Filter().filterId();
		
		let success: Function = (data: server.booleanResponse) => {
			this.MainModel().Message_Hide();
			
			if (data != null) {
				if (data.result) {
					let existing: savedFilterSettingsAuto = ko.utils.arrayFirst(this.MainModel().User().savedFiltersSettingsAuto(), function (item) {
						return item.savedFilterId() == filterId;
					});
					if (existing != null) {
						this.MainModel().User().savedFiltersSettingsAuto.remove(existing);
					}
					
					this.ClearSettingFilter();
				} else {
					this.MainModel().Message_Errors(data.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to delete the saved filter.");
			}
		};
		
		this.MainModel().Message_Deleting();
		tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteSavedSettingFilter/" + filterId.toString(), null, success);
	}
	//TODO: click: ExportSetting
	ExportSettings(): void {
		let filter: filterSettingsAuto = new filterSettingsAuto;
		filter.Load(JSON.parse(ko.toJSON(this.Filter)));
		filter.records([]);
		$("#setting-filter").val(ko.toJSON(filter));
		$("#setting-filter-download").submit();
	}

	ToggleSettingDetails(): void {
		if (this.ShowSettingDetails()) {
			this.ShowSettingDetails(false);
			localStorage.setItem("setting-details-" + this.MainModel().TenantId(), "0");
		} else {
			this.ShowSettingDetails(true);
			localStorage.setItem("setting-details-" + this.MainModel().TenantId(), "1");
		}
	
		this.RenderSettingTable();
	}
	
	/**
	 * Called when the Show or Hide Filter buttons are clicked.
	 */
	ToggleShowSettingFilter(): void {
		this.Filter().showFilters(!this.Filter().showFilters());
		this.SaveSettingFilter();
	}
	ToggleSettingView(): void {
		if (this.FilterViewSettingType() == "list") {
			this.FilterViewSettingType("card");
		} else {
			this.FilterViewSettingType("list");
		}
		localStorage.setItem("setting-view-" + this.MainModel().TenantId(), this.FilterViewSettingType());
		this.RenderSettingTable();
	}
	SaveSettingFilterRecord(): void {
		if (!tsUtilities.HasValue(this.SavedSettingFilterName())) {
			tsUtilities.DelayedFocus("saved-setting-filter-name");
			return;
		}
		
		let success: Function = (data: server.savedFilterSettingsAuto) => {
			this.MainModel().Message_Hide();
			
			if (data != null) {
				if (data.actionResponse.result) {
					let existing: savedFilterSettingsAuto = null;
					
					if (this.MainModel().User().savedFiltersSettingsAuto() != null && this.MainModel().User().savedFiltersSettingsAuto().length > 0) {
						existing = ko.utils.arrayFirst(this.MainModel().User().savedFiltersSettingsAuto(), function (item) {
							return item.savedFilterId() == data.savedFilterId;
						});
					}
					
					if (existing != null) {
						existing.Load(data);
					} else {
						let newSavedFilter: savedFilterSettingsAuto = new savedFilterSettingsAuto();
						newSavedFilter.Load(data);
						this.MainModel().User().savedFiltersSettingsAuto.push(newSavedFilter);
					}
					
					this.MainModel().User().savedFiltersSettingsAuto.sort(function (l, r) {
						return l.description() > r.description() ? 1 : -1;
					});
					
					this.GettingSavedSettingFilterName(false);
				} else {
					this.MainModel().Message_Errors(data.actionResponse.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to save the filter.");
			}
		};
		this.Filter().tenantId(this.MainModel().TenantId());
		
		let f: filterSettingsAuto = new filterSettingsAuto();
		f.Load(JSON.parse(ko.toJSON(this.Filter)));
		f.columns(null);
		f.records(null);
		
		let postFilter: savedFilterSettingsAuto = new savedFilterSettingsAuto();
		postFilter.description(this.SavedSettingFilterName());
		postFilter.savedFilterId(this.Filter().filterId());
		postFilter.userId(this.MainModel().User().userId());
		postFilter.tenantId(this.MainModel().TenantId());
		postFilter.filter(f);
		
		this.MainModel().Message_Saving();
		tsUtilities.AjaxData(window.baseURL + "api/Data/SaveSavedSettingFilter", ko.toJSON(postFilter), success);
	}
	/**
	 * Method fires when the URL action is "NewSetting"
	 */
	AddSetting(): void {
		this.Setting(new setting);
		this.Setting().settingId(null);
		this.Setting().tenantId(this.MainModel().TenantId());
	
		//this.MainModel().UDFFieldsRender("edit-setting-udf-fields", "Settings", JSON.parse(ko.toJSON(this.Setting)));
	
		tsUtilities.DelayedFocus("edit-setting-settingid");
	}
	
	/**
	 * Clears the values for the setting search filter.
	 */
	ClearSettingFilter(): void {
	
		this.Loading(true);
	
		this.Filter().keyword(null);
		this.Filter().settingId(null);
		this.Filter().settingName(null);
		this.Filter().settingNameFilterExact(false);
		this.Filter().settingType(null);
		this.Filter().settingTypeFilterExact(false);
		this.Filter().settingNotes(null);
		this.Filter().settingNotesFilterExact(false);
		this.Filter().settingText(null);
		this.Filter().settingTextFilterExact(false);
		this.Filter().userId(null);
		this.Filter().udf01(null);
		this.Filter().udf02(null);
		this.Filter().udf03(null);
		this.Filter().udf04(null);
		this.Filter().udf05(null);
		this.Filter().udf06(null);
		this.Filter().udf07(null);
		this.Filter().udf08(null);
		this.Filter().udf09(null);
		this.Filter().udf10(null);
		this.Filter().page(1);
	
		//TODO: Callback method for more filtering outside of the autos
	
		this.Loading(false);
		this.GetSettings();
	}
	
	/**
	 * Deletes a setting.
	 */
	DeleteSetting(): void {
		let success: Function = (data: server.booleanResponse) => {
			this.MainModel().Message_Hide();
			if (data != null) {
				if (data.result) {
					this.MainModel().Nav("SettingsAuto");
				} else {
					this.MainModel().Message_Errors(data.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to delete the Setting.");
			}
		};
	
		this.MainModel().Message_Deleting();
		tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteSettingAuto/" + this.MainModel().Id(), null, success);
	}
	
	/**
	 * Method fires when the URL action is "EditSetting"
	 */
	EditSetting(): void {
		this.MainModel().Message_Hide();
		let settingId: string = this.MainModel().Id();
		this.Setting(new setting);
		this.Setting().settingId(null);
	
		if (tsUtilities.HasValue(settingId)) {
			let success: Function = (data: server.setting) => {
				if (data != null) {
					if (data.actionResponse.result) {
						this.Setting().Load(data);
						tsUtilities.DelayedFocus("edit-setting-category");
	
						//this.MainModel().UDFFieldsRender("edit-setting-udf-fields", "Settings", JSON.parse(ko.toJSON(this.Setting)));
	
						this.Loading(false);
					} else {
						this.MainModel().Message_Errors(data.actionResponse.messages);
					}
				} else {
					this.MainModel().Message_Error("An unknown error occurred attempting to load the Setting record.");
				}
			};
	
			this.Loading(true);
			tsUtilities.AjaxData(window.baseURL + "api/data/GetSettingAuto/" + settingId, null, success);
		} else {
			this.MainModel().Message_Error("No valid SettingId received.");
		}
	
	}
	
	/**
	 * The callback method used by the paged recordset control to handle the action on the record.
	 * @param record {server.setting} - The object being passed is a JSON object, not an observable.
	 */
	EditSettingCallback(record: server.setting): void {
		if (record != undefined && record != null && record.settingId != null) {
			this.MainModel().Nav("EditSettingAuto", "" + record.settingId);
		}
	}
	
	/**
	 * Called when the setting filter changes to reload setting records, unless the filter is changing because
	 * records are being reloaded.
	 */
	FilterChanged(): void {
		if (!this.Loading()) {
			this.GetSettings();
		}
	}
	
	/**
	 * Loads the saved filter that is stored in a cookie as a JSON object.
	 */
	GetSavedFilter(): void {
		this.Filter(new filterSettingsAuto);
		this.Filter().tenantId(this.MainModel().TenantId());
	
		// maybe this should be specific per tenant?
		let savedFilter: string = localStorage.getItem("saved-filter-settings-" + this.MainModel().TenantId().toLowerCase());
		if (tsUtilities.HasValue(savedFilter)) {
			this.Filter().Load(JSON.parse(savedFilter));
			this.StartFilterMonitoring();
		}
	
		this.GetSettings();
	}
	
	/**
	 * Called when the filter changes or when the page loads to get the settings matching the current filter.
	 */
	GetSettings(): void {
		// Load the filter
		this.Loading(true);
		if (this.Filter().recordsPerPage() == null || this.Filter().recordsPerPage() == 0) {
			this.Filter().recordsPerPage(10);
		}
	
		let success: Function = (data: server.filterSettingsAuto) => {
			this.MainModel().Message_Hide();
			if (data != null) {
				if (data.actionResponse.result) {
					this.Filter().Load(data);
	
					this.RenderSettingTable();
	
					this.Loading(false);
				} else {
					this.MainModel().Message_Errors(data.actionResponse.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to load Setting records.");
			}
			this.Loading(false);
		};
	
	
		let postFilter: filterSettingsAuto = new filterSettingsAuto();
		postFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
		postFilter.columns(null);
		postFilter.records(null);
	
		let jsonData: string = ko.toJSON(postFilter);
		localStorage.setItem("saved-filter-settings-" + this.MainModel().TenantId().toLowerCase(), jsonData);
		tsUtilities.AjaxData(window.baseURL + "api/Data/GetSettingsFilteredAuto/", jsonData, success);
	
	}
	
	/**
	 * Method handles the callback from the paged recordset control when the page changes, the records per page changes, or when the sort order changes.
	 * @param type {string} - The type of change event (count, page, or sort)
	 * @param data {any} - The data passed back, which is a number for count and page and a field column id for the sort.
	 */
	RecordsetCallbackHandler(type: string, data: any): void {
		console.log("RecordsetCallbackHandler", type, data);
		switch (type) {
			case "count":
				window.settingsModelAuto.Filter().recordsPerPage(data);
				window.settingsModelAuto.GetSettings();
				break;
	
			case "page":
				window.settingsModelAuto.Filter().page(data);
				window.settingsModelAuto.GetSettings();
				break;
	
			case "sort":
				window.settingsModelAuto.UpdateSort(data);
				break;
		}
	}
	
	/**
	 * Called when the Refresh Filter button is clicked.
	 */
	RefreshSettingFilter(): void {
		this.SaveSettingFilter();
		this.GetSettings();
	}
	
	/**
	 * Renders the paged recordset view. This happens when the filter loads, but also gets called for certain SignalR events
	 * to update settings that might be in the current setting list.
	 */
	RenderSettingTable(): void {
		// Load records in the pagedRecordset
		let f: filter = new filter();
		f.Load(JSON.parse(ko.toJSON(this.Filter)));
	
		let records: any = JSON.parse(ko.toJSON(this.Filter().records));
	
		pagedRecordset.Render({
			elementId: "setting-records-auto",
			data: JSON.parse(ko.toJSON(f)),
			recordsetCallbackHandler: (type: string, data: any) => { this.RecordsetCallbackHandler(type, data); },
			actionHandlers: [
				{
					callbackHandler: (setting: server.setting) => { this.EditSettingCallback(setting); },
					actionElement: "<button type='button' class='btn btn-sm btn-primary nowrap'>" + this.MainModel().IconAndText("Setting.EditTableButton") + "</button>"
				}
			],
			recordNavigation: "both",
			//photoBaseUrl: photoBaseUrl,
			booleanIcon: this.MainModel().Icon("selected")
		});
	}
	
	/**
	 * Saves the current filter as a JSON object in a cookie. The items that aren't needed are nulled out first
	 * so that the column data and record data are not stored in the cookie.
	 */
	SaveSettingFilter(): void {
		let saveFilter: filterSettingsAuto = new filterSettingsAuto();
		saveFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
		saveFilter.actionResponse(null);
		saveFilter.columns([]);
		saveFilter.records(null);
	
		localStorage.setItem("saved-filter-settings-" + this.MainModel().TenantId().toLowerCase(), ko.toJSON(saveFilter));
	}
	
	/**
	 * Saves a setting record for the setting currently being added or edited.
	 */
	SaveSetting(): void {
		this.MainModel().Message_Hide();
		let errors: string[] = [];
		let focus: string = "";
	
	//if (!tsUtilities.HasValue(this.Setting().settingName())) {
	//    errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("Setting.SettingName")));
	//    if (focus == "") { focus = "edit-setting-settingname"; }
	//}
	
		if (errors.length > 0) {
			this.MainModel().Message_Errors(errors);
			tsUtilities.DelayedFocus(focus);
			return;
		}
	
		this.MainModel().UDFFieldsGetValues("Settings", this.Setting());
		let json: string = ko.toJSON(this.Setting);
	
		let success: Function = (data: server.setting) => {
			this.MainModel().Message_Hide();
			if (data != null) {
				if (data.actionResponse.result) {
					this.MainModel().Nav("SettingsAuto");
				} else {
					this.MainModel().Message_Errors(data.actionResponse.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to save this Setting.");
			}
		};
	
		this.MainModel().Message_Saving();
		tsUtilities.AjaxData(window.baseURL + "api/Data/SaveSettingAuto", json, success);
	}
	
	/**
	 * This model subscribes to SignalR updates from the MainModel so that settings in the setting filter list
	 * can be removed or updated when their data changes or they are deleted.
	 */
	SignalrUpdate(): void {
		//console.log("In Tenants, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
		switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
			case "setting":
				let settingId: string = this.MainModel().SignalRUpdate().itemId();
	
				switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
					case "deletedsetting":
						let records: any[] = [];
						if (this.Filter().records() != null && this.Filter().records().length > 0) {
							this.Filter().records().forEach(function (e) {
								if (e["settingId"] != settingId) {
									records.push(e);
								}
							});
						}
						this.Filter().records(records);
						this.RenderSettingTable();
	
						break;
	
					case "savedsetting":
						let settingData: any = this.MainModel().SignalRUpdate().object();
	
						let index: number = -1;
						let indexItem: number = -1;
						if (this.Filter().records() != null && this.Filter().records().length > 0) {
							this.Filter().records().forEach(function (e) {
								index++;
								if (e["settingId"] == settingId) {
									indexItem = index;
								}
							});
						}
	
						if (indexItem > -1) {
							this.Filter().records()[indexItem] = JSON.parse(settingData);
							this.RenderSettingTable();
						}
				}
	
				break;
		}
	}
	
	/**
	 * Starts observing changes to the filter elements to call FilterChanged when selections are changed.
	 */
	StartFilterMonitoring(): void {
		// Subscribe to filter changed
		this.Filter().keyword.subscribe(() => { this.FilterChanged(); });                                                                                             
		this.Filter().settingName.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingNameFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingType.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingTypeFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingNotes.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingNotesFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingText.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingTextFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().settingId.subscribe(() => { this.FilterChanged(); });
		this.Filter().tenantId.subscribe(() => { this.FilterChanged(); });
		this.Filter().userId.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf01.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf02.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf03.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf04.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf05.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf06.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf07.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf08.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf09.subscribe(() => { this.FilterChanged(); });
		this.Filter().udf10.subscribe(() => { this.FilterChanged(); });
	}
	
	
	
	/**
	 * Handles changing the sort order and updating the filter.
	 * @param dataElementName
	 */
	UpdateSort(dataElementName: string): void {
		let currentSort: string = this.Filter().sort();
		if (tsUtilities.HasValue(currentSort)) {
			currentSort = currentSort.toLowerCase();
		} else {
			currentSort = "";
		}
	
		let currentDirection: string = this.Filter().sortOrder();
		if (tsUtilities.HasValue(currentDirection)) {
			currentDirection = currentDirection.toUpperCase();
		}
	
		if (tsUtilities.HasValue(dataElementName)) {
			if (currentSort.toLowerCase() == dataElementName.toLowerCase()) {
				if (currentDirection == "ASC") {
					this.Filter().sortOrder("DESC");
				} else {
					this.Filter().sortOrder("ASC");
				}
			} else {
				this.Filter().sort(dataElementName);
				switch (dataElementName.toLowerCase()) {
					case "modified":
						this.Filter().sortOrder("DESC");
						break;
	
					default:
						this.Filter().sortOrder("ASC");
						break;
				}
			}
			this.GetSettings();
		}
	}
	
	/**
	 * Called when the view changes in the MainModel to do any necessary work in this viewModel.
	 */
	ViewChanged(): void {
		this.Loading(false);
	
		switch (this.MainModel().CurrentView().toLowerCase()) {
			case "editsettingauto":
				this.EditSetting();
				break;
	
			case "newsettingauto":
				this.AddSetting();
				break;
	
			case "settingsauto":
				this.GetSavedFilter();
				this.Filter().tenantId(this.MainModel().TenantId());
				break;
		}
	}
}
