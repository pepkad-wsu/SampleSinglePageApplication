using System.Collections;
using System.Reflection;
using System.Text;

namespace SampleSinglePageApplication.Transcriber // Note: actual namespace depends on the project name.
{
    public class ReflectedClassInfo
    {
        public ReflectedClassInfo(PropertyInfo? pi, Type? baseType, Type mainType)
        {
            bool isNullable = false;
            bool isCollection = false;
            bool isEnum = false;
            bool isDictionary = false;
            bool isString = false;
            string defaultValue = string.Empty;

            Type innerType = null;

            if (baseType.IsArray) {
                isCollection = true;
                (isNullable, innerType) = ExamineForNullable(baseType.GetElementType());
                //Console.WriteLine($"{ (pi != null ? pi.Name : baseType.Name)} data type is an array of {innerT} (nullable: {isNullable})");
                isEnum = innerType.IsEnum;
            } else if (baseType.GetInterface(nameof(ICollection)) != null) {
                if (baseType.IsGenericType) {
                    isCollection = true;
                    (isNullable, innerType) = ExamineForNullable(baseType.GetGenericArguments()[0]);
                    //Console.WriteLine($"{(pi != null ? pi.Name : baseType.Name)} data type is collection of {innerT} (nullable: {isNullable})");
                    isEnum = innerType.IsEnum;
                } else {
                    // I think this is a dictionary, we don't really do them though right?
                    isDictionary = true;
                }
            } else {
                (isNullable, innerType) = ExamineForNullable(baseType);
                if (isNullable) {
                    //Console.WriteLine($"{(pi != null ? pi.Name : baseType.Name)} data type is nullable of {innerT} (nullable: {isNullable})");
                    isEnum = innerType.IsEnum;
                } else {
                    //Console.WriteLine($"{(pi != null ? pi.Name : baseType.Name)} data type is {baseType}");
                    isEnum = baseType.IsEnum;
                }
            }

            var actualType = (isNullable || isCollection) ? innerType : baseType;
            this.ActualType = actualType;
            this.Info = pi;
            this.BaseType = baseType;
            this.InnerType = innerType;
            this.IsEnum = isEnum;
            this.IsCollection = isCollection;
            this.IsDictionary = isDictionary;
            this.IsNullable = isNullable;
            this.MainType = mainType;

            bool isEfModel = false;
            bool isOnEFModel = false;
            bool isPrimaryKey = false;
            bool isEfVariableNullable = false;
            Type? primaryKeyType = null;
            string primaryKeyVariableName = "";
        }

        public Type? MainType { get; set; }
        public Type? ActualType { get; set; }
        public Type? BaseType { get; set; }
        public string? PascalCaseClassName { get { return ActualType.Name; } }
        public string? PascalCaseVariableName { get { return Info != null ? Info.Name : BaseType.Name; } }
        public PropertyInfo? Info { get; set; }
        public Type? InnerType { get; set; }
        public bool IsCollection { get; set; }
        public bool IsDictionary { get; set; }
        public bool IsEfModel { get; set; } = false;
        public bool IsEfVariableNullable { get; set; } = false;
        public bool IsEnum { get; set; }
        public bool IsNullable { get; set; }
        public bool IsOnEFModel { get; set; } = false;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsString { get { return typeof(string) == ActualType; } }
        public bool IsDateTime { get { return typeof(DateTime) == ActualType; } }
        public bool IsGuid { get { return typeof(Guid) == ActualType; } }
        public bool IsBool { get { return typeof(bool) == ActualType; } }
        public bool IsObject { get { return typeof(Object) == ActualType; } }
        public bool IsByte { get { return typeof(byte) == ActualType; } }

        public bool IsJavaScriptDate {
            get {
                return ActualType == typeof(DateTime)
                          || ActualType == typeof(DateOnly);
            }
        }

        public bool IsJavaScriptNumber {
            get {
                return ActualType == typeof(System.Decimal)
                          || ActualType == typeof(System.Int16)
                          || ActualType == typeof(System.Int32)
                          || ActualType == typeof(System.Int64)
                          || ActualType == typeof(long)
                          || ActualType == typeof(System.Double);
            }
        }
        public int? MaxLength { get; set; }
        public Type? PrimaryKeyType { get; set; }
        public string? PrimaryKeyVariableName { get; set; }


