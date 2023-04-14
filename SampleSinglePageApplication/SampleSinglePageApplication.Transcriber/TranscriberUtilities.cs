using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Org.BouncyCastle.Ocsp;
using System.Reflection;

namespace SampleSinglePageApplication.Transcriber
{
    internal static class TranscriberUtilities
    {
        public static TranscribedClass ClassTranscribers(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data, Type classType)
        {
            var output = new TranscribedClass(_data, classType);
            // if we are doing knockout you need to have a section to load the server version of the data Load(data: server.Version) )
            var knockoutLoadList = new List<string>();
            // then if that data object is null then we need to just set some defaults.
            var knockoutLoadDefaultList = new List<string>();
            //var camelCaseClassName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(classType.Name);
            var isActionResponse = false;
            var knockoutPropList = new List<string>();
            var dtoPropList = new List<string>();

            /// Our goal is to write out the contents of this class as a list of strings that can represent a javascript file
            ///
            /// This will look slightly different based on the if it is an enum vs class. and if we are trying to write knockout vs a server interface
            ///
            /// The default is a server interface.  For classes it can look two ways depending on if it has a base class.  Anything that we can't
            /// figure out we just assign to type any.  If it is an array we give it different array notation.
            /// ex: interface camelCaseClassName {  // C#_ClassName
            ///        camelCaseVariableName: variableType;
            ///        camelCaseVariableNameArray: variableType[];
            ///        camelCaseVariableNameUnknownType: any;
            ///        camelCaseVariableNameUnknownTypeArray: any[];
            ///        etc.. repeating for every variable on the base class            ///
            ///     }
            /// or if it extends a another class
            /// ex:  interface camelCaseName extends camelCaseOtherClassName { // C#_ClassName
            ///         camelCaseVariableName: variableType;
            ///         etc... repeating for every variable on the base class
            ///      }
            /// Then for enums its slightly different, instead of interface you say enum and we just assign numbers to each of the enum properties
            /// ex:  enum camelCaseName { // C#_EnumName
            ///         camelCaseEnumName = 0,
            ///         etc = n++,
            ///      }
            ///
            /// Or if its knockout  you need to do it slightly differently but it looks similar
            /// just using class instead of inteface and KnockoutObservable/KnockoutObservableArray for the variables.
            /// Also you don't need to redefine the enum for knockout, so just skip it entirely

            // First check if its an enum, if it is we print it differently.
            if (classType.IsEnum) {
                // enums only needed for the dto not for the model
                //if (!knockout) {
                var enumValues = System.Enum.GetValues(classType);
                var enumList = new List<string>();
                enumList.Add($"enum {output.Info.CamelCaseClassName} " + "{ //" + classType.ToString());
                int count = 0;
                foreach (var item in enumValues) {
                    enumList.Add($"\t{item} = {count},");
                    count++;
                }
                enumList.Add("}");

                foreach (string line in enumList) {
                    output.DtoTypeScriptResults.Add("" + line);
                }
                //} else {
                // we need to make the conversion methods for the knockout.  Some global method like the tsutils does.

                List<string> typeScriptEnumList = TranscriberUtilities.TranscribeTypeScriptEnumConvertMethods(classType, output.Info, enumValues);
                foreach (string line in typeScriptEnumList) {
                    output.KnockoutTypeScriptResults.Add("" + line);
                }

                ///
                /// hokay now do this again, but for c#
                ///
                // start of the convert method for the enum
                List<string> cSharpDataAccessEnumList = TranscriberUtilities.TranscribeDataAccessEnumMethods(classType, output.Info, enumValues);
                foreach (string line in cSharpDataAccessEnumList) {
                    output.CSharpDataAccessEnums.Add("" + line);
                }
            } else {
                // first reflect the object and get all of its properties
                var props = GetReflectedClassInfoList(_data, classType);

                // OK Now lets gather the typescript and dto file info, these share alot of data so just do them together
                (List<string> knockoutResults, List<string> dtoResults) = TranscriberUtilities.TranscribeKnockoutAndDtos(_data, classType, output.Info, props);
                foreach (string line in knockoutResults) {
                    output.KnockoutTypeScriptResults.Add("" + line);
                }
                foreach (string line in dtoResults) {
                    output.DtoTypeScriptResults.Add("" + line);
                }

                // now do the data access methods for the classes since we know it is a type of class
                List<string> cSharpDataAccessList = TranscriberUtilities.TranscribeDataAccessMethods(classType, output.Info, props);
                foreach (string line in cSharpDataAccessList) {
                    output.CSharpDataAccess.Add("" + line);
                }

                // cshtml partial views
                List<string> partialViewList = TranscriberUtilities.TranscribePartialViews(classType, output.Info, props);
                foreach (string line in partialViewList) {
                    output.PartialViews.Add("" + line);
                }

                // knockout view models for the partial views
                List<string> viewModelsList = TranscriberUtilities.TranscribeViewModels(classType, output.Info, props);
                foreach (string line in viewModelsList) {
                    output.ViewModels.Add("" + line);
                }
                List<string> dataObjectsList = TranscriberUtilities.TranscribeDataObjects(classType, output.Info, props);
                foreach (string line in dataObjectsList) {
                    output.CSharpDataObjects.Add("" + line);
                }
                List<string> dataControllersList = TranscriberUtilities.TranscribeDataControllers(classType, output.Info, props);
                foreach (string line in dataControllersList) {
                    output.CSharpDataControllers.Add("" + line);
                }

                // ok now do the window interface

                //output.WindowInterfaceTypeScriptResults.Add("\tinterface Window");
                //output.WindowInterfaceTypeScriptResults.Add("\t{");
                if (output.Info.IsEfModel) {
                    output.WindowInterfaceTypeScriptResults = ("\t" + output.Info.CamelCaseClassNamePlural + "ModelAuto: " + output.Info.PascalCaseClassNamePlural + "ModelAuto;            ");
                } else {
                    output.WindowInterfaceTypeScriptResults = ("\t// NOT EF MODEL //" + output.Info.CamelCaseClassNamePlural + "ModelAuto: " + output.Info.PascalCaseClassNamePlural + "ModelAuto;            ");
                }
                //output.WindowInterfaceTypeScriptResults.Add("\t}");

            }

            return output;
        }

        public static ReflectedClassInfo CreateReflectedClassInfo(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data, Type mainType, Type baseType, PropertyInfo? pi = null)
        {
            //Type baseType = pi.PropertyType;
            ReflectedClassInfo output = new ReflectedClassInfo(pi, baseType, mainType);
            bool isEfModel = false;
            bool isOnEFModel = false;
            bool isPrimaryKey = false;
            bool isEfVariableNullable = false;
            Type? primaryKeyType = null;
            string primaryKeyVariableName = "";
            int? maxSize = null;
            if (mainType != null) {
                try {
                    // hokay, so determine if the thing has a max length
                    var className = "SampleSinglePageApplication.EFModels.EFModels." + mainType.Name;
                    IEntityType model = _data.Model.FindEntityType(className);
                    if (model != null) {
                        isEfModel = true;
                        IProperty prop = model.GetProperty(output.PascalCaseVariableName);
                        isEfVariableNullable = prop.IsNullable;
                        isOnEFModel = true;
                        isPrimaryKey = prop.IsPrimaryKey();
                        if (isPrimaryKey) {
                            primaryKeyVariableName = prop.Name;
                            primaryKeyType = prop.ClrType;
                        }

                        IEnumerable<IAnnotation> ann = prop.GetAnnotations();
                        maxSize = (int?)ann.SingleOrDefault(item => item.Name.Equals("MaxLength"))?.Value;
                    }
                } catch (Exception ex) {
                    // Console.WriteLine(ex.Message);
                }
            }

            output.IsEfModel = isEfModel;
            output.IsEfVariableNullable = isEfVariableNullable;
            output.IsPrimaryKey = isPrimaryKey;
            output.PrimaryKeyType = primaryKeyType;
            output.PrimaryKeyVariableName = primaryKeyVariableName;
            output.IsOnEFModel = isOnEFModel;
            output.MaxLength = maxSize;

            return output;
        }

        /// <summary>
        /// list of all the classes and enums we want dto / knockout models of
        /// </summary>
        /// <returns></returns>
        public static List<Type> DefaultList(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data)
        {
            var output = new List<Type>();

            var list = typeof(DataObjects).GetNestedTypes();
            foreach (var type in list) {
                try {
                    output.Add(type);
                } catch (Exception ex) {
                    //Console.WriteLine("ex: " + ex.Message);
                }
            }

            output = output.Distinct().ToList();

            output = output.ToList().OrderByDescending(o => CreateReflectedClassInfo(_data, null, o).IsEnum).ThenBy(o => CreateReflectedClassInfo(_data, null, o).PascalCaseClassName).ToList();

            return output;
        }

        ///https://stackoverflow.com/questions/67237719/using-reflection-how-can-we-tell-if-a-class-property-is-a-nullable-collection-an
        public static List<ReflectedClassInfo> GetReflectedClassInfoList(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data, Type mainType)
        {
            var output = new List<ReflectedClassInfo>();

            foreach (PropertyInfo pi in mainType.GetProperties()) {
                var result = CreateReflectedClassInfo(_data, mainType, pi.PropertyType, pi);
                output.Add(result);
            }

            return output;
        }

        public static List<TranscribedClass> TranscribeClasses(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data, List<Type>? types = null)
        {
            var output = new List<TranscribedClass>();

            if (types == null || types.Count == 0) {
                // default types for this project
                types = DefaultList(_data);
            }

            foreach (var type in types) {
                output.Add(ClassTranscribers(_data, type));
            }

            return output;
        }

        /// <summary>
        /// /// This takes an enum and returns a list of strings that can be used to represent that enum
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="classInfo"></param>
        /// <param name="enumValues"></param>
        /// <returns></returns>
        public static List<string> TranscribeDataAccessEnumMethods(Type classType, ReflectedClassInfo classInfo, Array enumValues)
        {

            List<string> output = new List<string>();
            output.Add("namespace SampleSinglePageApplication;");
            List<string> dataAccessEnumList = new List<string>();
            List<string> iDataAccessList = new List<string>();
            iDataAccessList.Add("public partial interface IDataAccess");
            iDataAccessList.Add("{");
            dataAccessEnumList.Add("public partial class DataAccess");
            dataAccessEnumList.Add("{");

            var enumStringValues = new List<string>();

            foreach (var enumItem in enumValues) {
                enumStringValues.Add(enumItem.ToString());
            }
            enumStringValues = enumStringValues.OrderBy(o => o).ToList();

            int count = 0;

            // FromString

            var convertFromStringName = $"static DataObjects.{classInfo.PascalCaseClassName}? Convert{classInfo.PascalCaseClassName}FromString(string? value)";
            iDataAccessList.Add("\t" + convertFromStringName + " => throw new NotImplementedException();");

            dataAccessEnumList.Add($"\tpublic {convertFromStringName}  //" + classType.ToString());
            dataAccessEnumList.Add("\t{");

            var defaultFromStringOutput = "null";
            if (enumStringValues.Contains("Unknown")) {
                defaultFromStringOutput = $"DataObjects.{classInfo.PascalCaseClassName}.Unknown";
            }

            dataAccessEnumList.Add($"\t\tDataObjects.{classInfo.PascalCaseClassName}? output = {defaultFromStringOutput};");
            dataAccessEnumList.Add("");

            // start of the if checking for value
            dataAccessEnumList.Add("\t\tif (!string.IsNullOrWhiteSpace(value)) {");
            // start of the switch of the value
            dataAccessEnumList.Add("\t\t\tvar lowerValue = (\"\" + value.ToString()).ToLower();");
            dataAccessEnumList.Add("\t\t\tswitch (lowerValue) {");

            count = 0;

            foreach (string value in enumStringValues) {
                //enumList.Add($"\t\t\t\tcase {count}:");
                //enumList.Add($"\t\t\t\tcase \"{count}\":");
                dataAccessEnumList.Add($"\t\t\t\tcase \"{value.ToLower()}\":");

                dataAccessEnumList.Add($"\t\t\t\t\toutput = DataObjects.{classInfo.PascalCaseClassName}.{value};");
                dataAccessEnumList.Add($"\t\t\t\t\tbreak;");
                count++;
            }
            dataAccessEnumList.Add("\t\t\t\tdefault:");
            dataAccessEnumList.Add("\t\t\t\t\tbreak;");

            // close the switch
            dataAccessEnumList.Add("\t\t\t}");
            // close the if
            dataAccessEnumList.Add("\t\t}");
            dataAccessEnumList.Add("");
            dataAccessEnumList.Add("\t\treturn output;");
            // close the function method
            dataAccessEnumList.Add("\t}");

            // ToString
            // and a header
            dataAccessEnumList.Add("\t///");
            dataAccessEnumList.Add("\t/// " + classInfo.PascalCaseClassName);
            dataAccessEnumList.Add("\t///");
            dataAccessEnumList.Add("\t/// WARNING: AUTO GENERATED FILE - DO NOT MODIFY BY HAND");
            dataAccessEnumList.Add("\t/// GENERATED BY: SampleSinglePageApplication.Transcriber console application.");
            dataAccessEnumList.Add("\t///   To regenerate the file, first update the path varibale in the program.cs then run the console app.");
            dataAccessEnumList.Add("\t///");

            var convertToStringName = $"static string Convert{classInfo.PascalCaseClassName}ToString(DataObjects.{classInfo.PascalCaseClassName}? value)";
            iDataAccessList.Add("\t" + convertToStringName + " => throw new NotImplementedException();");

            dataAccessEnumList.Add($"\tpublic {convertToStringName} //" + classType.ToString());
            dataAccessEnumList.Add("\t{");

            var defaultToStringOutput = "\"\"";
            if (enumStringValues.Contains("Unknown")) {
                defaultToStringOutput = "\"unknown\"";
            }

            dataAccessEnumList.Add($"\t\tvar output = {defaultToStringOutput};");
            dataAccessEnumList.Add("");
            // start of the if checking for value
            dataAccessEnumList.Add("\t\tif (value != null)");
            dataAccessEnumList.Add("\t\t{");
            // start of the switch of the value
            //cSharpEnumList.Add("\t\t\tvar lowerValue  =(\"\" + value.ToString()).ToLower();");
            dataAccessEnumList.Add("\t\t\tswitch (value)");
            dataAccessEnumList.Add("\t\t\t{");

            count = 0;
            foreach (var item in enumValues) {
                //enumList.Add($"\t\t\t\tcase {count}:");
                //enumList.Add($"\t\t\t\tcase \"{count}\":");
                dataAccessEnumList.Add($"\t\t\t\tcase DataObjects.{classInfo.PascalCaseClassName}.{item.ToString()}:");

                dataAccessEnumList.Add($"\t\t\t\t\toutput = \"{item.ToString().ToLower()}\";");
                dataAccessEnumList.Add($"\t\t\t\t\tbreak;");
                count++;
            }
            dataAccessEnumList.Add("\t\t\tdefault:");
            dataAccessEnumList.Add("\t\t\t\tbreak;");

            // close the switch
            dataAccessEnumList.Add("\t\t\t}");
            // close the if
            dataAccessEnumList.Add("\t\t}");
            dataAccessEnumList.Add("");
            dataAccessEnumList.Add("\t\treturn output;");
            // close the function method
            dataAccessEnumList.Add("\t}");

            dataAccessEnumList.Add("");

            dataAccessEnumList.Add("}");
            iDataAccessList.Add("}");
            iDataAccessList.Add("");
            output.AddRange(iDataAccessList);
            output.AddRange(dataAccessEnumList);
            return output;
        }

