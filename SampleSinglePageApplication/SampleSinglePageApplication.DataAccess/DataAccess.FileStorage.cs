namespace SampleSinglePageApplication;
public partial class DataAccess
{
    public async Task<DataObjects.BooleanResponse> DeleteFileStorage(Guid FileId)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == FileId);
        if (rec == null) {
            output.Messages.Add("Error Deleting File Storage " + FileId.ToString() + " - Record No Longer Exists");
        } else {
            Guid requestId = GuidValue(rec.ItemId);
            Guid tenantId = GuidValue(rec.TenantId);
            data.FileStorages.Remove(rec);
            try {
                await data.SaveChangesAsync();
                output.Result = true;

                await SignalRUpdate(new DataObjects.SignalRUpdate {
                    TenantId = tenantId,
                    RequestId = requestId,
                    ItemId = FileId.ToString(),
                    UpdateType = DataObjects.SignalRUpdateType.Files,
                    Message = "Deleted"
                });
            } catch (Exception ex) {
                output.Messages.Add("Error Deleting File Storage " + FileId.ToString() + " - " + ex.Message);
            }
        }

        return output;
    }

    private string FileIdClosestToDate(List<DataObjects.FileStorage> files, string Filename, DateTime? itemTime)
    {
        string output = String.Empty;

        DateTime checkTime = itemTime.HasValue
            ? Convert.ToDateTime(itemTime)
            : DateTime.UtcNow;

        double? TotalSeconds = null;
        var recs = files.Where(x => x.FileName != null && x.FileName.ToLower() == Filename.ToLower());
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                double Seconds = Math.Abs((checkTime - rec.UploadDate).TotalSeconds);
                if (TotalSeconds == null || Seconds < TotalSeconds) {
                    output = rec.FileId.ToString();
                    TotalSeconds = Seconds;
                }
            }
        }

        return output;
    }

    private byte[]? FileStorageToBrowser(byte[] value, string Extension, long? Bytes, bool ImagesOnly, bool ResizeAsThumbnail)
    {
        if (!Bytes.HasValue) { Bytes = 0; }

        byte[]? output = value;
        if (ImagesOnly) {
            switch (Extension.ToUpper()) {
                case ".JPG":
                case ".JPEG":
                case ".PNG":
                case ".GIF":
                    // OK to return these, but in the future these will be resized as thumbnails
                    break;
                default:
                    output = null;
                    break;
            }
        }
        return output;
    }

    public async Task<DataObjects.FileStorage> GetFileStorage(Guid FileId)
    {
        DataObjects.FileStorage output = new DataObjects.FileStorage();
        output.ActionResponse = GetNewActionResponse();

        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == FileId);
        if (rec == null) {
            output.ActionResponse.Messages.Add("File " + FileId.ToString() + " Does Not Exist");
        } else {
            output.ActionResponse.Result = true;
            output.TenantId = GuidValue(rec.TenantId);
            output.Extension = rec.Extension;
            output.Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null;
            output.FileId = FileId;
            output.FileName = rec.FileName;
            output.ItemId = rec.ItemId;
            output.SourceFileId = rec.SourceFileId;
            output.UploadDate = rec.UploadDate.HasValue ? (DateTime)rec.UploadDate : DateTime.UtcNow;
            output.UserId = rec.UserId;
            output.Value = rec.Value != null
                ? rec.Value.ToArray()
                : null;
        }

        return output;
    }

    public async Task<List<DataObjects.FileStorage>> GetFileStorageItems(Guid ItemId, bool ImagesOnly, bool ResizeAsThumbnail)
    {
        List<DataObjects.FileStorage> output = new List<DataObjects.FileStorage>();

        var recs = await data.FileStorages.Where(x => x.ItemId == ItemId).OrderBy(x => x.UploadDate).ToListAsync();
        if (recs != null && recs.Count() > 0) {
            foreach (var rec in recs) {
                long? Bytes = rec.Bytes.HasValue ? (long)rec.Bytes : (long?)null;
                byte[]? value = null;

                if (rec.Value != null && !String.IsNullOrEmpty(rec.Extension)) {
                    value = FileStorageToBrowser(rec.Value.ToArray(), rec.Extension, Bytes, ImagesOnly, ResizeAsThumbnail);
                }

                output.Add(new DataObjects.FileStorage {
                    ActionResponse = null,
                    Extension = rec.Extension,
                    Bytes = Bytes,
                    FileId = rec.FileId,
                    FileName = rec.FileName,
                    ItemId = rec.ItemId,
                    TenantId = GuidValue(rec.TenantId),
                    SourceFileId = rec.SourceFileId,
                    UploadDate = rec.UploadDate.HasValue ? (DateTime)rec.UploadDate : DateTime.UtcNow,
                    UserId = rec.UserId,
                    Value = value
                });
            }
        }

        return output;
    }

    public async Task<DataObjects.FileStorage> SaveFileStorage(DataObjects.FileStorage fileStorage)
    {
        fileStorage.ActionResponse = GetNewActionResponse();

        bool newRecord = false;
        var rec = await data.FileStorages.FirstOrDefaultAsync(x => x.FileId == fileStorage.FileId);
        if (rec == null) {
            if (fileStorage.FileId == Guid.Empty) {
                rec = new FileStorage();
                fileStorage.FileId = Guid.NewGuid();
                rec.FileId = fileStorage.FileId;
                newRecord = true;
            } else {
                fileStorage.ActionResponse.Messages.Add("Error Saving File " + fileStorage.FileId.ToString() + " - Record No Longer Exists");
                return fileStorage;
            }
        }

        if (fileStorage.UploadDate == DateTime.MinValue) {
            fileStorage.UploadDate = DateTime.UtcNow;
        }

        rec.TenantId = fileStorage.TenantId;
        rec.ItemId = fileStorage.ItemId.HasValue ? (Guid)fileStorage.ItemId : (Guid?)null;
        rec.FileName = fileStorage.FileName;
        rec.Extension = fileStorage.Extension;

        if (!String.IsNullOrWhiteSpace(fileStorage.SourceFileId)) {
            if (fileStorage.SourceFileId.Length > 100) {
                fileStorage.SourceFileId = fileStorage.SourceFileId.Substring(0, 100);
            }
            rec.SourceFileId = fileStorage.SourceFileId;
        } else {
            rec.SourceFileId = null;
        }

        rec.Bytes = fileStorage.Bytes;
        rec.Value = fileStorage.Value;
        rec.UploadDate = fileStorage.UploadDate != DateTime.MinValue ? fileStorage.UploadDate : DateTime.UtcNow;
        rec.UserId = fileStorage.UserId.HasValue ? (Guid)fileStorage.UserId : (Guid?)null;
        try {
            if (newRecord) {
                data.FileStorages.Add(rec);
            }
            await data.SaveChangesAsync();
            fileStorage.ActionResponse.Result = true;

            await SignalRUpdate(new DataObjects.SignalRUpdate {
                TenantId = fileStorage.TenantId,
                ItemId = fileStorage.ItemId.ToString(),
                UpdateType = DataObjects.SignalRUpdateType.Files,
                Message = "FileSaved",
                Object = fileStorage
            });
        } catch (Exception ex) {
            fileStorage.ActionResponse.Messages.Add("Error Saving File " + fileStorage.FileId.ToString() + " - " + ex.Message);
        }

        return fileStorage;
    }

    public async Task<List<DataObjects.FileStorage>> SaveFileStorages(List<DataObjects.FileStorage> fileStorages)
    {
        List<DataObjects.FileStorage> output = new List<DataObjects.FileStorage>();
        foreach (var fileStorage in fileStorages) {
            var saved = await SaveFileStorage(fileStorage);
            output.Add(saved);
        }
        return output;
    }
}