        public string PascalCaseVariableNamePlural { get { return ("" + PascalCaseVariableName) + "s"; } }
        public string PascalCaseClassNamePlural { get { return ("" + PascalCaseClassName) + "s"; } }
        public string CamelCaseClassName { get { return "" + ToCamelCase("" + PascalCaseClassName); } }
        public string CamelCaseClassNamePlural { get { return ToCamelCase("" + PascalCaseClassNamePlural); } }
        public string CamelCaseVariableName { get { return "" + ToCamelCase("" + PascalCaseVariableName); } }
        public string CamelCaseVariableNamePlural { get { return "" + ToCamelCase("" + PascalCaseVariableNamePlural); } }
        public string LowerCaseClassName { get { return ("" + PascalCaseClassName).ToLower(); } }
        public string LowerCaseClassNamePlural { get { return ("" + PascalCaseClassNamePlural).ToLower(); } }
        public string LowerCaseVariableName { get { return ("" + PascalCaseVariableName).ToLower(); } }
        public string LowerCaseVariableNamePlural { get { return ("" + PascalCaseVariableNamePlural).ToLower(); } }
        public string SentenceCaseClassName { get { return "" + ToSentenceCase(PascalCaseClassName); } }
        public string SentenceCaseClassNamePlural { get { return "" + ToSentenceCase(PascalCaseClassNamePlural); } }
        public string SentenceCaseVariableName { get { return "" + ToSentenceCase(PascalCaseVariableName); } }
        public string SentenceCaseVariableNamePlural { get { return "" + ToSentenceCase(PascalCaseVariableNamePlural); } }


        public Dictionary<string, string> Language {
            get {
                var output = new Dictionary<string, string>();

                // icon and text
                output.Add(Language_ClassName, Language_ClassName_Value);
                output.Add(Language_ClassNameAddNew, Language_ClassNameAddNew_Value);
                output.Add(Language_ClassNameBack, Language_ClassNameBackValue);
                output.Add(Language_ClassNameCancel, Language_ClassNameCancel_Value);
                output.Add(Language_ClassNameCancelDeleteFilter, Language_ClassNameCancelDeleteFilter_Value);
                output.Add(Language_ClassNameClear, Language_ClassNameClear_Value);
                output.Add(Language_ClassNameConfirmDelete, Language_ClassNameConfirmDelete_Value);
                output.Add(Language_ClassNameConfirmDeleteFilter, Language_ClassNameConfirmDeleteFilter_Value);
                output.Add(Language_ClassNameDelete, Language_ClassNameDelete_Value);
                output.Add(Language_ClassNameDeleteFilter, Language_ClassNameDeleteFilter_Value);
                output.Add(Language_ClassNameEdit, Language_ClassNameEdit_Value);
                output.Add(Language_ClassNameEditTableButton, Language_ClassNameEditTableButton_Value);
                output.Add(Language_ClassNameFilterCardView, Language_ClassNameFilterCardView_Value);
                output.Add(Language_ClassNameFilterExport, Language_ClassNameFilterExport_Value);
                output.Add(Language_ClassNameFilterHidingDetails, Language_ClassNameFilterHidingDetails_Value);
                output.Add(Language_ClassNameFilterListView, Language_ClassNameFilterListView_Value);
                output.Add(Language_ClassNameFilterShowingDetails, Language_ClassNameFilterShowingDetails_Value);
                output.Add(Language_ClassNameHideFilter, Language_ClassNameHideFilter_Value);
                output.Add(Language_ClassNamePlural, Language_ClassNamePlural_Value);
                output.Add(Language_ClassNameRefresh, Language_ClassNameRefresh_Value);
                output.Add(Language_ClassNameSave, Language_ClassNameSave_Value);
                output.Add(Language_ClassNameSaveFilter, Language_ClassNameSaveFilter_Value);
                output.Add(Language_ClassNameShowFilter, Language_ClassNameShowFilter_Value);

                // just text
                output.Add(Language_ClassNameSavedFilters, Language_ClassNameSavedFilters_Value);
                output.Add(Language_ClassNameSavedFilterName, Language_ClassNameSavedFilterName_Value);
                output.Add(Language_ClassNameNoRecords, Language_ClassNameNoRecords_Value);
                output.Add(Language_ClassNameLoading, Language_ClassNameLoading_Value);
                output.Add(Language_VariableName, Language_VariableName_Value);
                output.Add(Language_VariableNameStart, Language_VariableNameStart_Value);
                output.Add(Language_VariableNameEnd, Language_VariableNameEnd_Value);
                output.Add(Language_VariableNameFilterExact, Language_VariableNameFilterExact_Value);
                output.Add(Language_VariableNameIncludeInKeyword, Language_VariableNameIncludeInKeyword_Value);
                output.Add(Language_VariableNameNoRecords, Language_VariableNameNoRecords_Value);

                if (IsDateTime) {
                    output.Add(PascalCaseVariableName + "Start", SentenceCaseVariableName + " Start\");");
                    output.Add(PascalCaseVariableName + "End", SentenceCaseVariableName + " End\");");
                } else {
                    output[string.Empty + PascalCaseVariableName] = SentenceCaseVariableName;
                }

                return output;
            }
        }
 
