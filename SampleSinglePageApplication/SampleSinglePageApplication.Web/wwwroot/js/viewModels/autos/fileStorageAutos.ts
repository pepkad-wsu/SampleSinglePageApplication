/// 
/// FileStorage
/// 
/// WARNING: AUTO GENERATED FILE - DO NOT MODIFY BY HAND
/// GENERATED BY: SampleSinglePageApplication.Transcriber console application.
///   To regenerate the file, first update the path varibale in the program.cs then run the console app.
///
class FileStoragesModelAuto {
	MainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);
	Filter: KnockoutObservable<filterFileStoragesAuto> = ko.observable(new filterFileStoragesAuto);
	FileStorage: KnockoutObservable<fileStorage> = ko.observable(new fileStorage);
	FileStorages: KnockoutObservableArray<fileStorage> = ko.observableArray([]);
	Loading: KnockoutObservable<boolean> = ko.observable(false);
	ConfirmDeleteFileStorage: KnockoutObservable<string> = ko.observable("");
	ShowFileStorageDetails: KnockoutObservable<boolean> = ko.observable(false);
	FilterViewFileStorageType: KnockoutObservable<string> = ko.observable("list");
	GettingSavedFileStorageFilterName: KnockoutObservable<boolean> = ko.observable(false);
	SavedFileStorageFilterName: KnockoutObservable<string> = ko.observable("");

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
	//TODO: click: DeleteSavedFileStorageFilter
	DeleteSavedFileStorageFilter(): void {
		this.MainModel().Message_Hide();
		let filterId: string = this.Filter().filterId();
		
		let success: Function = (data: server.booleanResponse) => {
			this.MainModel().Message_Hide();
			
			if (data != null) {
				if (data.result) {
					let existing: savedFilterFileStoragesAuto = ko.utils.arrayFirst(this.MainModel().User().savedFiltersFileStoragesAuto(), function (item) {
						return item.savedFilterId() == filterId;
					});
					if (existing != null) {
						this.MainModel().User().savedFiltersFileStoragesAuto.remove(existing);
					}
					
					this.ClearFileStorageFilter();
				} else {
					this.MainModel().Message_Errors(data.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to delete the saved filter.");
			}
		};
		
		this.MainModel().Message_Deleting();
		tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteSavedFileStorageFilter/" + filterId.toString(), null, success);
	}
	//TODO: click: ExportFileStorage
	ExportFileStorages(): void {
		let filter: filterFileStoragesAuto = new filterFileStoragesAuto;
		filter.Load(JSON.parse(ko.toJSON(this.Filter)));
		filter.records([]);
		$("#filestorage-filter").val(ko.toJSON(filter));
		$("#filestorage-filter-download").submit();
	}

	ToggleFileStorageDetails(): void {
		if (this.ShowFileStorageDetails()) {
			this.ShowFileStorageDetails(false);
			localStorage.setItem("filestorage-details-" + this.MainModel().TenantId(), "0");
		} else {
			this.ShowFileStorageDetails(true);
			localStorage.setItem("filestorage-details-" + this.MainModel().TenantId(), "1");
		}
	
		this.RenderFileStorageTable();
	}
	
	/**
	 * Called when the Show or Hide Filter buttons are clicked.
	 */
	ToggleShowFileStorageFilter(): void {
		this.Filter().showFilters(!this.Filter().showFilters());
		this.SaveFileStorageFilter();
	}
	ToggleFileStorageView(): void {
		if (this.FilterViewFileStorageType() == "list") {
			this.FilterViewFileStorageType("card");
		} else {
			this.FilterViewFileStorageType("list");
		}
		localStorage.setItem("filestorage-view-" + this.MainModel().TenantId(), this.FilterViewFileStorageType());
		this.RenderFileStorageTable();
	}
	SaveFileStorageFilterRecord(): void {
		if (!tsUtilities.HasValue(this.SavedFileStorageFilterName())) {
			tsUtilities.DelayedFocus("saved-filestorage-filter-name");
			return;
		}
		
		let success: Function = (data: server.savedFilterFileStoragesAuto) => {
			this.MainModel().Message_Hide();
			
			if (data != null) {
				if (data.actionResponse.result) {
					let existing: savedFilterFileStoragesAuto = null;
					
					if (this.MainModel().User().savedFiltersFileStoragesAuto() != null && this.MainModel().User().savedFiltersFileStoragesAuto().length > 0) {
						existing = ko.utils.arrayFirst(this.MainModel().User().savedFiltersFileStoragesAuto(), function (item) {
							return item.savedFilterId() == data.savedFilterId;
						});
					}
					
					if (existing != null) {
						existing.Load(data);
					} else {
						let newSavedFilter: savedFilterFileStoragesAuto = new savedFilterFileStoragesAuto();
						newSavedFilter.Load(data);
						this.MainModel().User().savedFiltersFileStoragesAuto.push(newSavedFilter);
					}
					
					this.MainModel().User().savedFiltersFileStoragesAuto.sort(function (l, r) {
						return l.description() > r.description() ? 1 : -1;
					});
					
					this.GettingSavedFileStorageFilterName(false);
				} else {
					this.MainModel().Message_Errors(data.actionResponse.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to save the filter.");
			}
		};
		this.Filter().tenantId(this.MainModel().TenantId());
		
		let f: filterFileStoragesAuto = new filterFileStoragesAuto();
		f.Load(JSON.parse(ko.toJSON(this.Filter)));
		f.columns(null);
		f.records(null);
		
		let postFilter: savedFilterFileStoragesAuto = new savedFilterFileStoragesAuto();
		postFilter.description(this.SavedFileStorageFilterName());
		postFilter.savedFilterId(this.Filter().filterId());
		postFilter.userId(this.MainModel().User().userId());
		postFilter.tenantId(this.MainModel().TenantId());
		postFilter.filter(f);
		
		this.MainModel().Message_Saving();
		tsUtilities.AjaxData(window.baseURL + "api/Data/SaveSavedFileStorageFilter", ko.toJSON(postFilter), success);
	}
	/**
	 * Method fires when the URL action is "NewFileStorage"
	 */
	AddFileStorage(): void {
		this.FileStorage(new fileStorage);
		this.FileStorage().fileId(this.MainModel().GuidEmpty());
		this.FileStorage().tenantId(this.MainModel().TenantId());
	
		//this.MainModel().UDFFieldsRender("edit-filestorage-udf-fields", "FileStorages", JSON.parse(ko.toJSON(this.FileStorage)));
	
		tsUtilities.DelayedFocus("edit-filestorage-fileid");
	}
	
	/**
	 * Clears the values for the fileStorage search filter.
	 */
	ClearFileStorageFilter(): void {
	
		this.Loading(true);
	
		this.Filter().keyword(null);
		this.Filter().fileId(null);
		this.Filter().itemId(null);
		this.Filter().fileName(null);
		this.Filter().fileNameFilterExact(false);
		this.Filter().extension(null);
		this.Filter().extensionFilterExact(false);
		this.Filter().sourceFileId(null);
		this.Filter().sourceFileIdFilterExact(false);
		this.Filter().bytes(null);
		this.Filter().uploadDate(null);
		this.Filter().uploadDateStart(null);
		this.Filter().uploadDateEnd(null);
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
		this.GetFileStorages();
	}
	
	/**
	 * Deletes a fileStorage.
	 */
	DeleteFileStorage(): void {
		let success: Function = (data: server.booleanResponse) => {
			this.MainModel().Message_Hide();
			if (data != null) {
				if (data.result) {
					this.MainModel().Nav("FileStoragesAuto");
				} else {
					this.MainModel().Message_Errors(data.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to delete the File Storage.");
			}
		};
	
		this.MainModel().Message_Deleting();
		tsUtilities.AjaxData(window.baseURL + "api/Data/DeleteFileStorageAuto/" + this.MainModel().Id(), null, success);
	}
	
	/**
	 * Method fires when the URL action is "EditFileStorage"
	 */
	EditFileStorage(): void {
		this.MainModel().Message_Hide();
		let fileId: string = this.MainModel().Id();
		this.FileStorage(new fileStorage);
		this.FileStorage().fileId(null);
	
		if (tsUtilities.HasValue(fileId)) {
			let success: Function = (data: server.fileStorage) => {
				if (data != null) {
					if (data.actionResponse.result) {
						this.FileStorage().Load(data);
						tsUtilities.DelayedFocus("edit-fileStorage-category");
	
						//this.MainModel().UDFFieldsRender("edit-fileStorage-udf-fields", "FileStorages", JSON.parse(ko.toJSON(this.FileStorage)));
	
						this.Loading(false);
					} else {
						this.MainModel().Message_Errors(data.actionResponse.messages);
					}
				} else {
					this.MainModel().Message_Error("An unknown error occurred attempting to load the File Storage record.");
				}
			};
	
			this.Loading(true);
			tsUtilities.AjaxData(window.baseURL + "api/data/GetFileStorageAuto/" + fileId, null, success);
		} else {
			this.MainModel().Message_Error("No valid FileId received.");
		}
	
	}
	
	/**
	 * The callback method used by the paged recordset control to handle the action on the record.
	 * @param record {server.fileStorage} - The object being passed is a JSON object, not an observable.
	 */
	EditFileStorageCallback(record: server.fileStorage): void {
		if (record != undefined && record != null && tsUtilities.HasValue(record.fileId)) {
			this.MainModel().Nav("EditFileStorageAuto", record.fileId);
		}
	}
	
	/**
	 * Called when the fileStorage filter changes to reload fileStorage records, unless the filter is changing because
	 * records are being reloaded.
	 */
	FilterChanged(): void {
		if (!this.Loading()) {
			this.GetFileStorages();
		}
	}
	
	/**
	 * Loads the saved filter that is stored in a cookie as a JSON object.
	 */
	GetSavedFilter(): void {
		this.Filter(new filterFileStoragesAuto);
		this.Filter().tenantId(this.MainModel().TenantId());
	
		// maybe this should be specific per tenant?
		let savedFilter: string = localStorage.getItem("saved-filter-fileStorages-" + this.MainModel().TenantId().toLowerCase());
		if (tsUtilities.HasValue(savedFilter)) {
			this.Filter().Load(JSON.parse(savedFilter));
			this.StartFilterMonitoring();
		}
	
		this.GetFileStorages();
	}
	
	/**
	 * Called when the filter changes or when the page loads to get the fileStorages matching the current filter.
	 */
	GetFileStorages(): void {
		// Load the filter
		this.Loading(true);
		if (this.Filter().recordsPerPage() == null || this.Filter().recordsPerPage() == 0) {
			this.Filter().recordsPerPage(10);
		}
	
		let success: Function = (data: server.filterFileStoragesAuto) => {
			this.MainModel().Message_Hide();
			if (data != null) {
				if (data.actionResponse.result) {
					this.Filter().Load(data);
	
					this.RenderFileStorageTable();
	
					this.Loading(false);
				} else {
					this.MainModel().Message_Errors(data.actionResponse.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to load File Storage records.");
			}
			this.Loading(false);
		};
	
	
		let postFilter: filterFileStoragesAuto = new filterFileStoragesAuto();
		postFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
		postFilter.columns(null);
		postFilter.records(null);
	
		let jsonData: string = ko.toJSON(postFilter);
		localStorage.setItem("saved-filter-fileStorages-" + this.MainModel().TenantId().toLowerCase(), jsonData);
		tsUtilities.AjaxData(window.baseURL + "api/Data/GetFileStoragesFilteredAuto/", jsonData, success);
	
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
				window.fileStoragesModelAuto.Filter().recordsPerPage(data);
				window.fileStoragesModelAuto.GetFileStorages();
				break;
	
			case "page":
				window.fileStoragesModelAuto.Filter().page(data);
				window.fileStoragesModelAuto.GetFileStorages();
				break;
	
			case "sort":
				window.fileStoragesModelAuto.UpdateSort(data);
				break;
		}
	}
	
	/**
	 * Called when the Refresh Filter button is clicked.
	 */
	RefreshFileStorageFilter(): void {
		this.SaveFileStorageFilter();
		this.GetFileStorages();
	}
	
	/**
	 * Renders the paged recordset view. This happens when the filter loads, but also gets called for certain SignalR events
	 * to update fileStorages that might be in the current fileStorage list.
	 */
	RenderFileStorageTable(): void {
		// Load records in the pagedRecordset
		let f: filter = new filter();
		f.Load(JSON.parse(ko.toJSON(this.Filter)));
	
		let records: any = JSON.parse(ko.toJSON(this.Filter().records));
	
		pagedRecordset.Render({
			elementId: "filestorage-records-auto",
			data: JSON.parse(ko.toJSON(f)),
			recordsetCallbackHandler: (type: string, data: any) => { this.RecordsetCallbackHandler(type, data); },
			actionHandlers: [
				{
					callbackHandler: (fileStorage: server.fileStorage) => { this.EditFileStorageCallback(fileStorage); },
					actionElement: "<button type='button' class='btn btn-sm btn-primary nowrap'>" + this.MainModel().IconAndText("FileStorage.EditTableButton") + "</button>"
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
	SaveFileStorageFilter(): void {
		let saveFilter: filterFileStoragesAuto = new filterFileStoragesAuto();
		saveFilter.Load(JSON.parse(ko.toJSON(this.Filter)));
		saveFilter.actionResponse(null);
		saveFilter.columns([]);
		saveFilter.records(null);
	
		localStorage.setItem("saved-filter-fileStorages-" + this.MainModel().TenantId().toLowerCase(), ko.toJSON(saveFilter));
	}
	
	/**
	 * Saves a fileStorage record for the fileStorage currently being added or edited.
	 */
	SaveFileStorage(): void {
		this.MainModel().Message_Hide();
		let errors: string[] = [];
		let focus: string = "";
	
	//if (!tsUtilities.HasValue(this.FileStorage().base64value())) {
	//    errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language("FileStorage.Base64value")));
	//    if (focus == "") { focus = "edit-fileStorage-base64value"; }
	//}
	
		if (errors.length > 0) {
			this.MainModel().Message_Errors(errors);
			tsUtilities.DelayedFocus(focus);
			return;
		}
	
		this.MainModel().UDFFieldsGetValues("FileStorages", this.FileStorage());
		let json: string = ko.toJSON(this.FileStorage);
	
		let success: Function = (data: server.fileStorage) => {
			this.MainModel().Message_Hide();
			if (data != null) {
				if (data.actionResponse.result) {
					this.MainModel().Nav("FileStoragesAuto");
				} else {
					this.MainModel().Message_Errors(data.actionResponse.messages);
				}
			} else {
				this.MainModel().Message_Error("An unknown error occurred attempting to save this File Storage.");
			}
		};
	
		this.MainModel().Message_Saving();
		tsUtilities.AjaxData(window.baseURL + "api/Data/SaveFileStorageAuto", json, success);
	}
	
	/**
	 * This model subscribes to SignalR updates from the MainModel so that fileStorages in the fileStorage filter list
	 * can be removed or updated when their data changes or they are deleted.
	 */
	SignalrUpdate(): void {
		//console.log("In Tenants, SignalR Update", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));
		switch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {
			case "setting":
				let fileId: string = this.MainModel().SignalRUpdate().itemId();
	
				switch (this.MainModel().SignalRUpdate().message().toLowerCase()) {
					case "deletedfileStorage":
						let records: any[] = [];
						if (this.Filter().records() != null && this.Filter().records().length > 0) {
							this.Filter().records().forEach(function (e) {
								if (e["fileId"] != fileId) {
									records.push(e);
								}
							});
						}
						this.Filter().records(records);
						this.RenderFileStorageTable();
	
						break;
	
					case "savedfileStorage":
						let fileStorageData: any = this.MainModel().SignalRUpdate().object();
	
						let index: number = -1;
						let indexItem: number = -1;
						if (this.Filter().records() != null && this.Filter().records().length > 0) {
							this.Filter().records().forEach(function (e) {
								index++;
								if (e["fileId"] == fileId) {
									indexItem = index;
								}
							});
						}
	
						if (indexItem > -1) {
							this.Filter().records()[indexItem] = JSON.parse(fileStorageData);
							this.RenderFileStorageTable();
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
		this.Filter().uploadDate.subscribe(() => { this.FilterChanged(); });
		this.Filter().uploadDateStart.subscribe(() => { this.FilterChanged(); });
		this.Filter().uploadDateEnd.subscribe(() => { this.FilterChanged(); });
		this.Filter().fileName.subscribe(() => { this.FilterChanged(); });
		this.Filter().fileNameFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().extension.subscribe(() => { this.FilterChanged(); });
		this.Filter().extensionFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().sourceFileId.subscribe(() => { this.FilterChanged(); });
		this.Filter().sourceFileIdFilterExact.subscribe(() => { this.FilterChanged(); });
		this.Filter().fileId.subscribe(() => { this.FilterChanged(); });
		this.Filter().tenantId.subscribe(() => { this.FilterChanged(); });
		this.Filter().itemId.subscribe(() => { this.FilterChanged(); });
		this.Filter().bytes.subscribe(() => { this.FilterChanged(); });
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
			this.GetFileStorages();
		}
	}
	
	/**
	 * Called when the view changes in the MainModel to do any necessary work in this viewModel.
	 */
	ViewChanged(): void {
		this.Loading(false);
	
		switch (this.MainModel().CurrentView().toLowerCase()) {
			case "editfilestorageauto":
				this.EditFileStorage();
				break;
	
			case "newfilestorageauto":
				this.AddFileStorage();
				break;
	
			case "filestoragesauto":
				this.GetSavedFilter();
				this.Filter().tenantId(this.MainModel().TenantId());
				break;
		}
	}
}