        /// <summary>
        /// /// This takes an enum and returns a list of strings that can be used to represent that enum
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="classInfo"></param>
        /// <param name="enumValues"></param>
        /// <returns></returns>
        public static List<string> TranscribeDataAccessMethods(Type classType, ReflectedClassInfo classInfo, List<ReflectedClassInfo> props)
        {
            int count = 0;
            if (!classInfo.IsEfModel) {
                return new List<string>();
            }
            var primaryKey = props.FirstOrDefault(o => o.IsPrimaryKey);

            var primaryKeyString = classInfo.PascalCaseClassName + "Id";
            var primaryKeyType = "Guid";
            if (primaryKey != null) {
                primaryKeyString = primaryKey.PascalCaseVariableName;
                primaryKeyType = primaryKey.PascalCaseClassName;
            }

            List<string> output = new List<string>();
            output.Add("namespace SampleSinglePageApplication;");
            List<string> dataAccessList = new List<string>();
            List<string> iDataAccessList = new List<string>();
            List<string> languageList = new List<string>();
            List<string> iconList = new List<string>();

            iDataAccessList.Add("public partial interface IDataAccess");
            iDataAccessList.Add("{");

            dataAccessList.Add("public partial class DataAccess");
            dataAccessList.Add("{");

            //Convert from ef model to data object
            #region
            var convertSingleMethodName = $"DataObjects.{classInfo.PascalCaseClassName} Convert{classInfo.PascalCaseClassName}Auto(SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName} item)";
            iDataAccessList.Add("\t" + convertSingleMethodName + ";");

            dataAccessList.Add($"\tpublic {convertSingleMethodName} //" + classType.ToString());
            dataAccessList.Add("\t{");

            // WARNING: Hardcoded for UserGroupSettings.... todo: figure out how to find strings on ef model and classes on dataobject then convert
            if(classInfo.PascalCaseClassName == "UserGroup") {
                dataAccessList.Add("");
                dataAccessList.Add("\t\tvar settings = DeserializeObject<DataObjects.UserGroupSettings>(item.Settings);");
                dataAccessList.Add("\t\tif (settings == null) {");
                dataAccessList.Add("\t\t\tsettings = new DataObjects.UserGroupSettings();");
                dataAccessList.Add("\t\t}");
                dataAccessList.Add("");
            }

            dataAccessList.Add($"\t\tvar output = new DataObjects.{classInfo.PascalCaseClassName}" + " {");
            foreach (var prop in props) {
                if (prop.IsEnum) {
                    dataAccessList.Add($"\t\t\t{prop.PascalCaseVariableName} = Convert{prop.PascalCaseClassName}FromString(item.{prop.PascalCaseVariableName}), //ENUM: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                } else if (prop.IsCollection) {
                    dataAccessList.Add($"\t\t\t//LIST: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                } else {
                    if (prop.PascalCaseClassName == "BooleanResponse") {
                        //output.Add($"\t\t//Bool Response - not in the db: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                    } else {
                        // update the rec in the database for each property that actually exists
                        // TODO: check against the actual model stored in the efmodels project
                        if (prop.IsEfVariableNullable && !prop.IsNullable) {
                            if (prop.BaseType.Name == "Guid" && prop.ActualType.Name == "Guid") {
                                // we have a guid that cannot be null
                                dataAccessList.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.PascalCaseVariableName} = GuidOrEmpty(item.{prop.PascalCaseVariableName}), //GUID nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name == "Boolean" && prop.ActualType.Name == "Boolean") {
                                // we have a bool that cannot be null
                                dataAccessList.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? false, //BOOL nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name == "String" && prop.ActualType.Name == "String") {
                                // we have a bool that cannot be null
                                dataAccessList.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? \"\", //string nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name == "Int32" && prop.ActualType.Name == "Int32") {
                                // we have an int that cannot be null
                                dataAccessList.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? 0, //int nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name == "DateTime" && prop.ActualType.Name == "DateTime") {
                                // we have an DateTime that cannot be null
                                dataAccessList.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? new DateTime(), //int nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name != "Int32" && prop.ActualType.Name == "Int32" && prop.IsOnEFModel) {
                                dataAccessList.Add($"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}, //nullable int on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name != "DateTime" && prop.ActualType.Name == "DateTime" && prop.IsOnEFModel) {
                                dataAccessList.Add($"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}, //nullable DateTime on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.BaseType.Name != "Guid" && prop.ActualType.Name == "Guid" && prop.IsOnEFModel) {
                                dataAccessList.Add($"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}, //nullable guid on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else if (prop.IsOnEFModel) {
                                if (prop.ActualType == typeof(DataObjects.UserGroupSettings)) {
                                    // hardcoded for UserGroupSettings
                                    dataAccessList.Add($"\t\t\t{prop.PascalCaseVariableName} = settings, //string nullable on ef model and DataObjects.UserGroupSettings on dataobjects : base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                                } else {
                                    dataAccessList.Add($"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}, //OTHER nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                                }
                            } else {
                                dataAccessList.Add("// not on ef model" + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? \"\", //OTHER nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            }
                        }
                        //else if (prop.BaseType.Name == "Boolean" && prop.ActualType.Name == "Boolean")
                        //{
                        //    output.Add((prop.IsOnEFModel ? "" : "//") + $"\t\t\t{prop.CSharpVariableName} = item.{prop.CSharpVariableName} ?? False, //ENUM: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                        //}
                        else {
                            dataAccessList.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}, //OTHER: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                        }
                    }
                }
            }
            dataAccessList.Add("\t\t};");
            dataAccessList.Add("\t\treturn output;");
            dataAccessList.Add("\t}");
            #endregion

            // delete single
            #region
            var deleteSingleMethodName = $"Task<DataObjects.BooleanResponse> Delete{classInfo.PascalCaseClassName}Auto({primaryKeyType} {classInfo.CamelCaseClassName}Id, bool trySave = true)";
            iDataAccessList.Add("\t" + deleteSingleMethodName + ";");

            dataAccessList.Add($"\tpublic async {deleteSingleMethodName} //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add("\t\tDataObjects.BooleanResponse output = new DataObjects.BooleanResponse();");
            dataAccessList.Add($"\t\tvar rec = await data.{classInfo.PascalCaseClassNamePlural}.FirstOrDefaultAsync(x => x.{primaryKeyString} == {classInfo.CamelCaseClassName}Id);");
            dataAccessList.Add("\t\tif (rec == null) {");
            dataAccessList.Add($"\t\t\toutput.Messages.Add(\"Error Deleting {classInfo.PascalCaseClassName} \" + {classInfo.CamelCaseClassName}Id.ToString() + \" - Record No Longer Exists\");");
            dataAccessList.Add("\t\t} else {");
            dataAccessList.Add($"\t\t\tdata.{classInfo.PascalCaseVariableNamePlural}.Remove(rec);");
            dataAccessList.Add("\t\t\ttry {");
            dataAccessList.Add("\t\t\t\tif (trySave) {");
            dataAccessList.Add("\t\t\t\t\tawait data.SaveChangesAsync();");
            dataAccessList.Add("\t\t\t\t}");
            dataAccessList.Add("\t\t\t\toutput.Result = true;");
            dataAccessList.Add("\t\t\t\t");
            //if(props.Any(o => o.CSharpVariableName == "TenantId")) {
            //    dataAccessList.Add("\t\t\t\tif (rec.TenantId != null && rec.TenantId != Guid.Empty) {");
            //    if(props.Single(o => o.CSharpVariableName == "TenantId").IsNullable) {
            //        dataAccessList.Add("\t\t\t\t\tClearTenantCache(rec.TenantId.Value);");
            //    } else {
            //        dataAccessList.Add("\t\t\t\t\tClearTenantCache(rec.TenantId);");
            //    }
            //    dataAccessList.Add("\t\t\t\t}");
            //}
            dataAccessList.Add("\t\t\t\tawait SignalRUpdate(new DataObjects.SignalRUpdate {");
            if (props.Any(o => o.PascalCaseVariableName == "TenantId")) {
                dataAccessList.Add("\t\t\t\t\tTenantId = rec.TenantId,");
            } else {
                dataAccessList.Add("\t\t\t\t\tTenantId = null,");
            }
            dataAccessList.Add($"\t\t\t\t\tItemId = rec.{primaryKeyString}.ToString(),");
            dataAccessList.Add($"\t\t\t\t\tUpdateType = DataObjects.SignalRUpdateType.{classInfo.PascalCaseClassName},");
            dataAccessList.Add($"\t\t\t\t\tMessage = \"Deleted{classInfo.PascalCaseClassName}\"");
            dataAccessList.Add("\t\t\t\t});");
            dataAccessList.Add("\t\t\t} catch (Exception ex) {");
            dataAccessList.Add($"\t\t\t\toutput.Messages.Add(\"Error Deleting {classInfo.PascalCaseClassName} \" + {classInfo.CamelCaseClassName}Id.ToString() + \" - \" + ex.Message);");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\treturn output;");

            dataAccessList.Add("\t}");
            dataAccessList.Add("");
            #endregion

            // delete many
            #region
            var deleteManyMethodName = $"Task<DataObjects.BooleanResponse> Delete{classInfo.PascalCaseClassNamePlural}Auto(List<{primaryKeyType}> {classInfo.CamelCaseClassName}Ids)";
            iDataAccessList.Add("\t" + deleteManyMethodName + ";");

            dataAccessList.Add($"\tpublic async {deleteManyMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add("\t\tDataObjects.BooleanResponse output = new DataObjects.BooleanResponse();");
            dataAccessList.Add($"\t\tvar recs = await data.{classInfo.PascalCaseClassNamePlural}.Where(x => {classInfo.CamelCaseClassName}Ids.Any(id => id == x.{primaryKeyString})).ToListAsync();");
            dataAccessList.Add($"\t\tdata.{classInfo.PascalCaseClassNamePlural}.RemoveRange(recs);");
            dataAccessList.Add("\t\ttry {");
            dataAccessList.Add("\t\t\tawait data.SaveChangesAsync();");
            dataAccessList.Add("\t\t\toutput.Result = true;");
            dataAccessList.Add("\t\t} catch (Exception ex) {");

            dataAccessList.Add($"\t\t\toutput.Messages.Add(\"Error Deleting {classInfo.PascalCaseClassNamePlural} \" + string.Join(\",\", {classInfo.CamelCaseClassName}Ids.Select(o => o.ToString())) + \" - \" + ex.Message);");
            dataAccessList.Add("\t\t}");

            dataAccessList.Add("\t\treturn output;");

            dataAccessList.Add("\t}");
            dataAccessList.Add("");
            #endregion

            // get single
            #region
            var getSingleByFilterMethodName = $"Task<DataObjects.{classInfo.PascalCaseClassName}> Get{classInfo.PascalCaseClassName}Auto(Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? filter = null, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<DataObjects.{classInfo.PascalCaseClassName}>>? convert = null)";
            iDataAccessList.Add("\t" + getSingleByFilterMethodName + ";");
            dataAccessList.Add($"\tpublic async {getSingleByFilterMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add($"\t\treturn await Get{classInfo.PascalCaseClassName}AutoPrivate(null, filter, convert);");
            dataAccessList.Add("\t}");

            var getSingleMethodName = $"Task<DataObjects.{classInfo.PascalCaseClassName}> Get{classInfo.PascalCaseClassName}Auto({primaryKeyType} id,Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? filter = null, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<DataObjects.{classInfo.PascalCaseClassName}>>? convert = null)";
            iDataAccessList.Add("\t" + getSingleMethodName + ";");
            dataAccessList.Add($"\tpublic async {getSingleMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add($"\t\treturn await Get{classInfo.PascalCaseClassName}AutoPrivate(id, filter, convert);");
            dataAccessList.Add("\t}");

            var privateGetSingleMethodName = $"Task<DataObjects.{classInfo.PascalCaseClassName}> Get{classInfo.PascalCaseClassName}AutoPrivate({primaryKeyType}? id, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? filter = null, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<DataObjects.{classInfo.PascalCaseClassName}>>? convert = null)";
            //iDataAccessList.Add("\t" + getSingleMethodName + ";");

            dataAccessList.Add($"\tprivate async {privateGetSingleMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add($"\t\tDataObjects.{classInfo.PascalCaseClassName} output = new DataObjects.{classInfo.PascalCaseClassName}();");
            dataAccessList.Add("\t\toutput.ActionResponse = GetNewActionResponse();");
            dataAccessList.Add($"\t\tList<DataObjects.{classInfo.PascalCaseClassName}> records = null;");
            dataAccessList.Add($"\t\tif(id != null) " + "{");//
            dataAccessList.Add($"\t\t\trecords = await Get{classInfo.PascalCaseClassNamePlural}Auto(new List<{primaryKeyType}>()" + " { id.Value }, filter, convert );");
            dataAccessList.Add($"\t\t" + "} else {");//
            dataAccessList.Add($"\t\t\trecords = await Get{classInfo.PascalCaseClassNamePlural}Auto(null, filter, convert );");//
            dataAccessList.Add($"\t\t" + "}");//
            dataAccessList.Add("\t\tif (records != null && records.Count() > 0) {");
            dataAccessList.Add("\t\t\toutput = records.Single();");
            dataAccessList.Add("\t\t\toutput.ActionResponse.Result = true;");

            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\telse {");
            dataAccessList.Add($"\t\t\toutput.ActionResponse.Messages.Add(\"{classInfo.PascalCaseClassName} \" + id.ToString() + \" Does Not Exist\");");
            dataAccessList.Add("\t\t}");
            //await Get{classInfo.CSharpClassName}sAuto(new List<Guid>()" + "{id}, filter, convert );
            dataAccessList.Add($"\t\treturn output;");
            dataAccessList.Add("\t}");
            #endregion

            // get multiple
            #region
            var getManyMethodName = $"Task<List<DataObjects.{classInfo.PascalCaseClassName}>> Get{classInfo.PascalCaseClassNamePlural}Auto(List<{primaryKeyType}>? ids = null, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? filter = null, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<DataObjects.{classInfo.PascalCaseClassName}>>? convert = null)";
            iDataAccessList.Add("\t" + getManyMethodName + ";");

            dataAccessList.Add($"\tpublic async {getManyMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add($"\t\tList<DataObjects.{classInfo.PascalCaseClassName}> output = new List<DataObjects.{classInfo.PascalCaseClassName}>();");
            dataAccessList.Add($"\t\tIQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>? recs = null;");
            dataAccessList.Add("\t\tif (ids != null) {");
            dataAccessList.Add($"\t\t\trecs = data.{classInfo.PascalCaseClassNamePlural}.Where(o => ids.Any(id => id == o.{primaryKeyString})).AsQueryable();");
            dataAccessList.Add("\t\t} else {");
            dataAccessList.Add($"\t\t\trecs = data.{classInfo.PascalCaseClassNamePlural}.AsQueryable();");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\tif (filter != null) {");
            dataAccessList.Add("\t\t\trecs = filter(recs);");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\tif (convert != null) {");
            dataAccessList.Add("\t\t\toutput = await (convert(recs)).ToListAsync();");
            dataAccessList.Add("\t\t} else {");

            dataAccessList.Add($"\t\t\tList<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}> records = await recs.ToListAsync();");
            //                $".Select(o => new DataObjects.{classInfo.CSharpClassName}" + " {");
            dataAccessList.Add("\t\t\tforeach(var o in records) {");
            dataAccessList.Add($"\t\t\t\toutput.Add(Convert{classInfo.PascalCaseClassName}Auto(o));");
            //output.Add($"\t\t\t\toutput.Add(new DataObjects.{classInfo.CSharpClassName}" + " {");
            //output.Add($"\t\t\toutput = (await recs.ToListAsync()).Select(o => new DataObjects.{classInfo.CSharpClassName}" + " {");

            //foreach (var prop in props)
            //{
            //    if (prop.IsEnum)
            //    {
            //        output.Add($"\t\t\t\t\t{prop.CSharpVariableName} = Convert{prop.CSharpClassName}FromString(o.{prop.CSharpVariableName}), //ENUM: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
            //    }
            //    else if (prop.IsCollection)
            //    {
            //        output.Add($"\t\t\t\t\t//LIST: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
            //    }
            //    else
            //    {
            //        if (prop.CSharpClassName == "BooleanResponse")
            //        {
            //            //output.Add($"\t\t//Bool Response - not in the db: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
            //        }
            //        else
            //        {
            //            // update the rec in the database for each property that actually exists
            //            // TODO: check against the actual model stored in the efmodels project
            //            output.Add($"\t\t\t\t\t{prop.CSharpVariableName} = o.{prop.CSharpVariableName}, //OTHER: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
            //        }
            //    }
            //}

            //output.Add("\t\t\t\t});");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("");

            dataAccessList.Add("\t\treturn output;");
            // close the function method
            dataAccessList.Add("\t}");
            #endregion

            // ok now lets do the get filtered auto
            #region
            var getFilterMethodName = $"Task<DataObjects.Filter{classInfo.PascalCaseClassNamePlural}Auto> Get{classInfo.PascalCaseClassNamePlural}FilteredAuto(DataObjects.Filter{classInfo.PascalCaseClassNamePlural}Auto filter, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? overrideFilter = null,Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? additionalFilter = null, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>,string, bool ,IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? additionalOrderByAuto = null, Func<DataObjects.{classInfo.PascalCaseClassName}, DataObjects.{classInfo.PascalCaseClassName}> additionalDataAuto = null  )";
            iDataAccessList.Add("\t" + getFilterMethodName + ";");

            dataAccessList.Add($"\tpublic async {getFilterMethodName}                    ");
            dataAccessList.Add("\t{");
            dataAccessList.Add($"\t\tDataObjects.Filter{classInfo.PascalCaseClassNamePlural}Auto output = filter;                                                                       ");
            dataAccessList.Add("\t\toutput.ActionResponse = GetNewActionResponse();");
            dataAccessList.Add("\t\toutput.Records = null;");
            dataAccessList.Add("\t\t");
            dataAccessList.Add("\t\toutput.Columns = new List<DataObjects.FilterColumn> {");

            foreach (var prop in props) {
                if (prop.IsOnEFModel && !prop.IsCollection && !prop.IsDictionary) {
                    // if (prop.ActualType.Name == "Guid")
                    // {
                    //     // we have a guid that cannot be null
                    //     output.Add((prop.IsOnEFModel ? "" : "// not on ef model") + $"\t\t\t{prop.CSharpVariableName} = GuidOrEmpty(item.{prop.CSharpVariableName}), //GUID nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                    // } else
                    if (prop.IsBool) {
                        dataAccessList.Add("\t\t\tnew DataObjects.FilterColumn{");
                        dataAccessList.Add($"\t\t\t\tAlign = \"center\",                                                                                      ");
                        dataAccessList.Add($"\t\t\t\tLabel = \"{prop.SentenceCaseVariableName}\",                                                                   ");
                        dataAccessList.Add($"\t\t\t\tTipText = \"\",                                                                                          ");
                        dataAccessList.Add($"\t\t\t\tSortable = true,                                                                                         ");
                        dataAccessList.Add($"\t\t\t\tDataElementName = \"{prop.CamelCaseVariableName}\",                                                      ");
                        dataAccessList.Add($"\t\t\t\tDataType = \"boolean\"                                                                                   ");
                        dataAccessList.Add("\t\t\t},");
                    } else if ((prop.IsString || prop.IsGuid) && prop.PascalCaseVariableName != "RawJson") {
                        dataAccessList.Add("\t\t\tnew DataObjects.FilterColumn{");
                        dataAccessList.Add($"\t\t\t\tAlign = \"\",                                                                                            ");
                        dataAccessList.Add($"\t\t\t\tLabel = \"{prop.SentenceCaseVariableName}\",                                                                   ");
                        dataAccessList.Add($"\t\t\t\tTipText = \"\",                                                                                          ");
                        dataAccessList.Add($"\t\t\t\tSortable = true,                                                                                         ");
                        dataAccessList.Add($"\t\t\t\tDataElementName = \"{prop.CamelCaseVariableName}\",                                                      ");
                        dataAccessList.Add($"\t\t\t\tDataType = \"string\"                                                                                    ");
                        dataAccessList.Add("\t\t\t},");
                    } else if (prop.IsJavaScriptNumber) {
                        dataAccessList.Add("\t\t\tnew DataObjects.FilterColumn{");
                        dataAccessList.Add($"\t\t\t\tAlign = \"\",                                                                                            ");
                        dataAccessList.Add($"\t\t\t\tLabel = \"{prop.SentenceCaseVariableName}\",                                                                   ");
                        dataAccessList.Add($"\t\t\t\tTipText = \"\",                                                                                          ");
                        dataAccessList.Add($"\t\t\t\tSortable = true,                                                                                         ");
                        dataAccessList.Add($"\t\t\t\tDataElementName = \"{prop.CamelCaseVariableName}\",                                                      ");
                        dataAccessList.Add($"\t\t\t\tDataType = \"number\"                                                                                    ");
                        dataAccessList.Add("\t\t\t},");
                    } else if (prop.IsJavaScriptDate) {
                        dataAccessList.Add("\t\t\tnew DataObjects.FilterColumn{");
                        dataAccessList.Add($"\t\t\t\tAlign = \"\",                                                                                            ");
                        dataAccessList.Add($"\t\t\t\tLabel = \"{prop.SentenceCaseVariableName}\",                                                                   ");
                        dataAccessList.Add($"\t\t\t\tTipText = \"\",                                                                                          ");
                        dataAccessList.Add($"\t\t\t\tSortable = true,                                                                                         ");
                        dataAccessList.Add($"\t\t\t\tDataElementName = \"{prop.CamelCaseVariableName}\",                                                      ");
                        dataAccessList.Add($"\t\t\t\tDataType = \"datetime\"                                                                                  ");
                        dataAccessList.Add("\t\t\t},");
                    } else if (prop.IsEnum) {
                        dataAccessList.Add("\t\t\tnew DataObjects.FilterColumn{");
                        dataAccessList.Add($"\t\t\t\tAlign = \"\",                                                                                            ");
                        dataAccessList.Add($"\t\t\t\tLabel = \"{prop.SentenceCaseVariableName}\",                                                                   ");
                        dataAccessList.Add($"\t\t\t\tTipText = \"\",                                                                                          ");
                        dataAccessList.Add($"\t\t\t\tSortable = true,                                                                                         ");
                        dataAccessList.Add($"\t\t\t\tDataElementName = \"{prop.CamelCaseVariableName}LabelAuto\",                                                      ");
                        dataAccessList.Add($"\t\t\t\tDataType = \"string\"                                                                                  ");
                        dataAccessList.Add("\t\t\t},");
                        //public List<SampleSinglePageApplicationSourceType>? Type { get; set; } = new List<SampleSinglePageApplicationSourceType>();
                        //public List<SampleSinglePageApplicationCategoryType>? Category { get; set; } = new List<SampleSinglePageApplicationCategoryType>();
                        dataAccessList.Add("\t\t\t// todo: enums.. not ready yet " + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? \"\", //OTHER nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                    } else if (prop.PascalCaseVariableName != "RawJson") {
                        // we have a bool that cannot be null
                        if (prop.ActualType == typeof(DataObjects.UserGroupSettings)) {
                            // hardcoded for UserGroupSettings
                            output.Add("\t\t// Dont need filtering for " + prop.PascalCaseVariableName);

                        } else {
                            dataAccessList.Add(("ERROR: // not an int, bool, guid or string, or datetime. but it still on the ef model, how odd, maybe like a double? ") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? \"\", //OTHER nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                        }
                    }
                }
            }

            // ok now we do some filtering i guess... this is where we should just do the if filter crap
            dataAccessList.Add("\t\t};");

            dataAccessList.Add("\t");

            dataAccessList.Add($"\t\tIQueryable<{classInfo.PascalCaseClassName}> recs = data.{classInfo.PascalCaseClassNamePlural}; ");
            if (props.Any(p => p.PascalCaseVariableName == "TenantId" && p.PascalCaseClassName != "Tenant")) {
                dataAccessList.Add($"\t\trecs = recs.Where(x => x.TenantId == output.TenantId);                                               ");
            } else {
                dataAccessList.Add($"\t\t// NO TENANTID ON {classInfo.PascalCaseClassName} // recs = recs.Where(x => x.TenantId == output.TenantId);                                               ");
            }

            //foreach(var prop in props.Where(o => o.IsEnum && o.IsOnEFModel))
            //{
            //    output.Add($"    var enum{prop.CSharpClassName}s = output.Category.Select(o => ConvertSampleSinglePageApplicationCategoryTypeToString(o)).ToList();                 ");
            //    output.Add("    if (output.Category != null && output.Category.Count() > 0)");
            //    output.Add("    {");
            //    output.Add("        recs = recs.Where(x => x.Category != null && categories.Any(o => o == x.Category));");
            //    output.Add("    }");
            //}

            dataAccessList.Add("\t\tif (overrideFilter != null) {");
            dataAccessList.Add("\t\t\trecs = overrideFilter(recs);");
            dataAccessList.Add("\t\t} else {");
            dataAccessList.Add("\t\t\tif(additionalFilter != null) " + "{");
            dataAccessList.Add("\t\t\t\trecs = additionalFilter(recs);");
            dataAccessList.Add("\t\t\t" + "}");

            // map out each variable for searching, but if its a string also include it in the keyword search filter
            // make a list of all the prop filtters
            var tempPropFilterList = new List<string>();
            foreach (var prop in props.Where(o => o.IsOnEFModel && !o.IsCollection && !o.IsDictionary)) {
                if (prop.IsEnum) {
                    tempPropFilterList.Add($"\t\t\tif (filter.{prop.PascalCaseVariableNamePlural} != null && filter.{prop.PascalCaseVariableNamePlural}.Count() > 0)" + " { " + $"// ENUM: {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t\trecs = recs.Where(o => filter.{prop.PascalCaseVariableNamePlural}.Select(t => Convert{prop.PascalCaseClassName}ToString(t)).Any(c => c == o.{prop.PascalCaseVariableName}));");
                    tempPropFilterList.Add($"\t\t\t" + "}");
                } else if (prop.IsDateTime) {
                    tempPropFilterList.Add($"\t\t\tif (filter.{prop.PascalCaseVariableName}.HasValue)" + " {");
                    tempPropFilterList.Add($"\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} == filter.{prop.PascalCaseVariableName}); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t" + "} else {");
                    tempPropFilterList.Add($"\t\t\t\tif (filter.{prop.PascalCaseVariableName}Start.HasValue)" + " {");
                    tempPropFilterList.Add($"\t\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} >= filter.{prop.PascalCaseVariableName}Start.Value); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t\t" + "}");
                    tempPropFilterList.Add($"\t\t\t\tif (filter.{prop.PascalCaseVariableName}End.HasValue)" + " {");
                    tempPropFilterList.Add($"\t\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} <= filter.{prop.PascalCaseVariableName}End.Value); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t\t" + "}");
                    tempPropFilterList.Add($"\t\t\t" + "}");
                } else if (prop.IsJavaScriptNumber) {
                    tempPropFilterList.Add($"\t\t\tif (filter.{prop.PascalCaseVariableName}.HasValue)" + " {");
                    tempPropFilterList.Add($"\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} == filter.{prop.PascalCaseVariableName}); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t" + "}");
                } else if (prop.IsString) {
                    // ok its a string lets add it to the keyword search, the keyword search is always a contains

                    // now the regular search
                    tempPropFilterList.Add($"\t\t\tif (!string.IsNullOrWhiteSpace(filter.{prop.PascalCaseVariableName}))" + " {");
                    // filter exact switch
                    tempPropFilterList.Add($"\t\t\t\tif (filter.{prop.PascalCaseVariableName}FilterExact)" + " {");
                    tempPropFilterList.Add($"\t\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} == filter.{prop.PascalCaseVariableName}); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t\t" + "} else {");
                    tempPropFilterList.Add($"\t\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName}.Contains(filter.{prop.PascalCaseVariableName})); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t\t" + "}");
                    tempPropFilterList.Add($"\t\t\t" + "}");
                } else if (prop.IsGuid && prop.PascalCaseVariableName != "TenantId") {
                    tempPropFilterList.Add($"\t\t\tif (GuidOrEmpty(filter.{prop.PascalCaseVariableName}) != Guid.Empty)" + " {");
                    tempPropFilterList.Add($"\t\t\t\trecs = recs.Where(o => GuidOrEmpty(o.{prop.PascalCaseVariableName}) == GuidOrEmpty(filter.{prop.PascalCaseVariableName})); // {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                    tempPropFilterList.Add($"\t\t\t" + "}");
                } else if (prop.IsBool) {
                    tempPropFilterList.Add($"\t\t\tif (filter.{prop.PascalCaseVariableName}.HasValue)" + " {");

                    /*
                        if (filter.Enabled.Value) {
					        recs = recs.Where(o => o.Enabled == true); // Boolean : Enabled
				        } else {
					        recs = recs.Where(o => o.Enabled == null || !o.Enabled.Value);
				        }
                     */
                    tempPropFilterList.Add($"\t\t\t\tif( filter.{prop.PascalCaseVariableName}" + ".Value ) {");
                    tempPropFilterList.Add($"\t\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} == true);");
                    tempPropFilterList.Add($"\t\t\t\t" + "} else {");
                    tempPropFilterList.Add($"\t\t\t\t\trecs = recs.Where(o => o.{prop.PascalCaseVariableName} == false || o.{prop.PascalCaseVariableName} == null);");
                    tempPropFilterList.Add($"\t\t\t\t" + "}");
                    tempPropFilterList.Add($"\t\t\t" + "}");
                } else {
                    tempPropFilterList.Add($"\t\t\t// TODO: {prop.PascalCaseClassName} : {prop.PascalCaseVariableName}");
                }

            }

            // then filter ont he props
            dataAccessList.AddRange(tempPropFilterList);

            dataAccessList.Add("\t\t}");
            // I want keyword search to be the last thing we do, so its always filtering explicitly before we get to this

            var includeProps = props.Where(o => o.IsOnEFModel && o.IsString && !o.IsCollection && !o.IsDictionary).ToList();
            if (includeProps.Count() > 0) {
                dataAccessList.Add("");
                dataAccessList.Add("\t\tif (!string.IsNullOrWhiteSpace(filter.Keyword)) {");

                dataAccessList.Add($"\t\t\trecs = recs.Where(o =>");

                var i = 0;
                foreach (var prop in includeProps) {
                    var includePropLine = "\t\t\t\t(filter." + prop.PascalCaseVariableName + "IncludeInKeyword && o." + prop.PascalCaseVariableName + ".Contains(string.Empty + filter." + prop.PascalCaseVariableName + "))";
                    if (i < (includeProps.Count() - 1)) {

                        includePropLine = includePropLine + " || ";
                    }
                    dataAccessList.Add(includePropLine);
                    i++;
                }

                dataAccessList.Add("\t\t\t);");
                dataAccessList.Add("\t\t}");
            }

            dataAccessList.Add("");
            dataAccessList.Add("\t\tbool Ascending = true;");
            dataAccessList.Add("\t\tif (StringOrEmpty(output.SortOrder).ToUpper() == \"DESC\")");
            dataAccessList.Add("\t\t{");
            dataAccessList.Add("\t\t    Ascending = false;");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\t");
            dataAccessList.Add("\t\tswitch (StringOrEmpty(output.Sort).ToUpper())");
            dataAccessList.Add("\t\t{");
            foreach (var prop in props.Where(o => o.IsOnEFModel)) {
                if (prop.IsEnum) {
                    dataAccessList.Add($"\t\t\tcase \"{prop.PascalCaseVariableName.ToUpper()}LABELAUTO\":                                                        ");
                }
                dataAccessList.Add($"\t\t\tcase \"{prop.PascalCaseVariableName.ToUpper()}\":                                                        ");
                dataAccessList.Add("\t\t\t\tif (Ascending)");
                dataAccessList.Add("\t\t\t\t{");
                dataAccessList.Add($"\t\t\t\t\trecs = recs.OrderBy(x => x.{prop.PascalCaseVariableName}).ThenBy(x => x.{primaryKeyString});     ");
                dataAccessList.Add("\t\t\t\t}");
                dataAccessList.Add("\t\t\t\telse");
                dataAccessList.Add("\t\t\t\t{");
                dataAccessList.Add($"\t\t\t\t\trecs = recs.OrderByDescending(x => x.{prop.PascalCaseVariableName}).ThenByDescending(x => x.{primaryKeyString});                    ");
                dataAccessList.Add("\t\t\t\t}");
                dataAccessList.Add("\t\t\t\tbreak;");
            }
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\tif (additionalOrderByAuto != null) {");
            dataAccessList.Add("\t\t\trecs = additionalOrderByAuto(recs,StringOrEmpty(output.Sort).ToUpper(),Ascending);");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\tif (recs != null && recs.Count() > 0)");
            dataAccessList.Add("\t\t{");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\tint TotalRecords = recs.Count();");
            dataAccessList.Add("\t\t\toutput.RecordCount = TotalRecords;");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\tif (output.RecordsPerPage > 0)");
            dataAccessList.Add("\t\t\t{");
            dataAccessList.Add("\t\t\t\t// We are filtering records per page");
            dataAccessList.Add("\t\t\t\tif (output.RecordsPerPage >= TotalRecords)");
            dataAccessList.Add("\t\t\t\t\t{");
            dataAccessList.Add("\t\t\t\t\toutput.Page = 1;");
            dataAccessList.Add("\t\t\t\t\toutput.PageCount = 1;");
            dataAccessList.Add("\t\t\t\t}");
            dataAccessList.Add("\t\t\t\telse");
            dataAccessList.Add("\t\t\t\t{");
            dataAccessList.Add("\t\t\t\t\t// Figure out the page count");
            dataAccessList.Add("\t\t\t\t\tif (output.Page < 1) { output.Page = 1; }");
            dataAccessList.Add("\t\t\t\t\tif (output.RecordsPerPage < 1) { output.RecordsPerPage = 25; }");
            dataAccessList.Add("\t\t\t\t\tdecimal decPages = (decimal)TotalRecords / (decimal)output.RecordsPerPage;");
            dataAccessList.Add("\t\t\t\t\tdecPages = Math.Ceiling(decPages);");
            dataAccessList.Add("\t\t\t\t\toutput.PageCount = (int)decPages;");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\t\t\tif (output.Page > output.PageCount)");
            dataAccessList.Add("\t\t\t\t\t{");
            dataAccessList.Add("\t\t\t\t\t\toutput.Page = output.PageCount;");
            dataAccessList.Add("\t\t\t\t\t}");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\t\t\tif (output.Page > 1)");
            dataAccessList.Add("\t\t\t\t\t{");
            dataAccessList.Add("\t\t\t\t\t\trecs = recs.Skip((output.Page - 1) * output.RecordsPerPage).Take(output.RecordsPerPage);");
            dataAccessList.Add("\t\t\t\t\t}");
            dataAccessList.Add("\t\t\t\t\telse");
            dataAccessList.Add("\t\t\t\t\t{");
            dataAccessList.Add("\t\t\t\t\t\trecs = recs.Take(output.RecordsPerPage);");
            dataAccessList.Add("\t\t\t\t\t}");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\t\t}");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t");

            /// paging is done lets actually get it now though

            dataAccessList.Add("\t\t\t// grab the id's now that it is fitered.");
            dataAccessList.Add($"\t\t\tvar ids = await recs.Select(o => o.{primaryKeyString}).ToListAsync();                                                  ");
            dataAccessList.Add("\t\t\t// then use the auto method to read them from the database");
            dataAccessList.Add($"\t\t\tvar autoRecs = await this.Get{classInfo.PascalCaseClassNamePlural}Auto(ids);                                                               ");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\t// loop over the sorted / ordered ids list and build the records output base on that order,");
            dataAccessList.Add("\t\t\t// not the order returned by the auto recs");
            dataAccessList.Add($"\t\t\tList<DataObjects.{classInfo.PascalCaseClassName}> records = new List<DataObjects.{classInfo.PascalCaseClassName}>();                                           ");
            dataAccessList.Add("\t\t\tforeach (var id in ids)");
            dataAccessList.Add("\t\t\t{");
            dataAccessList.Add("\t\t\t    // find the rec out of the autos");
            dataAccessList.Add($"\t\t\t    var autoRec = autoRecs.Single(o => o.{primaryKeyString} == id);                                                    ");
            dataAccessList.Add("\t\t\t    // add any addtional data we need");
            dataAccessList.Add("\t\t\t    if (additionalDataAuto != null) {");
            dataAccessList.Add("\t\t\t\t\tautoRec = additionalDataAuto(autoRec);");
            dataAccessList.Add("\t\t\t    }");
            dataAccessList.Add("\t\t\t    records.Add(autoRec);");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\t\toutput.Records = records.ToArray();");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\toutput.ActionResponse.Result = true;");
            dataAccessList.Add("\t");
            dataAccessList.Add("\t\treturn output;");
            dataAccessList.Add("\t}");
            #endregion

            // save method
            #region
            var saveSingleMethodName = $"Task<DataObjects.{classInfo.PascalCaseClassName}> Save{classInfo.PascalCaseClassName}Auto(DataObjects.{classInfo.PascalCaseClassName} item, bool tryLookup = true, bool trySave = true, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>, DataObjects.{classInfo.PascalCaseClassName}, IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? lookup = null)";
            iDataAccessList.Add("\t" + saveSingleMethodName + ";");

            dataAccessList.Add($"\tpublic async {saveSingleMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");

            dataAccessList.Add($"\t\t{classInfo.PascalCaseClassName}? rec = null;");
            dataAccessList.Add($"\t\titem.ActionResponse = GetNewActionResponse();");

            dataAccessList.Add("\t\tif (tryLookup) {");
            dataAccessList.Add("\t\t\tif (lookup == null) {");
            dataAccessList.Add($"\t\t\t\trec = await data.{classInfo.PascalCaseClassNamePlural}.FirstOrDefaultAsync(o => o.{primaryKeyString} == item.{primaryKeyString});");
            dataAccessList.Add("\t\t\t} else {");
            dataAccessList.Add($"\t\t\t\trec = await lookup(data.{classInfo.PascalCaseClassNamePlural}.AsQueryable(), item).FirstOrDefaultAsync();");
            dataAccessList.Add("\t\t\t}");

            dataAccessList.Add($"\t\t\tif(rec != null)" + " {");
            dataAccessList.Add($"\t\t\t\t item.{primaryKeyString} = rec.{primaryKeyString};");
            dataAccessList.Add($"\t\t\t" + "}");

            dataAccessList.Add("\t\t}");
            dataAccessList.Add($"\t\tvar newRecord = false;");
            dataAccessList.Add("\t\tif (rec == null) {");
            if (primaryKeyType == "Guid") {
                dataAccessList.Add($"\t\t\tif (item.{primaryKeyString} == Guid.Empty)" + "{");
            } else if (primaryKeyType == "Int32") {
                dataAccessList.Add($"\t\t\tif (item.{primaryKeyString} == 0 ) " + "{");
            } else {
                dataAccessList.Add($"\t\tERROR HERE! THIS SHOULDNT HAPPEN, WE FOUND A PRIMARY KEY OF NON INT AND NON GUID");
                dataAccessList.Add($"\t\t\tif (true)" + "{");
            }
            dataAccessList.Add($"\t\t\t\trec = new EFModels.EFModels.{classInfo.PascalCaseClassName}();");

            if (primaryKeyType == "Guid") {
                dataAccessList.Add($"\t\t\t\titem.{primaryKeyString} = Guid.NewGuid();");
                dataAccessList.Add($"\t\t\t\trec.{primaryKeyString} = item.{primaryKeyString};");
            } else if (primaryKeyType == "Int32") {
                dataAccessList.Add($"\t\t\t\t// no need to set a primary key for auto increment int");
            } else {
                dataAccessList.Add($"\t\t\t\tERROR HERE! THIS SHOULDNT HAPPEN, WE FOUND A PRIMARY KEY OF NON INT AND NON GUID");
            }

            dataAccessList.Add("\t\t\t\tnewRecord = true;");
            dataAccessList.Add("\t\t\t} else {");
            dataAccessList.Add($"\t\t\t\titem.ActionResponse.Messages.Add(\"Error Saving {classInfo.PascalCaseClassName} \" + item.{primaryKeyString}.ToString() + \" - Record No Longer Exists\");");
            dataAccessList.Add("\t\t\t\treturn item;");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t\t}");
            // close the if rec not null

            //output.Add("\t\tif (rec != null)");
            //output.Add("\t\t{");
            //
            //output.Add($"\t\t\trec = new EFModels.EFModels.{classInfo.CSharpClassName}();");
            //output.Add(primaryKeyType == "Guid" ? "" : "//" + $"\t\t\titem.{primaryKeyString} = Guid.NewGuid();");
            //output.Add($"\t\t\trec.{primaryKeyString} = item.{primaryKeyString};");
            //output.Add($"\t\t\tnewRecord = true;");
            //
            //
            //output.Add("\t\t}");
            //// start of else
            //output.Add("\t\telse");
            //output.Add("\t\t{");
            //output.Add($"\t\t\titem.ActionResponse.Messages.Add(\"Error Saving \" + item.{primaryKeyString}.ToString() + \" - Record No Longer Exists\");");
            //output.Add("\t\t\treturn item;");
            //// end of else
            //output.Add("\t\t}");

            // ok now we determined if this is new or not, now lets populate all the values

            // first truncate anything that needs to be
            foreach (var prop in props) {
                if (prop.IsString && prop.MaxLength.HasValue && prop.MaxLength > 0) {
                    dataAccessList.Add($"\t\titem.{prop.PascalCaseVariableName} = MaxStringLength(item.{prop.PascalCaseVariableName},{prop.MaxLength});");
                }
            }
            dataAccessList.Add("");
            // now set each property
            foreach (var prop in props) {
                if (prop.IsOnEFModel) {
                    if (prop.IsEnum) {
                        // convert enums to strings
                        dataAccessList.Add($"\t\trec.{prop.PascalCaseVariableName} = Convert{prop.PascalCaseClassName}ToString(item.{prop.PascalCaseVariableName}); //ENUM: base({prop.BaseType.Name}) : actual({prop.ActualType.Name}) : {prop.PascalCaseClassName}");
                    } else if (prop.IsCollection) {
                        // we dont do lists right now...
                        dataAccessList.Add($"\t\t//LIST: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                    } else {
                        // ignore this one
                        if (prop.PascalCaseClassName == "BooleanResponse") {
                            //output.Add($"\t\t//Bool Response - not in the db: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                        } else {
                            // update the rec in the database for each property that actually exists
                            // TODO: check against the actual model stored in the efmodels project
                            if(prop.PascalCaseVariableName == "OriginalId" && (classInfo.PascalCaseClassName == "SampleSinglePageApplication" || classInfo.PascalCaseClassName == "SyncSampleSinglePageApplication")) {
                                dataAccessList.Add($"\t\trec.{prop.PascalCaseVariableName} = string.Empty + item.{prop.PascalCaseVariableName}.ToString(); //OTHER: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                            } else {
                                // don't set the primary key in the autos
                                if(prop.PascalCaseVariableName != primaryKeyString) {
                                    // TODO: figure out how to detect strings on efmodel and settings class on dataobjects, for now just hardcode it
                                    if (prop.ActualType == typeof(DataObjects.UserGroupSettings)) {
                                        // hardcoded for UserGroupSettings
                                        dataAccessList.Add($"\t\trec.{prop.PascalCaseVariableName} = string.Empty + SerializeObject(item.{prop.PascalCaseVariableName}); //OTHER: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                                    } else {
                                        dataAccessList.Add($"\t\trec.{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}; //OTHER: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                                    }
                                }
                            }
                        }
                    }
                } else {
                    dataAccessList.Add($"\t\t//rec.{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName}; //OTHER: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})");
                }
            }

            dataAccessList.Add("");
            dataAccessList.Add("\t\ttry");
            dataAccessList.Add("\t\t{");
            dataAccessList.Add("\t\t\tif (newRecord)");
            dataAccessList.Add("\t\t\t{");
            dataAccessList.Add($"\t\t\t\tdata.{classInfo.PascalCaseClassNamePlural}.Add(rec);");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t\t\tif (trySave) {");
            dataAccessList.Add("\t\t\t\tawait data.SaveChangesAsync();");
            dataAccessList.Add("\t\t\t\t//await SignalRUpdate(new DataObjects.SignalRUpdate");
            dataAccessList.Add("\t\t\t\t//{");
            dataAccessList.Add("\t\t\t\t//\t//TenantId = item.TenantId,");
            dataAccessList.Add($"\t\t\t\t//\tItemId = item.{classInfo.PascalCaseClassName}Id.ToString(),");
            dataAccessList.Add($"\t\t\t\t//\tUpdateType = DataObjects.SignalRUpdateType.{classInfo.PascalCaseClassName},");
            dataAccessList.Add($"\t\t\t\t//\tMessage = \"{classInfo.PascalCaseClassName}Saved\",");
            dataAccessList.Add("\t\t\t\t//\tObject = item");
            dataAccessList.Add("\t\t\t\t//});");

            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t\t\titem.ActionResponse.Result = true;");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\tcatch (Exception ex)");
            dataAccessList.Add("\t\t{");
            dataAccessList.Add($"\t\t\titem.ActionResponse.Messages.Add(\"Error Saving {classInfo.PascalCaseClassName} \" + item.{primaryKeyString}.ToString() + \" - \" + ex.Message);");
            dataAccessList.Add("\t\t}");

            dataAccessList.Add("\t\treturn item;");
            // close the function method
            dataAccessList.Add("\t}");
            #endregion

            // save multiple
            #region
            var saveManyMethodName = $"Task<List<DataObjects.{classInfo.PascalCaseClassName}>> Save{classInfo.PascalCaseClassNamePlural}Auto(List<DataObjects.{classInfo.PascalCaseClassName}> items, bool tryLookup = true, Func<IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>, DataObjects.{classInfo.PascalCaseClassName}, IQueryable<SampleSinglePageApplication.EFModels.EFModels.{classInfo.PascalCaseClassName}>>? lookup = null)";
            iDataAccessList.Add("\t" + saveManyMethodName + ";");

            dataAccessList.Add($"\tpublic async {saveManyMethodName}  //" + classType.ToString());
            dataAccessList.Add("\t{");
            dataAccessList.Add($"\t\tList<DataObjects.{classInfo.PascalCaseClassName}> output = new List<DataObjects.{classInfo.PascalCaseClassName}>();");
            dataAccessList.Add("\t\tforeach(var item in items) {");
            dataAccessList.Add($"\t\t\tDataObjects.{classInfo.PascalCaseClassName} saved = await Save{classInfo.PascalCaseClassName}Auto(item, tryLookup, false, lookup);");
            dataAccessList.Add("\t\t\tif (saved != null && saved.ActionResponse != null && saved.ActionResponse.Result) {");
            dataAccessList.Add($"\t\t\t\toutput.Add(saved);");
            dataAccessList.Add("\t\t\t}");
            dataAccessList.Add("\t\t}");
            dataAccessList.Add("\t\tawait data.SaveChangesAsync();");
            dataAccessList.Add("\t\treturn output;");
            dataAccessList.Add("\t}");
            dataAccessList.Add("");
            #endregion

            if (classInfo.IsEfModel && !classInfo.IsEnum) {

                // Get Icon
                #region
                var getIconMethodName = $"Dictionary<string, string> Get{classInfo.PascalCaseClassName}IconAuto()";

                // language and icon
                //iconList.Add("\t\toutput.Add(\"AddNew" + classInfo.PascalCaseClassName + "\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Clear" + classInfo.PascalCaseClassName + "\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassName + "\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassNamePlural + "\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Delete" + classInfo.PascalCaseClassName + "Filter\", \"\");");
                //iconList.Add("\t\toutput.Add(\"CancelDelete" + classInfo.PascalCaseClassName + "Filter\", \"\");");
                //iconList.Add("\t\toutput.Add(\"ConfirmDelete" + classInfo.PascalCaseClassName + "Filter'\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Show" + classInfo.PascalCaseClassName + "Filter\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Hide" + classInfo.PascalCaseClassName + "Filter\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Refresh" + classInfo.PascalCaseClassName + "\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassName + "FilterListView\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassName + "FilterCardView\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Save" + classInfo.PascalCaseClassName + "Filter\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassName + "FilterShowingDetails\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassName + "FilterHidingDetails\", \"\");");
                //iconList.Add("\t\toutput.Add(\"" + classInfo.PascalCaseClassName + "FilterExport\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Edit" + classInfo.PascalCaseClassName + "\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Edit" + classInfo.PascalCaseClassName + "TableButton\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Back" + classInfo.PascalCaseClassName + "'\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Save" + classInfo.PascalCaseClassName + "'\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Delete" + classInfo.PascalCaseClassName + "'\", \"\");");
                //iconList.Add("\t\toutput.Add(\"Cancel" + classInfo.PascalCaseClassName + "'\", \"\");");
                //iconList.Add("\t\toutput.Add(\"ConfirmDelete" + classInfo.PascalCaseClassName + "'\", \"\");");

                // icon only

                iDataAccessList.Add("\t" + getIconMethodName + ";");

                dataAccessList.Add("\tpublic " + getIconMethodName + "");
                dataAccessList.Add("\t{");
                dataAccessList.Add("\t\tvar output = new Dictionary<string, string>();");
                dataAccessList.AddRange(iconList);
                dataAccessList.Add("\t\treturn output;");
                dataAccessList.Add("\t}");
                iDataAccessList.Add("");
                #endregion
                // Get Language
                #region
                var getLanguageMethodName = $"Dictionary<string, string> Get{classInfo.PascalCaseClassName}LanguageAuto()";

                iDataAccessList.Add("\t" + getLanguageMethodName + ";");
                dataAccessList.Add("\tpublic " + getLanguageMethodName + "");
                dataAccessList.Add("\t{");
                dataAccessList.Add("\t\tvar output = new Dictionary<string, string>();");

                // language and icon
                var dict = classInfo.Language.ToList();
                foreach (var item in dict) {
                    languageList.Add("\t\toutput.Add(\"" + item.Key + "\", \"" + item.Value + "\");");
                }

                // prop specific languages...


                foreach (var prop in props.Where(o => o.IsOnEFModel)) {
                    if (prop.IsDateTime) {
                        languageList.Add("\t\toutput.Add(\"" + prop.Language_VariableNameStart + "\", \"" + prop.Language_VariableNameStart_Value + "\");");
                        languageList.Add("\t\toutput.Add(\"" + prop.Language_VariableNameEnd + "\", \"" + prop.Language_VariableNameEnd_Value + "\");");
                    } else {
                        languageList.Add("\t\toutput.Add(\"" + prop.Language_VariableName + "\",\"" + prop.Language_VariableName_Value + "\");");
                    }

                    if (prop.IsString) {
                        // ook, strings can be filtered on with the keyword search so they need some extra stuff
                        languageList.Add("\t\toutput.Add(\"" + prop.Language_VariableNameFilterExact + "\",\"" + prop.Language_VariableNameFilterExact_Value + "\");");
                        languageList.Add("\t\toutput.Add(\"" + prop.Language_VariableNameIncludeInKeyword + "\",\"" + prop.Language_VariableNameIncludeInKeyword_Value + "\");");
                    } else if (prop.IsJavaScriptNumber) {
                        // todo: numbers aren't included in the keyword search

                    }
                }

                dataAccessList.AddRange(languageList);
                dataAccessList.Add("\t\treturn output;");
                dataAccessList.Add("\t}");
                #endregion
            }
            iDataAccessList.Add("}");

            dataAccessList.Add("");
            dataAccessList.Add("}");
            output.AddRange(iDataAccessList);
            output.AddRange(dataAccessList);

            return output;
        }

        public static List<string> TranscribeDataControllers(Type classType, ReflectedClassInfo classInfo, List<ReflectedClassInfo> props)
        {
            if (!classInfo.IsEfModel) {
                return new List<string>();
            }
            var output = new List<string>();

            var primaryKey = props.FirstOrDefault(o => o.IsPrimaryKey);

            var cSharpPrimaryKeyString = classInfo.PascalCaseClassName + "Id";
            var camelCasePrimaryKeyString = classInfo.CamelCaseClassName + "Id";
            var primaryKeyType = "Guid";
            if (primaryKey != null) {
                cSharpPrimaryKeyString = primaryKey.PascalCaseVariableName;
                primaryKeyType = primaryKey.PascalCaseClassName;
            }

            // hokay, i'm making a filter like the user filter

            output.Add("using Microsoft.AspNetCore.Mvc;");
            output.Add("using SampleSinglePageApplication.EFModels.EFModels;");
            output.Add("");
            output.Add("namespace SampleSinglePageApplication.Web.Controllers");
            output.Add("{");
            output.Add("\tpublic partial class DataController : ControllerBase");
            output.Add("\t{");
            output.Add("");
            output.Add("\t\t[HttpGet]");
            output.Add("\t\t[Route(\"~/api/Data/Get" + classInfo.PascalCaseClassName + "Auto/{" + camelCasePrimaryKeyString + "}\")]");
            output.Add("\t\tpublic async Task<ActionResult<DataObjects." + classInfo.PascalCaseClassName + ">> Get" + classInfo.PascalCaseClassName + "Auto(" + primaryKeyType + " " + camelCasePrimaryKeyString + ")");
            output.Add("\t\t{");
            output.Add("\t\t\tDataObjects." + classInfo.PascalCaseClassName + "? output = null;");
            output.Add("");
            output.Add("\t\t\tif (CurrentUser.Admin) {");
            output.Add("\t\t\t\toutput = await da.Get" + classInfo.PascalCaseClassName + "Auto(" + camelCasePrimaryKeyString + ");");
            if(props.Any(o => o.PascalCaseVariableName == "TenantId")) {
                output.Add("\t\t\t\tif (output != null && output.TenantId == CurrentUser.TenantId) {");
                output.Add("\t\t\t\t    return Ok(output);");
                output.Add("\t\t\t\t} else {");
                output.Add("\t\t\t\t    return Unauthorized(\"Access Denied\");");
                output.Add("\t\t\t\t}");
            } else {
                output.Add("\t\t\t\treturn Ok(output);");

            }
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\treturn Unauthorized(\"Access Denied\");");
            output.Add("\t\t\t}");
            output.Add("\t\t}");
            output.Add("");
            output.Add("\t\t[HttpPost]");
            output.Add("\t\t[Route(\"~/api/Data/Get" + classInfo.PascalCaseClassNamePlural + "FilteredAuto/\")]");
            output.Add("\t\tpublic async Task<ActionResult<DataObjects.Filter" + classInfo.PascalCaseClassNamePlural + "Auto>> Get" + classInfo.PascalCaseClassNamePlural + "FilteredAuto(DataObjects.Filter" + classInfo.PascalCaseClassNamePlural + "Auto filter)");
            output.Add("\t\t{");
            output.Add("\t\t\tif (CurrentUser.Admin) {");
            if (props.Any(o => o.PascalCaseVariableName == "TenantId")) {
                output.Add("\t\t\t\tfilter.TenantId = CurrentUser.TenantId;");
            }
            output.Add("\t\t\t\tvar output = await da.Get" + classInfo.PascalCaseClassNamePlural + "FilteredAuto(filter);");
            output.Add("\t\t\t\treturn Ok(output);");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\treturn Unauthorized(\"Access Denied\");");
            output.Add("\t\t\t}");
            output.Add("\t\t}");
            output.Add("");
            output.Add("\t\t[HttpGet]");
            output.Add("\t\t[Route(\"~/api/Data/Delete" + classInfo.PascalCaseClassName + "Auto/{" + camelCasePrimaryKeyString + "}\")]");
            output.Add("\t\tpublic async Task<ActionResult<bool>> Delete" + classInfo.PascalCaseClassName + "Auto(" + primaryKeyType + " " + camelCasePrimaryKeyString + ")");
            output.Add("\t\t{");
            output.Add("\t\t\tif (CurrentUser.Admin) {");
            if (props.Any(o => o.PascalCaseVariableName == "TenantId")) {
                output.Add("\t\t\t\tvar existing = await da.Get" + classInfo.PascalCaseClassName + "Auto(" + camelCasePrimaryKeyString + ");");
                output.Add("\t\t\t\tif (existing != null && existing.TenantId == CurrentUser.TenantId) {");
                output.Add("\t\t\t\t\tvar output = await da.Delete" + classInfo.PascalCaseClassName + "Auto(" + camelCasePrimaryKeyString + ");");
                output.Add("\t\t\t\t\treturn Ok(output);");
                output.Add("\t\t\t} else {");
                output.Add("\t\t\t\t\treturn Unauthorized(\"Access Denied\");");
                output.Add("\t\t\t}");
            } else {
                output.Add("\t\t\tvar output = await da.Delete" + classInfo.PascalCaseClassName + "Auto(" + camelCasePrimaryKeyString + ");");
                output.Add("\t\t\t\treturn Ok(output);");
            }

            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\treturn Unauthorized(\"Access Denied\");");
            output.Add("\t\t\t}");
            output.Add("\t\t}");
            output.Add("");
            output.Add("\t\t[HttpPost]");
            output.Add("\t\t[Route(\"~/api/Data/Save" + classInfo.PascalCaseClassName + "Auto/\")]");
            output.Add("\t\tpublic async Task<ActionResult<DataObjects." + classInfo.PascalCaseClassName + ">> Save" + classInfo.PascalCaseClassName + "Auto(DataObjects." + classInfo.PascalCaseClassName + " " + classInfo.CamelCaseClassName + ")");
            output.Add("\t\t{");
            output.Add("            DataObjects." + classInfo.PascalCaseClassName + " output = new DataObjects." + classInfo.PascalCaseClassName + "();");
            output.Add("");
            if (props.Any(o => o.PascalCaseVariableName == "TenantId")) {
                output.Add("            " + classInfo.CamelCaseClassName + ".TenantId = CurrentUser.TenantId;");
            }
            output.Add("            if (CurrentUser.Admin) {");
            output.Add("\t\t\t\toutput = await da.Save" + classInfo.PascalCaseClassName + "Auto(" + classInfo.CamelCaseClassName + ");");
            output.Add("\t\t\t\treturn Ok(output);");
            output.Add("            } else {");
            output.Add("\t\t\t\treturn Unauthorized(\"Access Denied\");");
            output.Add("            }");
            output.Add("\t\t}");
            output.Add("    }");
            output.Add("}");

            return output;
        }

        public static List<string> TranscribeDataObjects(Type classType, ReflectedClassInfo classInfo, List<ReflectedClassInfo> props)
        {
            if (!classInfo.IsEfModel) {
                return new List<string>();
            }
            var output = new List<string>();

            // hokay, i'm making a filter like the user filter

            output.Add($"\tpublic class Filter{classInfo.PascalCaseClassNamePlural}Auto : Filter");
            output.Add("\t{");
            foreach (var prop in props.Where(o => o.IsOnEFModel && !o.IsDictionary && !o.IsCollection && o.PascalCaseVariableName != "TenantId")) {
                //output.Add($"\tpublic List<Guid>? FilterDepartments " + "{ get; set; }");
                if (prop.IsEnum) {
                    output.Add($"\t\tpublic List<{prop.PascalCaseClassName}>? {prop.PascalCaseVariableNamePlural}" + " { get; set; }" + $" = new List<{prop.PascalCaseClassName}>();");
                } else if (prop.ActualType.Name == "DateTime") {
                    output.Add("\t\t[JsonConverter(typeof(UTCDateTimeConverter))]");
                    output.Add($"\t\tpublic {prop.PascalCaseClassName}? {prop.PascalCaseVariableName}Start" + " { get; set; }" + $"");
                    output.Add("\t\t[JsonConverter(typeof(UTCDateTimeConverter))]");
                    output.Add($"\t\tpublic {prop.PascalCaseClassName}? {prop.PascalCaseVariableName}End" + " { get; set; }" + $"");
                    output.Add("\t\t[JsonConverter(typeof(UTCDateTimeConverter))]");
                    output.Add($"\t\tpublic {prop.PascalCaseClassName}? {prop.PascalCaseVariableName}" + " { get; set; }              ");
                } else if (prop.IsString) {
                    output.Add($"\t\tpublic {prop.PascalCaseClassName}? {prop.PascalCaseVariableName}" + " { get; set; }              ");
                    output.Add($"\t\tpublic bool {prop.PascalCaseVariableName}FilterExact" + " { get; set; }              ");
                    output.Add($"\t\tpublic bool {prop.PascalCaseVariableName}IncludeInKeyword" + " { get; set; }              ");
                } else {
                    output.Add($"\t\tpublic {prop.PascalCaseClassName}? {prop.PascalCaseVariableName}" + " { get; set; }              ");
                }
            }
            foreach (var prop in props.Where(o => !o.IsOnEFModel && o.PascalCaseClassName != "BooleanResponse")) {
                output.Add($"\t\t// IS NOT ON EF MODEL //public {prop.PascalCaseClassName}? {prop.PascalCaseVariableName}" + " { get; set; }              ");
            }
            output.Add("\t}");

            output.Add("");

            output.Add($"\tpublic class SavedFilter{classInfo.PascalCaseClassNamePlural}Auto : ActionResponseObject");
            output.Add("\t{");
            output.Add("\t\tpublic string? Description { get; set; }");
            output.Add("\t\tpublic Filter" + classInfo.PascalCaseClassNamePlural + "Auto? Filter { get; set; }");
            output.Add("\t\tpublic Guid SavedFilterId { get; set; }");
            output.Add("\t\tpublic Guid TenantId { get; set; }");
            output.Add("\t\tpublic Guid UserId { get; set; }");
            output.Add("\t}");

            return output;
        }

        public static (List<string> KnockoutResults, List<string> DtoResults) TranscribeKnockoutAndDtos(SampleSinglePageApplication.EFModels.EFModels.EFDataModel _data, Type classType, ReflectedClassInfo classInfo, List<ReflectedClassInfo> props)
        {
            var dtoPropList = new List<string>();
            var knockoutLoadList = new List<string>();
            var knockoutPropList = new List<string>();
            var knockoutLoadDefaultList = new List<string>();

            var isActionResponse = false;
            foreach (var prop in props) {
                var isKnownClassArray = false;
                var knownClassArrayLoad = new List<string>();
                // in order to write out the result we need to determine its type and its camelCase variable name.
                // Getting the name is easy, so just do it.
                //var name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
                // next we need to determine the type, if we can't figure it out just default it to any.
                var type = "any";

                // First Switch on the Property Type.  Most basic types have been defined (System.Int, System.String, etc)
                string defaultValue = "";

                // if its a collection get the inner type, otherwise get the base type
                var actualType = prop.ActualType;
                var actualTypeName = "" + actualType != null ? actualType!.Name : "";

                if (prop.IsString || prop.IsGuid) {
                    defaultValue = "null";
                    type = "string";
                } else if (prop.IsJavaScriptNumber) {
                    defaultValue = "0";
                    type = "number";
                } else if (prop.IsBool) {
                    defaultValue = "false";
                    type = "boolean";
                } else if (prop.IsJavaScriptDate) {
                    defaultValue = "new Date";
                    type = "Date";
                } else if (prop.IsObject) {
                    defaultValue = "null";
                    type = "any";
                } else if (prop.IsByte) {
                    type = "any";
                } else {
                    // HOKAY, its not a default type
                    type = prop.CamelCaseClassName;
                    // but what if it is an enum...
                    if (prop.IsEnum) {
                        defaultValue = "0";
                    } else {
                        defaultValue = "new " + prop.CamelCaseClassName;
                    }
                }

                if (prop.IsNullable && !prop.IsCollection) {
                    defaultValue = "null";
                }

                string? knockoutVersion;
                string? dtoVersion;
                if (prop.IsCollection) {
                    defaultValue = "[]";
                    if (prop.IsEnum) {
                        // it is an array, but is it an enum too? It needs the server. reference since there is no knockout version
                        dtoVersion = $"{prop.CamelCaseVariableName}: server.{prop.CamelCaseClassName}[]; //" + $"{prop.InnerType}: {prop.InnerType.Name}";
                        knockoutVersion = $"{prop.CamelCaseVariableName}: KnockoutObservableArray<server.{prop.CamelCaseClassName}> = ko.observableArray({defaultValue}); //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                    } else {
                        // its an array of known classes that has the default load mechanic
                        if (DefaultList(_data).Contains(prop.InnerType)) {
                            dtoVersion = $"{prop.CamelCaseVariableName}: {prop.CamelCaseClassName}[]; //" + $"{prop.InnerType}: {prop.InnerType.Name}";
                            knockoutVersion = $"{prop.CamelCaseVariableName}: KnockoutObservableArray<{prop.CamelCaseClassName}> = ko.observableArray({defaultValue}); //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";

                            isKnownClassArray = true;

                            knownClassArrayLoad.Add($"let __{prop.CamelCaseVariableName}: {prop.CamelCaseClassName}[] = [];");
                            knownClassArrayLoad.Add($"if (data.{prop.CamelCaseVariableName} != null) " + "{");
                            knownClassArrayLoad.Add($"\tdata.{prop.CamelCaseVariableName}.forEach((e: server.{prop.CamelCaseClassName}) =>" + " {");
                            knownClassArrayLoad.Add($"\t\tlet item: {prop.CamelCaseClassName} = new {prop.CamelCaseClassName}();");
                            knownClassArrayLoad.Add("\t\titem.Load(e);");
                            knownClassArrayLoad.Add($"\t\t__{prop.CamelCaseVariableName}.push(item);");
                            knownClassArrayLoad.Add("\t});");
                            knownClassArrayLoad.Add("}");
                            knownClassArrayLoad.Add($"this.{prop.CamelCaseVariableName}(__{prop.CamelCaseVariableName});");
                        } else {
                            // anything else we don't really know so just make it an any
                            dtoVersion = $"{prop.CamelCaseVariableName}: {type}[]; //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                            knockoutVersion = $"{prop.CamelCaseVariableName}: KnockoutObservableArray<{type}> = ko.observableArray({defaultValue}); //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                        }
                    }
                } else if (!prop.IsCollection && prop.IsEnum) {
                    // its not an array, is it an enum though
                    dtoVersion = $"{prop.CamelCaseVariableName}: server.{type}; //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                    knockoutVersion = $"{prop.CamelCaseVariableName}: KnockoutObservable<server.{type}> = ko.observable(0);";

                } else {
                    // hardcoded fix for an infinite loop, dont let assets
                    if (prop.CamelCaseClassName == classInfo.CamelCaseClassName) {
                        defaultValue = "null";
                    }
                    // OK , so we aren't an array, and we aren't an enum
                    dtoVersion = $"{prop.CamelCaseVariableName}: {type}; //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                    if (prop.ActualType == typeof(System.Boolean) && classInfo.CamelCaseClassName.ToLower().Contains("auto") && classInfo.CamelCaseClassName.ToLower().Contains("filter") && !prop.CamelCaseVariableName.ToLower().Contains("includeinkeyword") && !prop.CamelCaseVariableName.ToLower().Contains("filterexact") && !prop.CamelCaseVariableName.ToLower().Equals("showfilters")) {
                        knockoutVersion = $"{prop.CamelCaseVariableName}: KnockoutObservable<string> = ko.observable(\"\"); //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                    } else {
                        knockoutVersion = $"{prop.CamelCaseVariableName}: KnockoutObservable<{type}> = ko.observable({defaultValue}); //" + $"{prop.BaseType}: {prop.PascalCaseVariableName}";
                    }
                }

                // First do stuff for knockout
                //if (knockout) {
                // hardcoded check for the bool response extended class object
                if (prop.BaseType.ToString() != "SampleSinglePageApplication.DataObjects+BooleanResponse") {
                    if (isKnownClassArray) {
                        foreach (var item in knownClassArrayLoad) {
                            knockoutLoadList.Add($"\t\t\t{item}");
                        }
                    } else {
                        if (prop.PascalCaseVariableName == "Records" && prop.PascalCaseClassName == "Object" && classInfo.PascalCaseClassName.Contains("Auto")) {

                            var noFilter = string.Join("", (classInfo.PascalCaseClassName.Skip("Filter".Length).ToArray()));
                            var recordCSharpClassName = string.Join("", (noFilter.Take(noFilter.Length - "sAuto".Length)));
                            var recordCamelCaseClassName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(recordCSharpClassName);
                            //var recordProp = props.Where(o => o.CSharpClassName == recordCSharpClassName).Single();

                            knockoutLoadList.Add("\t\t\tlet __records: " + recordCamelCaseClassName + "[] = [];");
                            knockoutLoadList.Add("\t\t\tif (data.records != null) {");
                            knockoutLoadList.Add("\t\t\t\tdata.records.forEach((e: server." + recordCamelCaseClassName + ") => {");
                            knockoutLoadList.Add("\t\t\t\t\tlet item: " + recordCamelCaseClassName + " = new " + recordCamelCaseClassName + "();");
                            knockoutLoadList.Add("\t\t\t\t\titem.Load(e);");
                            knockoutLoadList.Add("\t\t\t\t\t__records.push(item);");
                            knockoutLoadList.Add("\t\t\t\t});");
                            knockoutLoadList.Add("\t\t\t}");
                            knockoutLoadList.Add("\t\t\tthis.records(__records);");
                        } else if ((DefaultList(_data).Contains(prop.BaseType) && !prop.IsEnum)) {
                            knockoutLoadList.Add($"\t\t\tthis.{prop.CamelCaseVariableName}().Load(data.{prop.CamelCaseVariableName});");

                        } else {
                            // so if its an enum just load it, but what if i wana make a label auto
                            // .... TODO: convert c# bool null, True, False to javascript
                            if (prop.ActualType == typeof(System.Boolean) && classInfo.CamelCaseClassName.ToLower().Contains("auto") && classInfo.CamelCaseClassName.ToLower().Contains("filter") && !prop.CamelCaseVariableName.ToLower().Contains("includeinkeyword") && !prop.CamelCaseVariableName.ToLower().Contains("filterexact") && !prop.CamelCaseVariableName.ToLower().Equals("showfilters")) {
                               //knockoutLoadList.Add($"\t\t\tvar jsValue{prop.CamelCaseVariableName} = null;");
                               //knockoutLoadList.Add($"\t\t\tif( ('' + data.{prop.CamelCaseVariableName}).toLowerCase() == 'true' )");
                               //knockoutLoadList.Add( "\t\t\t{");
                               //knockoutLoadList.Add($"\t\t\t\tjsValue{prop.CamelCaseVariableName} = true");
                               //knockoutLoadList.Add( "\t\t\t}");
                               //knockoutLoadList.Add($"\t\t\telse if( ('' + data.{prop.CamelCaseVariableName}).toLowerCase() == 'true' )");
                               //knockoutLoadList.Add("\t\t\t{");
                               //knockoutLoadList.Add($"\t\t\t\tjsValue{prop.CamelCaseVariableName} = false");
                               //knockoutLoadList.Add("\t\t\t}");

                                knockoutLoadList.Add($"\t\t\tthis.{prop.CamelCaseVariableName}(autoUtility.ConvertBooleanToString(data.{prop.CamelCaseVariableName}));");
                            } else {
                                knockoutLoadList.Add($"\t\t\tthis.{prop.CamelCaseVariableName}(data.{prop.CamelCaseVariableName});");
                            }
                        }
                    }
                    // ok, so this is a problem if the 
                    knockoutLoadDefaultList.Add($"\t\t\tthis.{prop.CamelCaseVariableName}({defaultValue});");
                } else {
                    if (classInfo.PascalCaseClassName != "ActionResponseObject") {
                        //loadList.Add("\t\t\tsuper.Load(data);");
                        //loadDefaultList.Add($"\t\t\tsuper.Load(data);");
                    } else {
                        knockoutLoadList.Add("\t\t\tthis.actionResponse().Load(data.actionResponse);");
                        knockoutLoadDefaultList.Add($"\t\t\tthis.actionResponse(new booleanResponse);");
                    }
                }
                //}

                if (prop.CamelCaseVariableName == "actionResponse" && classInfo.CamelCaseClassName != "actionResponseObject") {
                    isActionResponse = true;
                }

                //if (knockout) {
                if (!isActionResponse) {
                    knockoutPropList.Add($"\t{knockoutVersion}");
                    if (!prop.IsCollection && prop.IsEnum) {
                        knockoutPropList.Add("\t" + prop.CamelCaseVariableName + "LabelAuto = ko.computed((): string => {");
                        knockoutPropList.Add("\t\tlet output: string = null;");
                        knockoutPropList.Add("");
                        knockoutPropList.Add("\t\tif (this." + prop.CamelCaseVariableName + "() != null) {");
                        knockoutPropList.Add("\t\t\toutput = sampleSinglePageApplicationEnumAutos.Convert" + prop.PascalCaseClassName + "ToHumanReadableString(this." + prop.CamelCaseVariableName + "());");
                        knockoutPropList.Add("\t\t}");
                        knockoutPropList.Add("");
                        knockoutPropList.Add("\t\treturn output;");
                        knockoutPropList.Add("\t});");
                    }
                }

                //} else {
                dtoPropList.Add($"\t{(isActionResponse ? "//" : "")}{dtoVersion}");
                //}
            }

            var knockoutInterfaceTitle =
                               ($"class {classInfo.CamelCaseClassName}" + (isActionResponse ? " extends actionResponseObject" : "") + " { // " + classType.ToString());
            var KnockoutTypeScriptResults = new List<string>();
            KnockoutTypeScriptResults.Add(knockoutInterfaceTitle);
            KnockoutTypeScriptResults.AddRange(knockoutPropList);
            KnockoutTypeScriptResults.Add("");
            KnockoutTypeScriptResults.Add($"\tLoad(data: server.{classInfo.CamelCaseClassName})" + " {");
            if (isActionResponse) {
                KnockoutTypeScriptResults.Add($"\t\tsuper.Load(data);");
            }
            KnockoutTypeScriptResults.Add("\t\tif (data != null) {");

            knockoutLoadList.Add("\t\t}"); // end if
            knockoutLoadList.Add("\t\telse {");
            knockoutLoadList.AddRange(knockoutLoadDefaultList);
            knockoutLoadList.Add("\t\t}"); // end else
            knockoutLoadList.Add("\t}"); // end load

            KnockoutTypeScriptResults.AddRange(knockoutLoadList);
            KnockoutTypeScriptResults.Add("}");

            var dtoInterfaceTitle =
               ($"interface {classInfo.CamelCaseClassName}" + (isActionResponse ? " extends actionResponseObject" : "") + " { // " + classType.ToString());

            var DtoTypeScriptResults = new List<string>();
            DtoTypeScriptResults.Add(dtoInterfaceTitle);
            DtoTypeScriptResults.AddRange(dtoPropList);
            DtoTypeScriptResults.Add("}");

            return (KnockoutTypeScriptResults, DtoTypeScriptResults);
        }

        public static List<string> TranscribePartialViews(Type classType, ReflectedClassInfo classInfo, List<ReflectedClassInfo> props)
        {
            if (!classInfo.IsEfModel) {
                return new List<string>();
            }

            var output = new List<string>();

            var primaryKey = props.FirstOrDefault(o => o.IsPrimaryKey);

            var cSharpPrimaryKeyString = classInfo.PascalCaseClassName + "Id";
            var camelCasePrimaryKeyString = classInfo.CamelCaseClassName + "Id";
            var primaryKeyType = "Guid";
            if (primaryKey != null) {
                cSharpPrimaryKeyString = primaryKey.PascalCaseVariableName;
                camelCasePrimaryKeyString = primaryKey.CamelCaseVariableName;
                primaryKeyType = primaryKey.PascalCaseClassName;
            }

            output.Add($"<script src=\"~/js/viewModels/autos/{classInfo.CamelCaseClassName}Autos.js?v=@Html.Raw(Guid.NewGuid().ToString().Replace(\"-\", \"\"))\"></script>");
            output.Add($"<div id=\"view-{classInfo.LowerCaseClassNamePlural}-auto\">");

            output.Add("\t<div data-bind=\"visible:MainModel().CurrentView() == '" + classInfo.LowerCaseClassNamePlural + "auto'\">");
            output.Add("\t\t<div data-bind=\"css:{fixed: MainModel().StickyMenus() == true}\">");
            output.Add("\t\t\t<i class=\"sticky-menu-icon\" data-bind=\"html:MainModel().StickyIcon(), click:function() {MainModel().ToggleStickyMenus()}\"></i>");
            output.Add("\t\t\t<h1 class=\"display-7\"><i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNamePlural + "')\"></i></h1>");

            output.Add("\t\t\t<div class=\"padbottom-5\">");
            output.Add("\t\t\t\t<div class=\"btn-group\" role=\"group\">");

            // normal things get the add
            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-success\" data-bind=\"click:function() { MainModel().Nav('new" + classInfo.LowerCaseClassName + "auto') }\">");
            output.Add("\t\t\t\t\t\t<i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameAddNew + "')\"></i>");
            output.Add("\t\t\t\t\t</button>");
            //else {
            //}
            output.Add("\t\t\t\t\t");
            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-primary\" data-bind=\"click:Clear" + classInfo.PascalCaseClassName + "Filter\">");
            output.Add("\t\t\t\t\t\t<i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameClear + "')\"></i>");
            output.Add("\t\t\t\t\t</button>");
            output.Add("\t\t\t\t\t");
            //output.Add("\t\t\t\t\t<!-- ko if: Filter().filterId() != null && Filter().filterId() > 0 -->");
            //output.Add("\t\t\t\t\t    <!-- ko if: ConfirmDelete() != Filter().filterId().toString() -->");
            //output.Add("\t\t\t\t\t        <button type=\"button\" class=\"btn btn-danger\" data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameDeleteFilter + "'), click:function(){ConfirmDelete" + classInfo.PascalCaseClassName + "Filter(Filter().filterId().toString())}\"></button>");
            //output.Add("\t\t\t\t\t    <!-- /ko -->");
            //output.Add("\t\t\t\t\t    <!-- ko if: ConfirmDelete() == Filter().filterId().toString() -->");
            //output.Add("\t\t\t\t\t        <button type=\"button\" class=\"btn btn-dark\" data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameCancelDeleteFilter + "'), click:function(){ConfirmDelete" + classInfo.PascalCaseClassName + "Filter('')}\"></button>");
            //output.Add("\t\t\t\t\t        <button type=\"button\" class=\"btn btn-danger\" data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameConfirmDeleteFilter + "'), click:DeleteSaved" + classInfo.PascalCaseClassName + "Filter\"></button>");
            //output.Add("\t\t\t\t\t    <!-- /ko -->");
            //output.Add("\t\t\t\t\t<!-- /ko-->");
            //output.Add("\t\t\t\t\t");

            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"visible:Filter().showFilters() == false, click:ToggleShow" + classInfo.PascalCaseClassName + "Filter, enable:Loading() == false\">");
            output.Add("\t\t\t\t\t\t<i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameShowFilter + "')\"></i>");
            output.Add("\t\t\t\t\t</button>");
            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"visible:Filter().showFilters() == true, click:ToggleShow" + classInfo.PascalCaseClassName + "Filter, enable:Loading()  == false\">");
            output.Add("\t\t\t\t\t\t<i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameHideFilter + "')\"></i>");
            output.Add("\t\t\t\t\t</button>");
            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"click:Refresh" + classInfo.PascalCaseClassName + "Filter, enable:Loading()  == false\">");
            output.Add("\t\t\t\t\t\t<i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameRefresh + "')\"></i>");
            output.Add("\t\t\t\t\t</button>");


            // TODO: filter list vs card view

            //output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"visible:FilterView" + classInfo.PascalCaseClassName + "Type == 'list', click:Toggle" + classInfo.PascalCaseClassName + "View, enable:Loading() == false\">");
            //output.Add("\t\t\t\t\t <i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameFilterListView + "')\"></i>");
            //output.Add("\t\t\t\t\t</button>");
            //output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"visible:FilterView" + classInfo.PascalCaseClassName + "Type == 'card', click:Toggle" + classInfo.PascalCaseClassName + "View, enable:Loading() == false\">");
            //output.Add("\t\t\t\t\t <i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameFilterCardView + "')\"></i>");
            //output.Add("\t\t\t\t\t</button>");
            //output.Add("\t\t\t\t\t");


            //output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-info\" data-bind=\"click:Save" + classInfo.PascalCaseClassName + "Filter, enable:Loading() == false\">");
            //output.Add("\t\t\t\t\t    <i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameSaveFilter + "')\"></i>");
            //output.Add("\t\t\t\t\t</button>");


            // TODO: add back details list switch
            //output.Add("\t\t\t\t\t");
            //output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"click:Toggle" + classInfo.PascalCaseClassName + "Details, enable:Loading() == false\">");
            //output.Add("\t\t\t\t\t <!-- ko if: Show" + classInfo.PascalCaseClassName + "Details() == true -->");
            //output.Add("\t\t\t\t\t <i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameFilterShowingDetails + "')\"></i>");
            //output.Add("\t\t\t\t\t <!-- /ko -->");
            //output.Add("\t\t\t\t\t <!-- ko if: Show" + classInfo.PascalCaseClassName + "Details() == false -->");
            //output.Add("\t\t\t\t\t <i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameFilterHidingDetails + "')\"></i>");
            //output.Add("\t\t\t\t\t <!-- /ko -->");
            //output.Add("\t\t\t\t\t</button>");

            //output.Add("\t\t\t\t\t<button id=\"btn-saved-" + classInfo.LowerCaseClassName + "filters\" type=\"button\" class=\"btn btn-dark dropdown-toggle\" data-bs-toggle=\"dropdown\" aria-expanded=\"false\" data-bind=\"html:MainModel().Language('" + classInfo.Language_ClassNameSavedFilters + "'), enable:Loading() == false\"></button>");
            //output.Add("\t\t\t\t\t<ul class=\"dropdown-menu\" aria-labelledby=\"btn-saved-" + classInfo.LowerCaseClassName + "filters\">");
            //
            //if (classInfo.PascalCaseClassName == "SampleSinglePageApplication") {
            //    output.Add("\t\t\t\t\t <li>");
            //    output.Add("\t\t\t\t\t  <a class=\"dropdown-item\" href=\"#\" data-bind=\"html:MainModel().Language('MyOpenSampleSinglePageApplication'), click:function(){CommonSearch('MyOpen')}, css:{active: Filter().filterId() == window._guid1}\"></a>");
            //    output.Add("\t\t\t\t\t </li>");
            //    output.Add("\t\t\t\t\t <li>");
            //    output.Add("\t\t\t\t\t  <a class=\"dropdown-item\" href=\"#\" data-bind=\"html:MainModel().Language('MyClosedSampleSinglePageApplication'), click:function(){CommonSearch('MyClosed')}, css:{active: Filter().filterId() == window._guid2}\"></a>");
            //    output.Add("\t\t\t\t\t </li>");
            //}

            //output.Add("\t\t\t\t\t");
            //output.Add("\t\t\t\t\t <!-- ko if: MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto() != null && MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto().length > 0 -->");
            //output.Add("\t\t\t\t\t <li><hr class=\"dropdown-divider\"></li>");
            //output.Add("\t\t\t\t\t <!-- ko foreach: MainModel().User().savedFilters -->");
            //output.Add("\t\t\t\t\t <li>");
            //output.Add("\t\t\t\t\t  <a class=\"dropdown-item\" href=\"#\"");
            //output.Add("\t\t\t\t\t   data-bind=\"html:$data.description,");
            //output.Add("\t\t\t\t\t    click:function(){$root.RunSavedFilter($data)},");
            //output.Add("\t\t\t\t\t    css:{active: $root.Filter().filterId() == $data.savedFilterId() }\"></a>");
            //output.Add("\t\t\t\t\t </li>");
            //output.Add("\t\t\t\t\t <!-- /ko -->");
            //output.Add("\t\t\t\t\t <!-- /ko -->");
            //output.Add("\t\t\t\t\t</ul>");

            // TODO: add back export
            //output.Add("\t\t\t\t\t<button disabled type=\"button\" class=\"btn btn-dark\" data-bind=\"click:Export" + classInfo.PascalCaseClassNamePlural + ", enable:Loading() == false && Filter().records() != null && Filter().records().length > 0\">");
            //output.Add("\t\t\t\t\t <i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameFilterExport + "')\"></i>");
            //output.Add("\t\t\t\t\t</button>");

            output.Add("\t\t\t\t</div>");

            //TODO: Keyword search
            //output.Add("\t\t\t\t<div class=\"keyword-search\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
            //output.Add("\t\t\t\t <label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-keyword\">Keyword</label>");
            //output.Add("\t\t\t\t <div class=\"fixed-200\">");
            //output.Add("\t\t\t\t  <input type=\"text\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-keyword\" class=\"form-control\" data-bind=\"value:Filter().keyword\" placeholder=\"Search " + classInfo.SentenceCaseClassNamePlural + "\" />");
            //output.Add("\t\t\t\t </div>");
            //output.Add("\t\t\t\t</div>");
            //output.Add("\t\t\t\t");

            output.Add("\t\t\t</div>");
            output.Add("\t\t</div>");
            output.Add("");

            // todo: this is probably where the bug in the save filtername is happening, unelss i just don't have any code at all for it yet

            output.Add("\t\t<div data-bind=\"visible:GettingSaved" + classInfo.PascalCaseClassName + "FilterName() == true\">");
            output.Add("\t\t    <label for=\"saved-" + classInfo.LowerCaseClassName + "filter-name\" data-bind=\"html:MainModel().Language('" + classInfo.Language_ClassNameSavedFilterName + "')\"></label>");
            output.Add("\t\t    <div class=\"input-group\">");
            output.Add("\t\t        <button type=\"button\" class=\"btn btn-outline-dark\" data-bind=\"html:MainModel().IconAndText('Cancel'), click:function(){GettingSaved" + classInfo.PascalCaseClassName + "FilterName(false)}\"></button>");
            output.Add("\t\t        <button type=\"button\" class=\"btn btn-outline-success\" data-bind=\"html:MainModel().IconAndText('Save'), click:Save" + classInfo.PascalCaseClassName + "FilterRecord\"></button>");
            output.Add("\t\t        <input type=\"text\" class=\"form-control\" id=\"saved-" + classInfo.LowerCaseClassName + "filter-name\" data-bind=\"value:Saved" + classInfo.PascalCaseClassName + "FilterName\" />");
            output.Add("\t\t    </div>");
            output.Add("\t\t</div>");

            output.Add("\t\t<div class=\"row\" data-bind=\"visible:Loading() == true\">");
            output.Add("\t\t <div class=\"col-sm-12\" data-bind=\"html:MainModel().Language('" + classInfo.Language_ClassNameLoading + "');\"></div>");
            output.Add("\t\t</div>");

            output.Add("\t\t<div class=\"row\" data-bind=\"visible:Loading() == false\">");
            output.Add("\t\t <div data-bind=\"visible:Filter().showFilters() == true\">");
            output.Add("\t\t  <div class=\"row padbottom\">");

            //<div class=\"col-4 padbottom\">
            //    <label for=\"request-filter-firstname\" data-bind=\"html:MainModel().Language('FirstName')\"></label>
            //    <input type=\"text\" class=\"form-control\" id=\"request-filter-firstname\" data-bind=\"date:Filter().firstName\" />
            //</div>
            // first datetimes
            var orderedProps = props
                .OrderByDescending(o => o.IsEnum)
                .ThenByDescending(o => o.IsDateTime)
                .ThenByDescending(o => o.IsString)
                .ThenByDescending(o => o.IsBool)
                .ThenByDescending(o => o.IsJavaScriptNumber)
                .ThenByDescending(o => o.IsByte)
                .ThenByDescending(o => o.IsGuid)
                .ThenBy(o => o.CamelCaseVariableName).ToList();
            foreach (var prop in orderedProps) {
                output.Add($"\t\t\t\t@* {prop.PascalCaseClassName} {prop.PascalCaseVariableName} *@");
                if (prop.IsEnum && !prop.IsCollection && !prop.IsDictionary) {
                    output.Add("\t\t\t\t\t<div class=\"col-sm-3\">");
                    output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\">" + prop.SentenceCaseClassName + "</label>");
                    output.Add("\t\t\t\t\t\t<select class=\"form-control\" multiple=\"multiple\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" size=\"4\"");
                    output.Add("\t\t\t\t\t\t  data-bind=\"selectedOptions:Filter()." + prop.CamelCaseVariableNamePlural + ",");
                    output.Add("\t\t\t\t\t\t  options: sampleSinglePageApplicationEnumAutos.GetDropdown" + prop.PascalCaseClassName + "(),");
                    output.Add("\t\t\t\t\t\t  optionsText: function(item) {return item.name},");
                    output.Add("\t\t\t\t\t\t  optionsValue: function(item) {return item.value},");
                    output.Add("\t\t\t\t\t\t  valueAllowUnset: true\"></select>");
                    output.Add("\t\t\t\t\t</div>");
                } else if (prop.IsDateTime && prop.IsOnEFModel) {
                    output.Add("\t\t\t\t\t<div class=\"col-sm-2 padbottom\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "startauto\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableNameStart + "')\">" + prop.Language_VariableNameStart_Value + "</label>");
                    output.Add("\t\t\t\t\t\t<input type=\"text\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "startauto\" class=\"form-control datePicker\" data-bind=\"date:Filter()." + prop.CamelCaseVariableName + "Start\" placeholder=\"Search " + prop.Language_VariableNameStart_Value + "\" />");
                    output.Add("\t\t\t\t\t</div>");
                    output.Add("\t\t\t\t\t<div class=\"col-sm-2 padbottom\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "end\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableNameEnd + "')\">" + prop.Language_VariableNameEnd_Value + "</label>");
                    output.Add("\t\t\t\t\t\t<input type=\"text\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "end\" class=\"form-control datePicker\" data-bind=\"date:Filter()." + prop.CamelCaseVariableName + "End\" placeholder=\"Search " + prop.Language_VariableNameEnd_Value + "\" />");
                    output.Add("\t\t\t\t\t</div>");
                } else if (prop.IsString && prop.IsOnEFModel) {
                    // now strings
                    output.Add("\t\t\t\t\t<div class=\"col-sm-3 padbottom\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "')\">" + prop.Language_VariableName_Value + "</label>");
                    output.Add("\t\t\t\t\t\t<div class=\"form-check form-switch right\">");
                    output.Add("\t\t\t\t\t\t\t<input type=\"checkbox\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "filterexact\" role=\"switch\" class=\"form-check-input\"  data-bind=\"checked:Filter()." + prop.CamelCaseVariableName + "FilterExact\">");
                    output.Add("\t\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "filterexact\" class=\"unbold\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableNameFilterExact + "')\">Filter Exact</label>");
                    output.Add("\t\t\t\t\t\t</div>");
                    // TODO: Include in keyword search
                    //output.Add("\t\t\t\t\t\t<div class=\"form-check form-switch right\">");
                    //output.Add("\t\t\t\t\t\t\t<input type=\"checkbox\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "includeinkeyword\" role=\"switch\" class=\"form-check-input\" data-bind=\"checked:Filter()." + prop.CamelCaseVariableName + "IncludeInKeyword\">");
                    //output.Add("\t\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "includeinkeyword\" class=\"unbold\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableNameIncludeInKeyword + "')\">Include In Keyword</label>");
                    //output.Add("\t\t\t\t\t\t</div>");
                    output.Add("\t\t\t\t\t\t<input type=\"text\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" class=\"form-control\" data-bind=\"value:Filter()." + prop.CamelCaseVariableName + "\" placeholder=\"Search " + prop.SentenceCaseVariableName + "\" />");
                    output.Add("\t\t\t\t\t</div>");
                } else if (prop.IsBool && prop.IsOnEFModel) {
                    // now bools - for bools don't do true false, we should do true false and n/a (null)
                    //output.Add("\t\t\t\t\t<div class=\"col-sm-3 padbottom\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    //output.Add("\t\t\t\t\t\t<div class=\"form-check form-switch\">");
                    //output.Add("\t\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "')\">" + prop.Language_VariableName_Value + "</label>");
                    //output.Add("\t\t\t\t\t\t\t<input  type=\"checkbox\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" role=\"switch\" class=\"form-check-input\" data-bind=\"checked:Filter()." + prop.CamelCaseVariableName + "\" />");
                    //output.Add("\t\t\t\t\t\t</div>");
                    //output.Add("\t\t\t\t\t</div>");

                    output.Add("\t\t\t\t\t<div class=\"col-sm-3\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "')\"></label>");
					output.Add("\t\t\t\t\t\t<select class=\"form-control\" size=\"4\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" data-bind=\"value:Filter()." + prop.CamelCaseVariableName + "\">");
					output.Add("\t\t\t\t\t\t\t<option value=\"\">All</option>");
					output.Add("\t\t\t\t\t\t\t<option value=\"true\">Is " + prop.PascalCaseVariableName + "</option>");
					output.Add("\t\t\t\t\t\t\t<option value=\"false\">Is not " + prop.PascalCaseVariableName + " </option>");
					output.Add("\t\t\t\t\t\t</select>");
                    output.Add("\t\t\t\t\t</div>");

                } else if (prop.IsJavaScriptNumber && prop.IsOnEFModel) {
                    // now numbers
                    output.Add("\t\t\t\t\t<div class=\"col-sm-3 padbottom\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "')\">" + prop.Language_VariableName_Value + "</label>");
                    output.Add("\t\t\t\t\t\t<input type=\"number\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" class=\"form-control\" data-bind=\"value:Filter()." + prop.CamelCaseVariableName + "\" />");
                    output.Add("\t\t\t\t\t</div>");
                } else if (prop.IsGuid && prop.IsOnEFModel) {
                    //  so we don't really want guids, unless its a lookup from another table.. so TODO: i guess
                    output.Add("\t\t\t\t\t<!-- ERROR_ISGUID   " + prop.PascalCaseVariableName + "   -->");
                } else if (prop.IsByte && prop.IsOnEFModel) {
                    //  so we don't really want bytes, ... just don't do it auto probably
                    output.Add("\t\t\t\t\t<!-- ERROR_ISBYTE   " + prop.PascalCaseVariableName + "   -->");
                } else if (!prop.IsOnEFModel) {
                    output.Add("\t\t\t\t\t<!-- !ERROR_IsOnEFModel   " + prop.PascalCaseVariableName + "   -->");
                } else if (prop.ActualType == typeof(DataObjects.UserGroupSettings)) {
                    // hardcoded for UserGroupSettings
                    output.Add("\t\t\t\t\t<!-- Dont need filtering for " + prop.PascalCaseVariableName + "   -->");
                    /*
                     *  
                     *  var settings = DeserializeObject<DataObjects.UserGroupSettings>(rec.Settings);
                     *  if(settings == null) {
                     *      settings = new DataObjects.UserGroupSettings();
                     *  }
                     *
                     * 
                     * 
                     */
                } else {
                    // now everything we missed .. .what is is there?
                    throw (new Exception("new prop type encountered... deal with this before going on"));
                    output.Add("\t\t     // ERRORXXXXX     <div class=\"col-sm-3 padbottom\" data-bind=\"visible:Filter().showFilters() == true && Loading() == false\">");
                    output.Add("\t\t     // ERRORXXXXX      <label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "')\">" + prop.Language_VariableName_Value + "</label>");
                    output.Add("\t\t     // ERRORXXXXX      <input type=\"text\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-" + prop.LowerCaseVariableName + "\" class=\"form-control\" data-bind=\"value:Filter()." + prop.CamelCaseVariableName + "\" placeholder=\"Search " + prop.SentenceCaseVariableName + "\" />");
                    output.Add("\t\t     // ERRORXXXXX     </div>");
                }
            }
            for (int i = 1; i <= 10; i++) {
                var numberString = "";
                if (i < 10) {
                    numberString = $"0{i}";
                } else {
                    numberString = $"{i}";
                }
                output.Add($"\t\t\t\t@* UDF {i} *@");
                output.Add("\t\t\t\t\t<div class=\"col-3 padbottom\" data-bind=\"visible:MainModel().UDFShowInFilter('" + classInfo.PascalCaseClassNamePlural + "', " + i + ") == true\">");
                output.Add("\t\t\t\t\t\t<label for=\"" + classInfo.LowerCaseClassNamePlural + "-filter-udf" + numberString + "\" data-bind=\"text:MainModel().UDFLabel('" + classInfo.PascalCaseClassNamePlural + "', " + i + ")\"></label>");
                output.Add("\t\t\t\t\t\t<select class=\"form-control\" id=\"" + classInfo.LowerCaseClassNamePlural + "-filter-udf" + numberString + "\" size=\"4\"data-bind=\"value:Filter().udf" + numberString + ",options: MainModel().UDFFilterOptions('" + classInfo.PascalCaseClassNamePlural + "', " + i + "),optionsCaption: '',valueAllowUnset: true\"></select>");
                output.Add("\t\t\t\t\t</div>");

            }

            output.Add("\t\t\t\t</div>");
            output.Add("\t\t\t</div>");
            output.Add("\t\t");
            output.Add("\t\t\t<div data-bind=\"visible:Filter().recordCount() == 0, html:MainModel().Language('" + classInfo.Language_VariableNameNoRecords + "')\"></div>");
            output.Add("\t\t");
            output.Add("\t\t\t<div data-bind=\"visible:Filter().recordCount() > 0\">");
            output.Add("\t\t\t\t<div id=\"" + classInfo.LowerCaseClassName + "-records-auto\"></div>");
            output.Add("\t\t\t</div>");
            output.Add("\t\t</div>");
            output.Add("\t</div>");

            output.Add("");
            output.Add("\t<div data-bind=\"visible:(MainModel().CurrentView() == 'edit" + classInfo.LowerCaseClassName + "auto' || MainModel().CurrentView() == 'new" + classInfo.LowerCaseClassName + "auto')\">");
            output.Add("\t\t<div data-bind=\"css:{fixed: MainModel().StickyMenus() == true}\">");
            output.Add("\t\t\t<i class=\"sticky-menu-icon\" data-bind=\"html:MainModel().StickyIcon(), click:function() {MainModel().ToggleStickyMenus()}\"></i>");
            output.Add("\t\t\t<h1 class=\"display-7\">");
            output.Add("\t\t\t\t<!-- ko if: MainModel().CurrentView() == 'edit" + classInfo.LowerCaseClassName + "auto' -->");
            output.Add("\t\t\t\t<i data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameEdit + "')\"></i>");
            output.Add("\t\t\t\t<span class=\"element-id\" data-bind=\"visible:Loading() == false\">&ldquo;<span data-bind=\"text:MainModel().Id()\"></span>&rdquo;</span>");
            output.Add("\t\t\t\t<!-- /ko -->");
            output.Add("\t\t\t\t<!-- ko if: MainModel().CurrentView() == 'new" + classInfo.LowerCaseClassName + "auto' -->");
            output.Add("\t\t\t\t<i data-bind=\"html:MainModel().Icon('" + classInfo.PascalCaseClassNamePlural + "')\"></i><span class=\"icon-text\" data-bind=\"html:MainModel().Language('" + classInfo.Language_ClassNameAddNew + "')\"></span>");

            if (primaryKeyType == "Guid") {
                output.Add("\t\t\t\t<span class=\"element-id\" data-bind=\"visible:" + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "() != null && " + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "() != MainModel().GuidEmpty()\">");
            } else {
                output.Add("\t\t\t\t<span class=\"element-id\" data-bind=\"visible:" + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "() != null && " + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "() != null && " + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "() != 0\">");
            }
            output.Add("\t\t\t\t\t&ldquo;<span data-bind=\"text:" + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "\"></span>&rdquo;");
            output.Add("\t\t\t\t</span>");
            output.Add("\t\t\t\t<!-- /ko -->");
            output.Add("\t\t\t</h1>");
            output.Add("\t");
            output.Add("\t\t\t<div class=\"padbottom\">");
            output.Add("\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"click:function() { MainModel().Nav('" + classInfo.LowerCaseClassNamePlural + "auto') }, html:MainModel().IconAndText('" + classInfo.Language_ClassNameBack + "')\"></button>");
            output.Add("\t\t\t\t<button type=\"button\" class=\"btn btn-success\" data-bind=\"click:Save" + classInfo.PascalCaseClassName + ", html:MainModel().IconAndText('" + classInfo.Language_ClassNameSave + "')\"></button>");
            output.Add("\t\t\t\t<!-- ko if: MainModel().Id() != null && MainModel().Id() != '' -->");
            output.Add("\t\t\t\t<!-- ko if: ConfirmDelete" + classInfo.PascalCaseClassName + "() != MainModel().Id() -->");
            output.Add("\t\t\t\t<button type=\"button\" class=\"btn btn-danger\" data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameConfirmDelete + "'), click:function() { ConfirmDelete" + classInfo.PascalCaseClassName + "(MainModel().Id()) }\"></button>");
            output.Add("\t\t\t\t<!-- /ko -->");
            output.Add("\t\t\t\t<!-- ko if: ConfirmDelete" + classInfo.PascalCaseClassName + "() == MainModel().Id() -->");
            output.Add("\t\t\t\t<div class=\"btn-group\" role=\"group\">");
            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-dark\" data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameCancel + "'), click:function() { ConfirmDelete" + classInfo.PascalCaseClassName + "('') }\"></button>");
            output.Add("\t\t\t\t\t<button type=\"button\" class=\"btn btn-danger\" data-bind=\"html:MainModel().IconAndText('" + classInfo.Language_ClassNameConfirmDelete + "'), click:Delete" + classInfo.PascalCaseClassName + "\"></button>");
            output.Add("\t\t\t\t</div>");
            output.Add("\t\t\t\t<!-- /ko -->");
            output.Add("\t\t\t\t<!-- /ko -->");
            output.Add("\t\t\t</div>");
            output.Add("\t\t</div>");
            output.Add("\t");
            output.Add("\t\t<div data-bind=\"visible:Loading() == true, html:MainModel().Language('" + classInfo.Language_ClassNameLoading + "')\"></div>");
            output.Add("\t");
            output.Add("\t\t<div data-bind=\"visible:" + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "() != null && Loading() == false\">");
            output.Add("\t");
            output.Add("\t\t\t<table class=\"table first-column-small first-column-bold first-column-right\">");
            //output.Add("\t   <tr>");
            //output.Add("\t    <td><label for=\"edit-" + classInfo.LowerCaseClassName + "-type\" data-bind=\"html:MainModel().Language('" + classInfo.CSharpClassName + " Type', null, true)\"></label></td>");
            //output.Add("\t    <td>");
            //output.Add("\t      <select class=\"form-control\" id=\"edit-" + classInfo.LowerCaseClassName + "-type\"");
            //output.Add("\t       data-bind=\"value:" + classInfo.CSharpClassName + "().type,");
            //output.Add("\t       options: Types,");
            //output.Add("\t       optionsText: function(item) {return item.name},");
            //output.Add("\t       optionsValue: function(item) {return item.value},");
            //output.Add("\t       valueAllowUnset: false\"></select>");
            //output.Add("\t    </td>");
            //output.Add("\t   </tr>");

            // each prop now can be an input

            //<input type="checkbox" id="edit-user-enabled" role="switch" class="form-check-input" data-bind="checked:User().enabled">

            foreach (var prop in orderedProps) {
                output.Add($"\t\t\t\t@* {prop.PascalCaseClassName} {prop.PascalCaseVariableName} *@");
                if (prop.IsEfModel && !prop.IsPrimaryKey && !prop.IsCollection && !prop.IsDictionary && prop.PascalCaseVariableName != "ActionResponse" && prop.PascalCaseVariableName != "TenantId") {
                    if (prop.IsBool) {
                        output.Add("\t\t\t\t<tr>");
                        output.Add("\t\t\t\t\t<td><label for=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "', null, true)\"></label></td>");
                        output.Add("\t\t\t\t\t<td><input type=\"checkbox\" id=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" role=\"switch\" class=\"form-check-input\" data-bind=\"checked:" + classInfo.PascalCaseClassName + "()." + prop.CamelCaseVariableName + "\" /></td>");
                        output.Add("\t\t\t\t</tr>");
                    } else if (prop.IsString || prop.IsGuid) {
                        output.Add("\t\t\t\t<tr>");
                        output.Add("\t\t\t\t\t<td><label for=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "', null, true)\"></label></td>");
                        output.Add("\t\t\t\t\t<td><input type=\"text\" id=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\"class=\"form-control\" data-bind=\"value:" + classInfo.PascalCaseClassName + "()." + prop.CamelCaseVariableName + "\" /></td>");
                        output.Add("\t\t\t\t</tr>");
                    } else if (prop.IsJavaScriptNumber) {
                        output.Add("\t\t\t\t\t\t<tr>");
                        output.Add("\t\t\t\t\t<td><label for=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "', null, true)\"></label></td>");
                        output.Add("\t\t\t\t\t<td><input type=\"number\" id=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" class=\"form-control\" data-bind=\"value:" + classInfo.PascalCaseClassName + "()." + prop.CamelCaseVariableName + "\" /></td>");
                        output.Add("\t\t\t\t</tr>");
                    } else if (prop.IsJavaScriptDate) {
                        output.Add("\t\t\t\t\t\t\t<tr>");
                        output.Add("\t\t\t\t\t<td><label for=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.Language_VariableName + "', null, true)\"></label></td>");
                        output.Add("\t\t\t\t\t<td><input type=\"text\" id=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" class=\"form-control dateTimePicker\" data-bind=\"dateTime:" + classInfo.PascalCaseClassName + "()." + prop.CamelCaseVariableName + "\" /></td>");
                        output.Add("\t\t\t\t</tr>");
                    } else if (prop.IsEnum) {
                        output.Add("\t\t\t\t<tr>");
                        output.Add("\t\t\t\t\t<td><label for=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\" data-bind=\"html:MainModel().Language('" + prop.PascalCaseVariableName + "', null, true)\"></label></td>");
                        output.Add("\t\t\t\t\t<td>");
                        output.Add("\t\t\t\t\t\t<select class=\"form-control\" id=\"edit-" + classInfo.LowerCaseClassName + "-" + prop.LowerCaseVariableName + "\"");
                        output.Add("\t\t\t\t\t\t\tdata-bind=\"value:" + classInfo.PascalCaseClassName + "()." + prop.CamelCaseVariableName + ",");
                        output.Add("\t\t\t\t\t\t\toptions: sampleSinglePageApplicationEnumAutos.GetDropdown" + prop.PascalCaseClassName + "(),");
                        output.Add("\t\t\t\t\t\t\toptionsText: function(item) {return item.name},");
                        output.Add("\t\t\t\t\t\t\toptionsValue: function(item) {return item.value},");
                        output.Add("\t\t\t\t\t\t\tvalueAllowUnset: false\"></select>");
                        output.Add("\t\t\t\t\t</td>");
                        output.Add("\t\t\t\t</tr>");
                    } else {
                        // we have a bool that cannot be null
                        output.Add(("@*ERROR: // not an int, bool, guid or string, or datetime. but it still on the ef model, how odd, maybe like a double? ") + $"\t\t\t{prop.PascalCaseVariableName} = item.{prop.PascalCaseVariableName} ?? \"\", //OTHER nullable on ef model: base({prop.BaseType.Name}) : actual({prop.ActualType.Name})*@");
                    }
                }
            }

            output.Add("\t\t\t</table>");
            output.Add("\t\t</div>");
            output.Add("\t");
            output.Add("\t\t<div id=\"edit-" + classInfo.LowerCaseClassName + "-udf-fields\" data-bind=\"visible:Loading() == false\"></div>");
            output.Add("\t</div>");

            output.Add("\t<form id=\"" + classInfo.LowerCaseClassName + "-filter-download\" method=\"post\" action=\"/File/Download" + classInfo.PascalCaseClassName + "s/\" target=\"_blank\" style=\"display:none;\">");
            output.Add("\t\t<textarea name=\"" + classInfo.LowerCaseClassName + "-filter\" id=\"" + classInfo.LowerCaseClassName + "-filter\" aria-hidden=\"true\"></textarea>");
            output.Add("\t\t<input type=\"hidden\" name=\"" + classInfo.LowerCaseClassName + "-filter-tenantid\" id=\"" + classInfo.LowerCaseClassName + "-filter-tenantid\" data-bind=\"value:MainModel().TenantId()\" />");
            output.Add("\t\t<input type=\"hidden\" name=\"" + classInfo.LowerCaseClassName + "-filter-user-token\" id=\"" + classInfo.LowerCaseClassName + "-filter-user-token\" data-bind=\"value:MainModel().Token()\" />");
            output.Add("\t</form>");

            output.Add($"</div>");

            return output;
        }

        /// <summary>
        /// This takes an enum and returns a list of strings that can be used to represent that enum
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="classInfo"></param>
        /// <param name="enumValues"></param>
        /// <returns></returns>
        public static List<string> TranscribeTypeScriptEnumConvertMethods(Type classType, ReflectedClassInfo classInfo, Array enumValues)
        {

            var typeScriptEnumList = new List<string>();
            typeScriptEnumList.Add("\t/**");
            typeScriptEnumList.Add("\t * " + classInfo.PascalCaseClassName);
            typeScriptEnumList.Add("\t */");
            // start of the convert method for the enum
            typeScriptEnumList.Add($"\texport function Convert{classInfo.PascalCaseClassName}ToHumanReadableString(value: number | string | server.{classInfo.CamelCaseClassName}): string" + " {//" + classType.ToString());
            typeScriptEnumList.Add("\t\tlet output: string = \"\";");
            typeScriptEnumList.Add("");
            // start of the if checking for value
            typeScriptEnumList.Add("\t\tif (tsUtilities.HasValue(\"\" + value)) {");
            // start of the switch of the value
            typeScriptEnumList.Add("\t\t\tswitch (value) {");

            int count = 0;
            foreach (var item in enumValues) {
                typeScriptEnumList.Add($"\t\t\t\tcase {count}:");
                typeScriptEnumList.Add($"\t\t\t\tcase \"{count}\":");

                typeScriptEnumList.Add($"\t\t\t\t\toutput = \"{item.ToString()}\";");
                typeScriptEnumList.Add($"\t\t\t\t\tbreak;");
                count++;
            }
            typeScriptEnumList.Add("\t\t\tdefault:");
            typeScriptEnumList.Add("\t\t\t\tbreak;");

            // close the switch
            typeScriptEnumList.Add("\t\t\t}");
            // close the if
            typeScriptEnumList.Add("\t\t}");
            typeScriptEnumList.Add("");
            typeScriptEnumList.Add("\t\treturn output;");
            // close the export method
            typeScriptEnumList.Add("\t}");
            typeScriptEnumList.Add("");

            typeScriptEnumList.Add("\t/**");
            typeScriptEnumList.Add("\t * " + classInfo.PascalCaseClassName);
            typeScriptEnumList.Add("\t */");
            // start of the convert method for the enum
            typeScriptEnumList.Add($"\texport function Convert{classInfo.PascalCaseClassName}ToString(value: number | string | server.{classInfo.CamelCaseClassName}): string" + " {//" + classType.ToString());
            typeScriptEnumList.Add("\t\tlet output: string = \"\";");
            typeScriptEnumList.Add("");
            // start of the if checking for value
            typeScriptEnumList.Add("\t\tif (tsUtilities.HasValue(\"\" + value)) {");
            // start of the switch of the value
            typeScriptEnumList.Add("\t\t\tswitch (value) {");

            count = 0;
            foreach (var item in enumValues) {
                typeScriptEnumList.Add($"\t\t\t\tcase {count}:");
                typeScriptEnumList.Add($"\t\t\t\tcase \"{count}\":");

                typeScriptEnumList.Add($"\t\t\t\t\toutput = \"{item.ToString().ToLower()}\";");
                typeScriptEnumList.Add($"\t\t\t\t\tbreak;");
                count++;
            }
            typeScriptEnumList.Add("\t\t\tdefault:");
            typeScriptEnumList.Add("\t\t\t\tbreak;");

            // close the switch
            typeScriptEnumList.Add("\t\t\t}");
            // close the if
            typeScriptEnumList.Add("\t\t}");
            typeScriptEnumList.Add("");
            typeScriptEnumList.Add("\t\treturn output;");
            // close the export method
            typeScriptEnumList.Add("\t}");

            /// now the opposite, convert from string to int

            typeScriptEnumList.Add("\t/**");
            typeScriptEnumList.Add("\t * " + classInfo.PascalCaseClassName);
            typeScriptEnumList.Add("\t */");
            // start of the convert method for the enum
            typeScriptEnumList.Add($"\texport function Convert{classInfo.PascalCaseClassName}FromString(value: number | string): server.{classInfo.CamelCaseClassName}" + " {//" + classType.ToString());
            typeScriptEnumList.Add($"\t\tlet output: server.{classInfo.CamelCaseClassName} = null;");
            typeScriptEnumList.Add("");
            // start of the if checking for value
            typeScriptEnumList.Add("\t\tif (tsUtilities.HasValue(\"\" + value)) {");
            // start of the switch of the value
            typeScriptEnumList.Add("\t\t\tlet lowerValue = (\"\" + value).toLowerCase();");
            typeScriptEnumList.Add("\t\t\tswitch (lowerValue) {");

            count = 0;
            foreach (var item in enumValues) {
                //enumList.Add($"\t\t\t\tcase {count}:");
                typeScriptEnumList.Add($"\t\t\t\tcase \"{count}\":");
                typeScriptEnumList.Add($"\t\t\t\tcase \"{item.ToString().ToLower()}\":");

                typeScriptEnumList.Add($"\t\t\t\t\toutput = {count}");
                typeScriptEnumList.Add($"\t\t\t\t\tbreak;");
                count++;
            }
            typeScriptEnumList.Add("\t\t\tdefault:");
            typeScriptEnumList.Add("\t\t\t\tbreak;");

            // close the switch
            typeScriptEnumList.Add("\t\t\t}");
            // close the if
            typeScriptEnumList.Add("\t\t}");
            typeScriptEnumList.Add("");
            typeScriptEnumList.Add("\t\treturn output;");
            // close the export method
            typeScriptEnumList.Add("\t}");

            /// now finally make a list of the enum that can be used in a dropdown
            typeScriptEnumList.Add("\t/**");
            typeScriptEnumList.Add("\t * " + classInfo.PascalCaseClassName);
            typeScriptEnumList.Add("\t */");
            // start of the convert method for the enum
            typeScriptEnumList.Add($"\texport function GetDropdown{classInfo.PascalCaseClassName}(includeDefault: boolean = false): server.dropDownEnum[]" + " {//" + classType.ToString());
            typeScriptEnumList.Add($"\t\tlet output: server.dropDownEnum[] = [];");

            typeScriptEnumList.Add("\t\tif (includeDefault) {");
            typeScriptEnumList.Add($"\t\t\tlet itemChoose: server.dropDownEnum =" + " { " + $"name: \"Please chose a {classInfo.PascalCaseClassName}\", value: null" + " }" + $";");
            typeScriptEnumList.Add($"\t\t\toutput.push(itemChoose);");
            typeScriptEnumList.Add("\t\t}");

            count = 0;
            foreach (var item in enumValues) {
                typeScriptEnumList.Add($"\t\tlet item{item.ToString()}: server.dropDownEnum =" + " { " + $"name: \"{item.ToString()}\", value: {count}" + " }" + $"; // \"{item.ToString().ToLower()}\":");
                typeScriptEnumList.Add($"\t\toutput.push(item{item.ToString()});");

                //typeScriptEnumList.Add($"\t\t\t\titem.name = \"{item.ToString()}\";");
                //typeScriptEnumList.Add($"\t\t\t\titem.value = {count};");

                count++;
            }
            typeScriptEnumList.Add("\t\treturn output;");
            // close the export method
            typeScriptEnumList.Add("\t}");

            return typeScriptEnumList;
        }

        public static List<string> TranscribeViewModels(Type classType, ReflectedClassInfo classInfo, List<ReflectedClassInfo> props)
        {
            //var primaryKey = props.FirstOrDefault(o => o.IsPrimaryKey);

            //var primaryKeyString = "";
            //if(primaryKey != null) {
            //    primaryKeyString = primaryKey.CSharpVariableName;
            //} else {
            //    primaryKeyString = classInfo.CSharpClassName + "Id";
            //}
            //var camelCasePrimaryKeyString = ""//primaryKey.CamelCaseVariableName;
            //var primaryKeyType = "Guid";
            //if (primaryKey != null) {
            //    primaryKeyString = primaryKey.CSharpVariableName;
            //    primaryKeyType = primaryKey.CSharpClassName;
            //}

            var primaryKey = props.FirstOrDefault(o => o.IsPrimaryKey);

            var cSharpPrimaryKeyString = classInfo.PascalCaseClassName + "Id";
            var camelCasePrimaryKeyString = classInfo.CamelCaseClassName + "Id";
            var primaryKeyType = "Guid";
            if (primaryKey != null) {
                cSharpPrimaryKeyString = primaryKey.PascalCaseVariableName;
                camelCasePrimaryKeyString = primaryKey.CamelCaseVariableName;
                primaryKeyType = primaryKey.PascalCaseClassName;
            }

            if (!classInfo.IsEfModel) {
                return new List<string>();
            }
            var output = new List<string>();

            output.Add($"class {classInfo.PascalCaseClassNamePlural}ModelAuto " + "{");
            output.Add($"\tMainModel: KnockoutObservable<MainModel> = ko.observable(window.mainModel);");

            output.Add("\tFilter: KnockoutObservable<filter" + classInfo.PascalCaseClassNamePlural + "Auto> = ko.observable(new filter" + classInfo.PascalCaseClassNamePlural + "Auto);");
            output.Add("\t" + classInfo.PascalCaseClassName + ": KnockoutObservable<" + classInfo.CamelCaseClassName + "> = ko.observable(new " + classInfo.CamelCaseClassName + ");");
            output.Add("\t" + classInfo.PascalCaseClassNamePlural + ": KnockoutObservableArray<" + classInfo.CamelCaseClassName + "> = ko.observableArray([]);");
            output.Add("\tLoading: KnockoutObservable<boolean> = ko.observable(false);");

            // utility variables needed for
            output.Add("\tConfirmDelete" + classInfo.PascalCaseClassName + ": KnockoutObservable<string> = ko.observable(\"\");");
            output.Add("\tShow" + classInfo.PascalCaseClassName + "Details: KnockoutObservable<boolean> = ko.observable(false);");
            output.Add("\tFilterView" + classInfo.PascalCaseClassName + "Type: KnockoutObservable<string> = ko.observable(\"list\");");
            output.Add("\tGettingSaved" + classInfo.PascalCaseClassName + "FilterName: KnockoutObservable<boolean> = ko.observable(false);");
            output.Add("\tSaved" + classInfo.PascalCaseClassName + "FilterName: KnockoutObservable<string> = ko.observable(\"\");");

            output.Add("");
            output.Add("\tconstructor() {");
            output.Add("\t\tthis.MainModel().View.subscribe(() => {");
            output.Add("\t\t\tthis.ViewChanged();");
            output.Add("\t\t});");
            output.Add("");
            output.Add("\t\tthis.MainModel().SignalRUpdate.subscribe(() => {");
            output.Add("\t\t\tthis.SignalrUpdate();");
            output.Add("\t\t});");
            output.Add("\t\t//setTimeout(\"setupUserPhotoDropZone()\", 0);");
            output.Add("\t\tsetTimeout(() => this.StartFilterMonitoring(), 1000);");
            output.Add("\t}");

            //needed functions
            output.Add("\t//TODO: click: DeleteSaved" + classInfo.PascalCaseClassName + "Filter");
            output.Add("\tDeleteSaved" + classInfo.PascalCaseClassName + "Filter(): void {");
            output.Add("\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\tlet filterId: string = this.Filter().filterId();");
            output.Add("\t\t");
            output.Add("\t\tlet success: Function = (data: server.booleanResponse) => {");
            output.Add("\t\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\t\t");
            output.Add("\t\t\tif (data != null) {");
            output.Add("\t\t\t\tif (data.result) {");
            output.Add("\t\t\t\t\tlet existing: savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto = ko.utils.arrayFirst(this.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto(), function (item) {");
            output.Add("\t\t\t\t\t\treturn item.savedFilterId() == filterId;");
            output.Add("\t\t\t\t\t});");
            output.Add("\t\t\t\t\tif (existing != null) {");
            output.Add("\t\t\t\t\t\tthis.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto.remove(existing);");
            output.Add("\t\t\t\t\t}");
            output.Add("\t\t\t\t\t");


            output.Add("\t\t\t\t\tthis.Clear" + classInfo.PascalCaseClassName + "Filter();");

            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.MainModel().Message_Errors(data.messages);");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\tthis.MainModel().Message_Error(\"An unknown error occurred attempting to delete the saved filter.\");");
            output.Add("\t\t\t}");
            output.Add("\t\t};");
            output.Add("\t\t");
            output.Add("\t\tthis.MainModel().Message_Deleting();");
            output.Add("\t\ttsUtilities.AjaxData(window.baseURL + \"api/Data/DeleteSaved" + classInfo.PascalCaseClassName + "Filter/\" + filterId.toString(), null, success);");
            output.Add("\t}");

            output.Add("\t//TODO: click: Export" + classInfo.PascalCaseClassName + "");

            output.Add("\tExport" + classInfo.PascalCaseClassNamePlural + "(): void {");
            output.Add("\t\tlet filter: filter" + classInfo.PascalCaseClassNamePlural + "Auto = new filter" + classInfo.PascalCaseClassNamePlural + "Auto;");
            output.Add("\t\tfilter.Load(JSON.parse(ko.toJSON(this.Filter)));");
            output.Add("\t\tfilter.records([]);");
            output.Add("\t\t$(\"#" + classInfo.LowerCaseClassName + "-filter\").val(ko.toJSON(filter));");
            output.Add("\t\t$(\"#" + classInfo.LowerCaseClassName + "-filter-download\").submit();");
            output.Add("\t}");

            

            output.Add("");
            output.Add("\tToggle" + classInfo.PascalCaseClassName + "Details(): void {");
            output.Add("\t\tif (this.Show" + classInfo.PascalCaseClassName + "Details()) {");
            output.Add("\t\t\tthis.Show" + classInfo.PascalCaseClassName + "Details(false);");
            output.Add("\t\t\tlocalStorage.setItem(\"" + classInfo.LowerCaseClassName + "-details-\" + this.MainModel().TenantId(), \"0\");");
            output.Add("\t\t} else {");
            output.Add("\t\t\tthis.Show" + classInfo.PascalCaseClassName + "Details(true);");
            output.Add("\t\t\tlocalStorage.setItem(\"" + classInfo.LowerCaseClassName + "-details-\" + this.MainModel().TenantId(), \"1\");");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t\tthis.Render" + classInfo.PascalCaseClassName + "Table();");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Called when the Show or Hide Filter buttons are clicked.");
            output.Add("\t */");
            output.Add("\tToggleShow" + classInfo.PascalCaseClassName + "Filter(): void {");
            output.Add("\t\tthis.Filter().showFilters(!this.Filter().showFilters());");
            output.Add("\t\tthis.Save" + classInfo.PascalCaseClassName + "Filter();");
            output.Add("\t}");
            output.Add("\tToggle" + classInfo.PascalCaseClassName + "View(): void {");
            output.Add("\t\tif (this.FilterView" + classInfo.PascalCaseClassName + "Type() == \"list\") {");
            output.Add("\t\t\tthis.FilterView" + classInfo.PascalCaseClassName + "Type(\"card\");");
            output.Add("\t\t} else {");
            output.Add("\t\t\tthis.FilterView" + classInfo.PascalCaseClassName + "Type(\"list\");");
            output.Add("\t\t}");
            output.Add("\t\tlocalStorage.setItem(\"" + classInfo.LowerCaseClassName + "-view-\" + this.MainModel().TenantId(), this.FilterView" + classInfo.PascalCaseClassName + "Type());");
            output.Add("\t\tthis.Render" + classInfo.PascalCaseClassName + "Table();");
            output.Add("\t}");

            output.Add("\tSave" + classInfo.PascalCaseClassName + "FilterRecord(): void {");
            output.Add("\t\tif (!tsUtilities.HasValue(this.Saved" + classInfo.PascalCaseClassName + "FilterName())) {");
            output.Add("\t\t\ttsUtilities.DelayedFocus(\"saved-" + classInfo.LowerCaseClassName + "-filter-name\");");
            output.Add("\t\t\treturn;");
            output.Add("\t\t}");
            output.Add("\t\t");
            output.Add("\t\tlet success: Function = (data: server.savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto) => {");
            output.Add("\t\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\t\t");
            output.Add("\t\t\tif (data != null) {");
            output.Add("\t\t\t\tif (data.actionResponse.result) {");
            output.Add("\t\t\t\t\tlet existing: savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto = null;");
            output.Add("\t\t\t\t\t");
            output.Add("\t\t\t\t\tif (this.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto() != null && this.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto().length > 0) {");
            output.Add("\t\t\t\t\t\texisting = ko.utils.arrayFirst(this.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto(), function (item) {");
            output.Add("\t\t\t\t\t\t\treturn item.savedFilterId() == data.savedFilterId;");
            output.Add("\t\t\t\t\t\t});");
            output.Add("\t\t\t\t\t}");
            output.Add("\t\t\t\t\t");
            output.Add("\t\t\t\t\tif (existing != null) {");
            output.Add("\t\t\t\t\t\texisting.Load(data);");
            output.Add("\t\t\t\t\t} else {");
            output.Add("\t\t\t\t\t\tlet newSavedFilter: savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto = new savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto();");
            output.Add("\t\t\t\t\t\tnewSavedFilter.Load(data);");
            output.Add("\t\t\t\t\t\tthis.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto.push(newSavedFilter);");
            output.Add("\t\t\t\t\t}");
            output.Add("\t\t\t\t\t");
            output.Add("\t\t\t\t\tthis.MainModel().User().savedFilters" + classInfo.PascalCaseClassNamePlural + "Auto.sort(function (l, r) {");
            output.Add("\t\t\t\t\t\treturn l.description() > r.description() ? 1 : -1;");
            output.Add("\t\t\t\t\t});");
            output.Add("\t\t\t\t\t");
            output.Add("\t\t\t\t\tthis.GettingSaved" + classInfo.PascalCaseClassName + "FilterName(false);");
            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.MainModel().Message_Errors(data.actionResponse.messages);");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\tthis.MainModel().Message_Error(\"An unknown error occurred attempting to save the filter.\");");
            output.Add("\t\t\t}");
            output.Add("\t\t};");

            output.Add("\t\tthis.Filter().tenantId(this.MainModel().TenantId());");
            output.Add("\t\t");
            output.Add("\t\tlet f: filter" + classInfo.PascalCaseClassNamePlural + "Auto = new filter" + classInfo.PascalCaseClassNamePlural + "Auto();");
            output.Add("\t\tf.Load(JSON.parse(ko.toJSON(this.Filter)));");
            output.Add("\t\tf.columns(null);");
            output.Add("\t\tf.records(null);");
            output.Add("\t\t");
            output.Add("\t\tlet postFilter: savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto = new savedFilter" + classInfo.PascalCaseClassNamePlural + "Auto();");
            output.Add("\t\tpostFilter.description(this.Saved" + classInfo.PascalCaseClassName + "FilterName());");
            output.Add("\t\tpostFilter.savedFilterId(this.Filter().filterId());");
            output.Add("\t\tpostFilter.userId(this.MainModel().User().userId());");
            output.Add("\t\tpostFilter.tenantId(this.MainModel().TenantId());");
            output.Add("\t\tpostFilter.filter(f);");
            output.Add("\t\t");
            output.Add("\t\tthis.MainModel().Message_Saving();");
            output.Add("\t\ttsUtilities.AjaxData(window.baseURL + \"api/Data/SaveSaved" + classInfo.PascalCaseClassName + "Filter\", ko.toJSON(postFilter), success);");
            output.Add("\t}");

            output.Add("\t/**");
            output.Add("\t * Method fires when the URL action is \"New" + classInfo.PascalCaseClassName + "\"");
            output.Add("\t */");
            output.Add("\tAdd" + classInfo.PascalCaseClassName + "(): void {");
            output.Add("\t\tthis." + classInfo.PascalCaseClassName + "(new " + classInfo.CamelCaseClassName + ");");
            if (primaryKeyType == "Guid") {
                output.Add("\t\tthis." + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "(this.MainModel().GuidEmpty());");
            } else {
                output.Add("\t\tthis." + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "(null);");
            }
            if (props.Any(o => o.PascalCaseVariableName == "TenantId")) {
                output.Add("\t\tthis." + classInfo.PascalCaseClassName + "().tenantId(this.MainModel().TenantId());");
            } else {
                output.Add("\t\t// NO TENANT ID // this." + classInfo.PascalCaseClassName + "().tenantId(this.MainModel().TenantId());");
            }
            output.Add("\t");
            output.Add("\t\t//this.MainModel().UDFFieldsRender(\"edit-" + classInfo.LowerCaseClassName + "-udf-fields\", \"" + classInfo.PascalCaseClassNamePlural + "\", JSON.parse(ko.toJSON(this." + classInfo.PascalCaseClassName + ")));");
            output.Add("\t");
            output.Add("\t\ttsUtilities.DelayedFocus(\"edit-" + classInfo.LowerCaseClassName + "-" + props.Where(o => !o.IsEnum && o.IsOnEFModel).First().LowerCaseVariableName + "\");");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Clears the values for the " + classInfo.CamelCaseClassName + " search filter.");
            output.Add("\t */");
            output.Add("\tClear" + classInfo.PascalCaseClassName + "Filter(): void {");
            output.Add("\t");
            output.Add("\t\tthis.Loading(true);");
            output.Add("\t");
            output.Add("\t\tthis.Filter().keyword(null);");

            foreach (var prop in props.Where(o => !o.IsCollection && !o.IsDictionary && o.PascalCaseVariableName != "TenantId")) {
                if (prop.IsEnum) {
                    output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableNamePlural + "([]);");
                } else if (prop.IsOnEFModel) {
                    if (prop.IsDateTime) {
                        output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "(null);");
                        output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "Start(null);");
                        output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "End(null);");

                    } else if (prop.IsString) {
                        output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "(null);");
                        output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "FilterExact(false);");
                    } else if (prop.PascalCaseVariableName != "TenantId") {
                        output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "(null);");
                    }
                }

            }

            output.Add("\t\tthis.Filter().udf01(null);");
            output.Add("\t\tthis.Filter().udf02(null);");
            output.Add("\t\tthis.Filter().udf03(null);");
            output.Add("\t\tthis.Filter().udf04(null);");
            output.Add("\t\tthis.Filter().udf05(null);");
            output.Add("\t\tthis.Filter().udf06(null);");
            output.Add("\t\tthis.Filter().udf07(null);");
            output.Add("\t\tthis.Filter().udf08(null);");
            output.Add("\t\tthis.Filter().udf09(null);");
            output.Add("\t\tthis.Filter().udf10(null);");
            output.Add("\t\tthis.Filter().page(1);");
            output.Add("\t");
            output.Add("\t\t//TODO: Callback method for more filtering outside of the autos");
            output.Add("\t");
            output.Add("\t\tthis.Loading(false);");
            output.Add("\t\tthis.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Deletes a " + classInfo.CamelCaseClassName + ".");
            output.Add("\t */");
            output.Add("\tDelete" + classInfo.PascalCaseClassName + "(): void {");
            output.Add("\t\tlet success: Function = (data: server.booleanResponse) => {");
            output.Add("\t\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\t\tif (data != null) {");
            output.Add("\t\t\t\tif (data.result) {");
            output.Add("\t\t\t\t\tthis.MainModel().Nav(\"" + classInfo.PascalCaseClassNamePlural + "Auto\");");
            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.MainModel().Message_Errors(data.messages);");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\tthis.MainModel().Message_Error(\"An unknown error occurred attempting to delete the " + classInfo.SentenceCaseClassName + ".\");");
            output.Add("\t\t\t}");
            output.Add("\t\t};");
            output.Add("\t");
            output.Add("\t\tthis.MainModel().Message_Deleting();");
            output.Add("\t\ttsUtilities.AjaxData(window.baseURL + \"api/Data/Delete" + classInfo.PascalCaseClassName + "Auto/\" + this.MainModel().Id(), null, success);");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Method fires when the URL action is \"Edit" + classInfo.PascalCaseClassName + "\"");
            output.Add("\t */");
            output.Add("\tEdit" + classInfo.PascalCaseClassName + "(): void {");
            output.Add("\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\tlet " + camelCasePrimaryKeyString + ": string = this.MainModel().Id();");
            output.Add("\t\tthis." + classInfo.PascalCaseClassName + "(new " + classInfo.CamelCaseClassName + ");");
            output.Add("\t\tthis." + classInfo.PascalCaseClassName + "()." + camelCasePrimaryKeyString + "(null);");
            output.Add("\t");
            output.Add("\t\tif (tsUtilities.HasValue(" + camelCasePrimaryKeyString + ")) {");
            output.Add("\t\t\tlet success: Function = (data: server." + classInfo.CamelCaseClassName + ") => {");
            output.Add("\t\t\t\tif (data != null) {");
            output.Add("\t\t\t\t\tif (data.actionResponse.result) {");
            output.Add("\t\t\t\t\t\tthis." + classInfo.PascalCaseClassName + "().Load(data);");
            output.Add("\t\t\t\t\t\ttsUtilities.DelayedFocus(\"edit-" + classInfo.CamelCaseClassName + "-category\");");
            output.Add("\t");
            output.Add("\t\t\t\t\t\t//this.MainModel().UDFFieldsRender(\"edit-" + classInfo.CamelCaseClassName + "-udf-fields\", \"" + classInfo.PascalCaseClassNamePlural + "\", JSON.parse(ko.toJSON(this." + classInfo.PascalCaseClassName + ")));");
            output.Add("\t");
            output.Add("\t\t\t\t\t\tthis.Loading(false);");
            output.Add("\t\t\t\t\t} else {");
            output.Add("\t\t\t\t\t\tthis.MainModel().Message_Errors(data.actionResponse.messages);");
            output.Add("\t\t\t\t\t}");
            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.MainModel().Message_Error(\"An unknown error occurred attempting to load the " + classInfo.SentenceCaseClassName + " record.\");");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t};");
            output.Add("\t");
            output.Add("\t\t\tthis.Loading(true);");
            output.Add("\t\t\ttsUtilities.AjaxData(window.baseURL + \"api/data/Get" + classInfo.PascalCaseClassName + "Auto/\" + " + camelCasePrimaryKeyString + ", null, success);");
            output.Add("\t\t} else {");
            output.Add("\t\t\tthis.MainModel().Message_Error(\"No valid " + cSharpPrimaryKeyString + " received.\");");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * The callback method used by the paged recordset control to handle the action on the record.");
            output.Add("\t * @param record {server." + classInfo.CamelCaseClassName + "} - The object being passed is a JSON object, not an observable.");
            output.Add("\t */");
            output.Add("\tEdit" + classInfo.PascalCaseClassName + "Callback(record: server." + classInfo.CamelCaseClassName + "): void {");
            //primaryKeyType

            if (primaryKeyType == "Guid") {
                output.Add("\t\tif (record != undefined && record != null && tsUtilities.HasValue(record." + camelCasePrimaryKeyString + ")) {");
                output.Add("\t\t\tthis.MainModel().Nav(\"Edit" + classInfo.PascalCaseClassName + "Auto\", record." + camelCasePrimaryKeyString + ");");
            } else {
                output.Add("\t\tif (record != undefined && record != null && record." + camelCasePrimaryKeyString + " != null) {");
                output.Add("\t\t\tthis.MainModel().Nav(\"Edit" + classInfo.PascalCaseClassName + "Auto\", \"\" + record." + camelCasePrimaryKeyString + ");");
            }
            output.Add("\t\t}");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Called when the " + classInfo.CamelCaseClassName + " filter changes to reload " + classInfo.CamelCaseClassName + " records, unless the filter is changing because");
            output.Add("\t * records are being reloaded.");
            output.Add("\t */");
            output.Add("\tFilterChanged(): void {");
            output.Add("\t\tif (!this.Loading()) {");
            output.Add("\t\t\tthis.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t\t}");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Loads the saved filter that is stored in a cookie as a JSON object.");
            output.Add("\t */");
            output.Add("\tGetSavedFilter(): void {");
            output.Add("\t\tthis.Filter(new filter" + classInfo.PascalCaseClassNamePlural + "Auto);");
            output.Add("\t\tthis.Filter().tenantId(this.MainModel().TenantId());");
            output.Add("\t");
            output.Add("\t\t// maybe this should be specific per tenant?");
            output.Add("\t\tlet savedFilter: string = localStorage.getItem(\"saved-filter-" + classInfo.CamelCaseClassNamePlural + "-\" + this.MainModel().TenantId().toLowerCase());");
            output.Add("\t\tif (tsUtilities.HasValue(savedFilter)) {");
            output.Add("\t\t\tthis.Filter().Load(JSON.parse(savedFilter));");
            output.Add("\t\t\tthis.StartFilterMonitoring();");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t\tthis.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Called when the filter changes or when the page loads to get the " + classInfo.CamelCaseClassNamePlural + " matching the current filter.");
            output.Add("\t */");
            output.Add("\tGet" + classInfo.PascalCaseClassNamePlural + "(): void {");
            output.Add("\t\t// Load the filter");
            output.Add("\t\tthis.Loading(true);");
            output.Add("\t\tif (this.Filter().recordsPerPage() == null || this.Filter().recordsPerPage() == 0) {");
            output.Add("\t\t\tthis.Filter().recordsPerPage(10);");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t\tlet success: Function = (data: server.filter" + classInfo.PascalCaseClassNamePlural + "Auto) => {");
            output.Add("\t\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\t\tif (data != null) {");
            output.Add("\t\t\t\tif (data.actionResponse.result) {");
            output.Add("\t\t\t\t\tthis.Filter().Load(data);");
            output.Add("\t");
            output.Add("\t\t\t\t\tthis.Render" + classInfo.PascalCaseClassName + "Table();");
            output.Add("\t");
            output.Add("\t\t\t\t\tthis.Loading(false);");
            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.MainModel().Message_Errors(data.actionResponse.messages);");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\tthis.MainModel().Message_Error(\"An unknown error occurred attempting to load " + classInfo.SentenceCaseClassName + " records.\");");
            output.Add("\t\t\t}");
            output.Add("\t\t\tthis.Loading(false);");
            output.Add("\t\t};");
            output.Add("\t");
            output.Add("\t");
            output.Add("\t\tlet postFilter: filter" + classInfo.PascalCaseClassNamePlural + "Auto = new filter" + classInfo.PascalCaseClassNamePlural + "Auto();");
            output.Add("\t\tpostFilter.Load(JSON.parse(ko.toJSON(this.Filter)));");
            output.Add("\t\tpostFilter.columns(null);");
            output.Add("\t\tpostFilter.records(null);");
            output.Add("\t");
            output.Add("\t\tlet jsonData: string = ko.toJSON(postFilter);");
            output.Add("\t\tlocalStorage.setItem(\"saved-filter-" + classInfo.CamelCaseClassNamePlural + "-\" + this.MainModel().TenantId().toLowerCase(), jsonData);");
            output.Add("\t\ttsUtilities.AjaxData(window.baseURL + \"api/Data/Get" + classInfo.PascalCaseClassNamePlural + "FilteredAuto/\", jsonData, success);");
            output.Add("\t");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Method handles the callback from the paged recordset control when the page changes, the records per page changes, or when the sort order changes.");
            output.Add("\t * @param type {string} - The type of change event (count, page, or sort)");
            output.Add("\t * @param data {any} - The data passed back, which is a number for count and page and a field column id for the sort.");
            output.Add("\t */");
            output.Add("\tRecordsetCallbackHandler(type: string, data: any): void {");
            output.Add("\t\tconsole.log(\"RecordsetCallbackHandler\", type, data);");
            output.Add("\t\tswitch (type) {");
            output.Add("\t\t\tcase \"count\":");
            output.Add("\t\t\t\twindow." + classInfo.CamelCaseClassNamePlural + "ModelAuto.Filter().recordsPerPage(data);");
            output.Add("\t\t\t\twindow." + classInfo.CamelCaseClassNamePlural + "ModelAuto.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t");
            output.Add("\t\t\tcase \"page\":");
            output.Add("\t\t\t\twindow." + classInfo.CamelCaseClassNamePlural + "ModelAuto.Filter().page(data);");
            output.Add("\t\t\t\twindow." + classInfo.CamelCaseClassNamePlural + "ModelAuto.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t");
            output.Add("\t\t\tcase \"sort\":");
            output.Add("\t\t\t\twindow." + classInfo.CamelCaseClassNamePlural + "ModelAuto.UpdateSort(data);");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t\t}");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Called when the Refresh Filter button is clicked.");
            output.Add("\t */");
            output.Add("\tRefresh" + classInfo.PascalCaseClassName + "Filter(): void {");
            output.Add("\t\tthis.Save" + classInfo.PascalCaseClassName + "Filter();");
            output.Add("\t\tthis.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Renders the paged recordset view. This happens when the filter loads, but also gets called for certain SignalR events");
            output.Add("\t * to update " + classInfo.CamelCaseClassNamePlural + " that might be in the current " + classInfo.CamelCaseClassName + " list.");
            output.Add("\t */");
            output.Add("\tRender" + classInfo.PascalCaseClassName + "Table(): void {");
            output.Add("\t\t// Load records in the pagedRecordset");
            output.Add("\t\tlet f: filter = new filter();");
            output.Add("\t\tf.Load(JSON.parse(ko.toJSON(this.Filter)));");
            output.Add("\t");
            output.Add("\t\tlet records: any = JSON.parse(ko.toJSON(this.Filter().records));");
            output.Add("\t");
            output.Add("\t\tpagedRecordset.Render({");
            output.Add("\t\t\telementId: \"" + classInfo.LowerCaseClassName + "-records-auto\",");
            output.Add("\t\t\tdata: JSON.parse(ko.toJSON(f)),");
            output.Add("\t\t\trecordsetCallbackHandler: (type: string, data: any) => { this.RecordsetCallbackHandler(type, data); },");
            output.Add("\t\t\tactionHandlers: [");
            output.Add("\t\t\t\t{");
            output.Add("\t\t\t\t\tcallbackHandler: (" + classInfo.CamelCaseClassName + ": server." + classInfo.CamelCaseClassName + ") => { this.Edit" + classInfo.PascalCaseClassName + "Callback(" + classInfo.CamelCaseClassName + "); },");
            output.Add("\t\t\t\t\tactionElement: \"<button type='button' class='btn btn-sm btn-primary nowrap'>\" + this.MainModel().IconAndText(\"" + classInfo.Language_ClassNameEditTableButton + "\") + \"</button>\"");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t],");
            output.Add("\t\t\trecordNavigation: \"both\",");
            output.Add("\t\t\t//photoBaseUrl: photoBaseUrl,");
            output.Add("\t\t\tbooleanIcon: this.MainModel().Icon(\"selected\")");
            output.Add("\t\t});");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Saves the current filter as a JSON object in a cookie. The items that aren't needed are nulled out first");
            output.Add("\t * so that the column data and record data are not stored in the cookie.");
            output.Add("\t */");
            output.Add("\tSave" + classInfo.PascalCaseClassName + "Filter(): void {");
            output.Add("\t\tlet saveFilter: filter" + classInfo.PascalCaseClassNamePlural + "Auto = new filter" + classInfo.PascalCaseClassNamePlural + "Auto();");
            output.Add("\t\tsaveFilter.Load(JSON.parse(ko.toJSON(this.Filter)));");
            output.Add("\t\tsaveFilter.actionResponse(null);");
            output.Add("\t\tsaveFilter.columns([]);");
            output.Add("\t\tsaveFilter.records(null);");
            output.Add("\t");
            output.Add("\t\tlocalStorage.setItem(\"saved-filter-" + classInfo.CamelCaseClassNamePlural + "-\" + this.MainModel().TenantId().toLowerCase(), ko.toJSON(saveFilter));");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Saves a " + classInfo.CamelCaseClassName + " record for the " + classInfo.CamelCaseClassName + " currently being added or edited.");
            output.Add("\t */");
            output.Add("\tSave" + classInfo.PascalCaseClassName + "(): void {");
            output.Add("\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\tlet errors: string[] = [];");
            output.Add("\t\tlet focus: string = \"\";");
            output.Add("\t");

            // todo: figure out which ones need to be savd and checked against
            foreach (var prop in props.Where(o => o.IsEfModel && !o.IsEfVariableNullable && !o.IsPrimaryKey && o.PascalCaseVariableName != "ActionResponse")) {

                output.Add("\t//if (!tsUtilities.HasValue(this." + classInfo.PascalCaseClassName + "()." + prop.CamelCaseVariableName + "())) {");
                output.Add("\t//    errors.push(this.MainModel().MissingRequiredField(this.MainModel().Language(\"" + prop.Language_VariableName + "\")));");
                output.Add("\t//    if (focus == \"\") { focus = \"edit-" + classInfo.CamelCaseClassName + "-" + prop.LowerCaseVariableName + "\"; }");
                output.Add("\t//}");
            }

            output.Add("\t");
            output.Add("\t\tif (errors.length > 0) {");
            output.Add("\t\t\tthis.MainModel().Message_Errors(errors);");
            output.Add("\t\t\ttsUtilities.DelayedFocus(focus);");
            output.Add("\t\t\treturn;");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t\tthis.MainModel().UDFFieldsGetValues(\"" + classInfo.PascalCaseClassNamePlural + "\", this." + classInfo.PascalCaseClassName + "());");
            output.Add("\t\tlet json: string = ko.toJSON(this." + classInfo.PascalCaseClassName + ");");
            output.Add("\t");
            output.Add("\t\tlet success: Function = (data: server." + classInfo.CamelCaseClassName + ") => {");
            output.Add("\t\t\tthis.MainModel().Message_Hide();");
            output.Add("\t\t\tif (data != null) {");
            output.Add("\t\t\t\tif (data.actionResponse.result) {");
            output.Add("\t\t\t\t\tthis.MainModel().Nav(\"" + classInfo.PascalCaseClassNamePlural + "Auto\");");
            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.MainModel().Message_Errors(data.actionResponse.messages);");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\tthis.MainModel().Message_Error(\"An unknown error occurred attempting to save this " + classInfo.SentenceCaseClassName + ".\");");
            output.Add("\t\t\t}");
            output.Add("\t\t};");
            output.Add("\t");
            output.Add("\t\tthis.MainModel().Message_Saving();");
            output.Add("\t\ttsUtilities.AjaxData(window.baseURL + \"api/Data/Save" + classInfo.PascalCaseClassName + "Auto\", json, success);");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * This model subscribes to SignalR updates from the MainModel so that " + classInfo.CamelCaseClassNamePlural + " in the " + classInfo.CamelCaseClassName + " filter list");
            output.Add("\t * can be removed or updated when their data changes or they are deleted.");
            output.Add("\t */");
            output.Add("\tSignalrUpdate(): void {");
            output.Add("\t\t//console.log(\"In Tenants, SignalR Update\", JSON.parse(ko.toJSON(this.MainModel().SignalRUpdate)));");
            output.Add("\t\tswitch (this.MainModel().SignalRUpdate().updateTypeString().toLowerCase()) {");
            output.Add("\t\t\tcase \"setting\":");
            output.Add("\t\t\t\tlet " + camelCasePrimaryKeyString + ": string = this.MainModel().SignalRUpdate().itemId();");
            output.Add("\t");
            output.Add("\t\t\t\tswitch (this.MainModel().SignalRUpdate().message().toLowerCase()) {");
            output.Add("\t\t\t\t\tcase \"deleted" + classInfo.CamelCaseClassName + "\":");
            output.Add("\t\t\t\t\t\tlet records: any[] = [];");
            output.Add("\t\t\t\t\t\tif (this.Filter().records() != null && this.Filter().records().length > 0) {");
            output.Add("\t\t\t\t\t\t\tthis.Filter().records().forEach(function (e) {");
            output.Add("\t\t\t\t\t\t\t\tif (e[\"" + camelCasePrimaryKeyString + "\"] != " + camelCasePrimaryKeyString + ") {");
            output.Add("\t\t\t\t\t\t\t\t\trecords.push(e);");
            output.Add("\t\t\t\t\t\t\t\t}");
            output.Add("\t\t\t\t\t\t\t});");
            output.Add("\t\t\t\t\t\t}");
            output.Add("\t\t\t\t\t\tthis.Filter().records(records);");
            output.Add("\t\t\t\t\t\tthis.Render" + classInfo.PascalCaseClassName + "Table();");
            output.Add("\t");
            output.Add("\t\t\t\t\t\tbreak;");
            output.Add("\t");
            output.Add("\t\t\t\t\tcase \"saved" + classInfo.CamelCaseClassName + "\":");
            output.Add("\t\t\t\t\t\tlet " + classInfo.CamelCaseClassName + "Data: any = this.MainModel().SignalRUpdate().object();");
            output.Add("\t");
            output.Add("\t\t\t\t\t\tlet index: number = -1;");
            output.Add("\t\t\t\t\t\tlet indexItem: number = -1;");
            output.Add("\t\t\t\t\t\tif (this.Filter().records() != null && this.Filter().records().length > 0) {");
            output.Add("\t\t\t\t\t\t\tthis.Filter().records().forEach(function (e) {");
            output.Add("\t\t\t\t\t\t\t\tindex++;");
            output.Add("\t\t\t\t\t\t\t\tif (e[\"" + camelCasePrimaryKeyString + "\"] == " + camelCasePrimaryKeyString + ") {");
            output.Add("\t\t\t\t\t\t\t\t\tindexItem = index;");
            output.Add("\t\t\t\t\t\t\t\t}");
            output.Add("\t\t\t\t\t\t\t});");
            output.Add("\t\t\t\t\t\t}");
            output.Add("\t");
            output.Add("\t\t\t\t\t\tif (indexItem > -1) {");
            output.Add("\t\t\t\t\t\t\tthis.Filter().records()[indexItem] = JSON.parse(" + classInfo.CamelCaseClassName + "Data);");
            output.Add("\t\t\t\t\t\t\tthis.Render" + classInfo.PascalCaseClassName + "Table();");
            output.Add("\t\t\t\t\t\t}");
            output.Add("\t\t\t\t}");
            output.Add("\t");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t\t}");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Starts observing changes to the filter elements to call FilterChanged when selections are changed.");
            output.Add("\t */");
            output.Add("\tStartFilterMonitoring(): void {");
            output.Add("\t\t// Subscribe to filter changed");

            output.Add("\t\tthis.Filter().keyword.subscribe(() => { this.FilterChanged(); });     " +
                "                                                                                        ");
            // do the enums
            foreach (var prop in props.Where(o => o.IsEnum && o.IsOnEFModel && !o.IsCollection && !o.IsDictionary)) {
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableNamePlural + ".subscribe(() => { this.FilterChanged(); });");
            }
            // first do the dates, they are special
            foreach (var prop in props.Where(o => o.IsDateTime && o.IsOnEFModel && !o.IsCollection && !o.IsDictionary)) {
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + ".subscribe(() => { this.FilterChanged(); });");
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "Start.subscribe(() => { this.FilterChanged(); });");
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "End.subscribe(() => { this.FilterChanged(); });");
            }

            // now the strings and their checkbox counterparts
            foreach (var prop in props.Where(o => o.IsString && o.IsOnEFModel && !o.IsCollection && !o.IsDictionary)) {
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + ".subscribe(() => { this.FilterChanged(); });");
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + "FilterExact.subscribe(() => { this.FilterChanged(); });");
            }

            // now everything else
            foreach (var prop in props.Where(o => !o.IsEnum && !o.IsDateTime && !o.IsString && o.IsOnEFModel && !o.IsCollection && !o.IsDictionary)) {
                output.Add("\t\tthis.Filter()." + prop.CamelCaseVariableName + ".subscribe(() => { this.FilterChanged(); });");
            }

            output.Add("\t\tthis.Filter().udf01.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf02.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf03.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf04.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf05.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf06.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf07.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf08.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf09.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t\tthis.Filter().udf10.subscribe(() => { this.FilterChanged(); });");
            output.Add("\t}");
            output.Add("\t");

            output.Add("\t");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Handles changing the sort order and updating the filter.");
            output.Add("\t * @param dataElementName");
            output.Add("\t */");
            output.Add("\tUpdateSort(dataElementName: string): void {");
            output.Add("\t\tlet currentSort: string = this.Filter().sort();");
            output.Add("\t\tif (tsUtilities.HasValue(currentSort)) {");
            output.Add("\t\t\tcurrentSort = currentSort.toLowerCase();");
            output.Add("\t\t} else {");
            output.Add("\t\t\tcurrentSort = \"\";");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t\tlet currentDirection: string = this.Filter().sortOrder();");
            output.Add("\t\tif (tsUtilities.HasValue(currentDirection)) {");
            output.Add("\t\t\tcurrentDirection = currentDirection.toUpperCase();");
            output.Add("\t\t}");
            output.Add("\t");
            output.Add("\t\tif (tsUtilities.HasValue(dataElementName)) {");
            output.Add("\t\t\tif (currentSort.toLowerCase() == dataElementName.toLowerCase()) {");
            output.Add("\t\t\t\tif (currentDirection == \"ASC\") {");
            output.Add("\t\t\t\t\tthis.Filter().sortOrder(\"DESC\");");
            output.Add("\t\t\t\t} else {");
            output.Add("\t\t\t\t\tthis.Filter().sortOrder(\"ASC\");");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t} else {");
            output.Add("\t\t\t\tthis.Filter().sort(dataElementName);");
            output.Add("\t\t\t\tswitch (dataElementName.toLowerCase()) {");
            output.Add("\t\t\t\t\tcase \"modified\":");
            output.Add("\t\t\t\t\t\tthis.Filter().sortOrder(\"DESC\");");
            output.Add("\t\t\t\t\t\tbreak;");
            output.Add("\t");
            output.Add("\t\t\t\t\tdefault:");
            output.Add("\t\t\t\t\t\tthis.Filter().sortOrder(\"ASC\");");
            output.Add("\t\t\t\t\t\tbreak;");
            output.Add("\t\t\t\t}");
            output.Add("\t\t\t}");
            output.Add("\t\t\tthis.Get" + classInfo.PascalCaseClassNamePlural + "();");
            output.Add("\t\t}");
            output.Add("\t}");
            output.Add("\t");
            output.Add("\t/**");
            output.Add("\t * Called when the view changes in the MainModel to do any necessary work in this viewModel.");
            output.Add("\t */");
            output.Add("\tViewChanged(): void {");
            output.Add("\t\tthis.Loading(false);");
            output.Add("\t");
            output.Add("\t\tswitch (this.MainModel().CurrentView().toLowerCase()) {");
            output.Add("\t\t\tcase \"edit" + classInfo.LowerCaseClassName + "auto\":");
            output.Add("\t\t\t\tthis.Edit" + classInfo.PascalCaseClassName + "();");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t");
            output.Add("\t\t\tcase \"new" + classInfo.LowerCaseClassName + "auto\":");
            output.Add("\t\t\t\tthis.Add" + classInfo.PascalCaseClassName + "();");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t");
            output.Add("\t\t\tcase \"" + classInfo.LowerCaseClassNamePlural + "auto\":");
            output.Add("\t\t\t\tthis.GetSavedFilter();");
            output.Add("\t\t\t\tthis.Filter().tenantId(this.MainModel().TenantId());");
            output.Add("\t\t\t\tbreak;");
            output.Add("\t\t}");
            output.Add("\t}");
            output.Add("}");

            return output;
        }
    }
}