        // language and icon
        public string Language_ClassName { get { return "" + PascalCaseClassName + ""; } }
        public string Language_ClassName_Value { get { return "" + SentenceCaseClassName + ""; } }

        public string Language_ClassNameAddNew { get { return "" + PascalCaseClassName + ".AddNew"; } }
        public string Language_ClassNameAddNew_Value { get { return "Add New"; } }
        public string Language_ClassNameBack { get { return "" + PascalCaseClassName + ".Back"; } }
        public string Language_ClassNameBackValue { get { return "Back"; } }
        public string Language_ClassNameCancel { get { return "" + PascalCaseClassName + ".Cancel"; } }
        public string Language_ClassNameCancel_Value { get { return "Cancel"; } }
        public string Language_ClassNameCancelDeleteFilter { get { return "" + PascalCaseClassName + ".CancelDeleteFilter"; } }
        public string Language_ClassNameCancelDeleteFilter_Value { get { return "Cancel"; } }
        public string Language_ClassNameClear { get { return "" + PascalCaseClassName + ".Clear"; } }
        public string Language_ClassNameClear_Value { get { return "Clear"; } }
        public string Language_ClassNameConfirmDelete { get { return "" + PascalCaseClassName + ".ConfirmDelete"; } }
        public string Language_ClassNameConfirmDelete_Value { get { return "Confirm Delete"; } }
        public string Language_ClassNameConfirmDeleteFilter { get { return "" + PascalCaseClassName + ".ConfirmDeleteFilter"; } }
        public string Language_ClassNameConfirmDeleteFilter_Value { get { return "Confirm Delete"; } }
        public string Language_ClassNameDelete { get { return "" + PascalCaseClassName + ".Delete"; } }
        public string Language_ClassNameDelete_Value { get { return "Delete"; } }
        public string Language_ClassNameDeleteFilter { get { return "" + PascalCaseClassName + ".DeleteFilter"; } }
        public string Language_ClassNameDeleteFilter_Value { get { return "Delete Filter"; } }
        public string Language_ClassNameEdit { get { return "" + PascalCaseClassName + ".Edit"; } }
        public string Language_ClassNameEdit_Value { get { return "Edit"; } }
        public string Language_ClassNameEditTableButton { get { return "" + PascalCaseClassName + ".EditTableButton"; } }
        public string Language_ClassNameEditTableButton_Value { get { return "Edit"; } }
        public string Language_ClassNameFilterCardView { get { return "" + PascalCaseClassName + ".FilterCardView"; } }
        public string Language_ClassNameFilterCardView_Value { get { return "Card View"; } }
        public string Language_ClassNameFilterExport { get { return "" + PascalCaseClassName + ".FilterExport"; } }
        public string Language_ClassNameFilterExport_Value { get { return "Export"; } }
        public string Language_ClassNameFilterHidingDetails { get { return "" + PascalCaseClassName + ".HidingDetails"; } }
        public string Language_ClassNameFilterHidingDetails_Value { get { return "Hiding Details"; } }
        public string Language_ClassNameFilterListView { get { return "" + PascalCaseClassName + ".ListView"; } }
        public string Language_ClassNameFilterListView_Value { get { return "List View"; } }
        public string Language_ClassNameFilterShowingDetails { get { return "" + PascalCaseClassName + ".ShowingDetails"; } }
        public string Language_ClassNameFilterShowingDetails_Value { get { return "Showing Details"; } }
        public string Language_ClassNameHideFilter { get { return "" + PascalCaseClassName + ".HideFilter"; } }
        public string Language_ClassNameHideFilter_Value { get { return "Hide Filter"; } }
        public string Language_ClassNamePlural { get { return "" + PascalCaseClassNamePlural + ""; } }
        public string Language_ClassNamePlural_Value { get { return "" + SentenceCaseClassNamePlural + ""; } }
        public string Language_ClassNameRefresh { get { return "" + PascalCaseClassName + ".Refresh"; } }
        public string Language_ClassNameRefresh_Value { get { return "Refresh"; } }
        public string Language_ClassNameSave { get { return "" + PascalCaseClassName + ".Save"; } }
        public string Language_ClassNameSave_Value { get { return "Save"; } }
        public string Language_ClassNameSaveFilter { get { return "" + PascalCaseClassName + ".SaveFilter"; } }
        public string Language_ClassNameSaveFilter_Value { get { return "Save Filter"; } }
        public string Language_ClassNameShowFilter { get { return "" + PascalCaseClassName + ".ShowFilter"; } }
        public string Language_ClassNameShowFilter_Value { get { return "Show Filter"; } }

