﻿namespace SampleSinglePageApplication;
public partial class DataAccess
{
    /// <summary>
    /// Converts a string byte array (eg: 0x01, 0x02, 0x03) to a shorter version (eg: 010203)
    /// </summary>
    /// <param name="byteArray">A string representation of a byte array</param>
    /// <returns>A condensed version with just the byte values</returns>
    public string CompressByteArrayString(string? byteArray)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(byteArray)) {
            output = byteArray.Substring(2)
                .Replace(" ", "")
                .Replace(",0x", "");
        }

        return output;
    }

    /// <summary>
    /// Converts a compressed byte array string (eg: 010203) back to a standard byte array string (eg: 0x01, 0x02, 0x03)
    /// </summary>
    /// <param name="compressedByteArray">A compressed byte array string</param>
    /// <returns>A standard byte array string</returns>
    public string CompressedByteArrayStringToFullString(string? compressedByteArray)
    {
        System.Text.StringBuilder output = new System.Text.StringBuilder();

        if (!String.IsNullOrWhiteSpace(compressedByteArray)) {
            int len = compressedByteArray.Length;
            int pos = 0;

            while (pos < len) {
                if (pos > 0) {
                    output.Append(",");
                }
                output.Append("0x" + compressedByteArray.Substring(pos, 2));
                pos += 2;
            }
        }

        return output.ToString();
    }

    private byte[] ConvertByteArrayStringToByteArray(string ByteArrayString)
    {
        byte[] output = new byte[] { };
        if (!string.IsNullOrWhiteSpace(ByteArrayString)) {
            try {
                output = ByteArrayString.Split(',').Select(x => x.Trim().Substring(2)).Select(x => Convert.ToByte(x, 16)).ToArray();
            } catch { }
        }
        return output;
    }

    private string ConvertByteArrayToString(byte[] ByteArray)
    {
        string output = String.Empty;
        if (ByteArray != null) {
            output = "0x" + BitConverter.ToString(ByteArray).Replace("-", ",0x");
        }
        return output;
    }

    public string Decrypt(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            Encryption e = new Encryption(GetEncryptionKey);
            var decrypted = e.Decrypt(input);
            if (!String.IsNullOrEmpty(decrypted)) {
                output = decrypted;
            }
        }

        return output;
    }

    public string Encrypt(string? input)
    {
        string output = String.Empty;

        if (!String.IsNullOrEmpty(input)) {
            Encryption e = new Encryption(GetEncryptionKey);
            var encrypted = e.Encrypt(input);
            if (!String.IsNullOrEmpty(encrypted)) {
                output = encrypted;
            }
        }

        return output;
    }

    public string GenerateChecksum(string input)
    {
        Encryption e = new Encryption(GetEncryptionKey);
        string output = e.GenerateChecksum(input);
        return output;
    }

    private byte[] GetEncryptionKey {
        get {
            // The default key is hard-code here.
            byte[] output = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x30, 0x31, 0x32 };

            // See if we have cached a key from the settings
            var settingsEncryptionKey = CacheStore.GetCachedItem<string>(Guid.Empty, "EncryptionKey");

            // If it's not cached get it from the settings
            bool missingSetting = false;

            if (String.IsNullOrEmpty(settingsEncryptionKey)) {
                settingsEncryptionKey = GetSetting<string>("EncryptionKey", DataObjects.SettingType.Text);
                if (String.IsNullOrEmpty(settingsEncryptionKey)) {
                    Encryption enc = new Encryption(output);
                    missingSetting = true;
                    settingsEncryptionKey = enc.GetNewEncryptionKeyAsString();
                    output = enc.ConvertByteArrayStringToByteArray(settingsEncryptionKey);
                }
            }

            if (!String.IsNullOrEmpty(settingsEncryptionKey)) {
                CacheStore.SetCacheItem(Guid.Empty, "EncryptionKey", settingsEncryptionKey);
                var bytes = ConvertByteArrayStringToByteArray(settingsEncryptionKey);
                if (bytes != null && bytes.Length > 0) {
                    output = bytes;
                }
            }

            if (missingSetting) {
                SaveSetting("EncryptionKey", DataObjects.SettingType.Text, ConvertByteArrayToString(output));
            }

            return output;
        }
    }

    public string GetNewEncryptionKey()
    {
        string output = String.Empty;
        Encryption e = new Encryption(GetEncryptionKey);
        var newKey = e.GetNewEncryptionKey();
        if (newKey != null && newKey.Length > 0) {
            output = ConvertByteArrayToString(newKey);
        }
        return output;
    }

    private DataObjects.BooleanResponse UpdateApplicationEncryptionKey(string? oldKeyAsByteArrayString, string? newKeyAsByteArrayString)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        if (String.IsNullOrEmpty(oldKeyAsByteArrayString)) {
            output.Messages.Add("Missing Current Encryption Key");
            return output;
        }
        if (String.IsNullOrEmpty(newKeyAsByteArrayString)) {
            output.Messages.Add("Missing New Encryption Key");
            return output;
        }

        try {
            Encryption encCurrent = new Encryption(oldKeyAsByteArrayString);
            Encryption encNew = new Encryption(newKeyAsByteArrayString);

            // Decrypt and re-encrypt all encrypted settings.
            var settings = data.Settings.Where(x => x.SettingType != null && x.SettingType.ToLower() == "encryptedtext" && x.SettingText != null && x.SettingText != "");
            if (settings != null && settings.Any()) {
                foreach (var rec in settings) {
                    string currentValue = StringValue(rec.SettingText);
                    if (!String.IsNullOrEmpty(currentValue)) {
                        string decrypted = encCurrent.Decrypt(currentValue);
                        rec.SettingText = encNew.Encrypt(decrypted);
                    }
                }
            }
            data.SaveChanges();

            // Decrypt and re-encrypt all local passwords.
            var users = data.Users.Where(x => x.Password != null && x.Password != "");
            if (users != null && users.Any()) {
                foreach (var rec in users) {
                    string currentValue = StringValue(rec.Password);
                    if (!String.IsNullOrEmpty(currentValue)) {
                        string decrypted = encCurrent.Decrypt(currentValue);
                        rec.Password = encNew.Encrypt(decrypted);
                    }
                }
            }
            data.SaveChanges();

            // Update the encryption key in the settings table and clear any cached value
            SaveSetting("EncryptionKey", DataObjects.SettingType.Text, newKeyAsByteArrayString);
            CacheStore.SetCacheItem(Guid.Empty, "EncryptionKey", "");

            output.Result = true;
        } catch (Exception ex) {
            output.Messages.Add("Error Updating Encryption Key: " + ex.Message);
        }

        return output;
    }
}