        // language only

        public string Language_ClassNameSavedFilters { get { return "" + PascalCaseClassName + ".SavedFilters"; } }
        public string Language_ClassNameSavedFilters_Value { get { return "Saved Filters"; } }
        public string Language_ClassNameSavedFilterName { get { return "" + PascalCaseClassName + ".SavedFilterName"; } }
        public string Language_ClassNameSavedFilterName_Value { get { return "Saved Filter Name"; } }
        public string Language_ClassNameNoRecords { get { return "" + PascalCaseClassName + ".NoRecords"; } }
        public string Language_ClassNameNoRecords_Value { get { return "No Records"; } }
        public string Language_ClassNameLoading { get { return "" + PascalCaseClassName + ".Loading"; } }
        public string Language_ClassNameLoading_Value {get {return "Loading " + SentenceCaseClassNamePlural + ", Please Wait"; } }

        public string Language_VariableName { get { return (MainType != null ? (MainType.Name + ".") : "") + PascalCaseVariableName + ""; } }
        public string Language_VariableName_Value { get { return "" + SentenceCaseVariableName + ""; } }
        public string Language_VariableNameStart { get { return (MainType != null ? (MainType.Name + ".") : "") + PascalCaseVariableName + "Start"; } }
        public string Language_VariableNameStart_Value { get { return "" + SentenceCaseVariableName + " Start"; } }
        public string Language_VariableNameEnd { get { return (MainType != null ? (MainType.Name + ".") : "") + PascalCaseVariableName + "End"; } }
        public string Language_VariableNameEnd_Value { get { return "" + SentenceCaseVariableName + " End"; } }

        public string Language_VariableNameFilterExact { get { return (MainType != null ? (MainType.Name + ".") : "") + PascalCaseVariableName + "FilterExact"; } }
        public string Language_VariableNameFilterExact_Value { get { return "Exact"; } }
        public string Language_VariableNameIncludeInKeyword { get { return (MainType != null ? (MainType.Name + ".") : "") + PascalCaseVariableName + "IncludeInKeyword"; } }
        public string Language_VariableNameIncludeInKeyword_Value { get { return "Keyword"; } }
        public string Language_VariableNameNoRecords { get { return (MainType != null ?( MainType.Name + ".") : "" ) + PascalCaseVariableName + "NoRecords"; } }
        public string Language_VariableNameNoRecords_Value { get { return "No" + PascalCaseClassName + "Records"; } }


        //https://stackoverflow.com/questions/323314/best-way-to-convert-pascal-case-to-a-sentence
        private static string ToSentenceCase(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            var sb = new StringBuilder();
            // start with the first character -- consistent camelcase and pascal case
            sb.Append(char.ToUpper(input[0]));

            // march through the rest of it
            for (var i = 1; i < input.Length; i++) {
                // any time we hit an uppercase OR number, it's a new word
                if (char.IsUpper(input[i]) || char.IsDigit(input[i]))
                    sb.Append(' ');
                // add regularly
                sb.Append(input[i]);
            }

            return sb.ToString();
        }

        private static string ToCamelCase(string? input)
        {
            return System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName("" + input);
        }


        private static (bool nullable, Type type) ExamineForNullable(Type t)
                    => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)
                            ? (true, Nullable.GetUnderlyingType(t))
                            : (false, t);

    }
}
