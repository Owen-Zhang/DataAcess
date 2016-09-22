using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DataAccess
{        
    /// <summary>
    /// A type handler for data-types that are supported by the underlying provider, but which need
    /// a well-known UdtTypeName to be specified
    /// </summary>
    public class UdtTypeHandler : ITypeHandler
    {
        private readonly string udtTypeName;
        /// <summary>
        /// Creates a new instance of UdtTypeHandler with the specified UdtTypeName
        /// </summary>
        public UdtTypeHandler(string udtTypeName)
        {
            if (string.IsNullOrEmpty(udtTypeName)) throw new ArgumentException("Cannot be null or empty", udtTypeName);
            this.udtTypeName = udtTypeName;
        }
        object ITypeHandler.Parse(Type destinationType, object value)
        {
            return value is DBNull ? null : value;
        }

        void ITypeHandler.SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = SqlMapper.SanitizeParameterValue(value);
            if (parameter is System.Data.SqlClient.SqlParameter && !(value is DBNull))
            {
                ((System.Data.SqlClient.SqlParameter)parameter).SqlDbType = SqlDbType.Udt;
                ((System.Data.SqlClient.SqlParameter)parameter).UdtTypeName = udtTypeName;
            }
        }
    }

    /// <summary>
    /// Base-class for simple type-handlers
    /// </summary>
    public abstract class TypeHandler<T> : ITypeHandler
    {
        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        public abstract void SetValue(IDbDataParameter parameter, T value);

        /// <summary>
        /// Parse a database value back to a typed value
        /// </summary>
        /// <param name="value">The value from the database</param>
        /// <returns>The typed value</returns>
        public abstract T Parse(object value);

        void ITypeHandler.SetValue(IDbDataParameter parameter, object value)
        {
            if (value is DBNull)
            {
                parameter.Value = value;
            }
            else
            {
                SetValue(parameter, (T)value);
            }
        }

        object ITypeHandler.Parse(Type destinationType, object value)
        {
            return Parse(value);
        }
    }

    sealed class SqlDataRecordHandler : ITypeHandler
    {
        public object Parse(Type destinationType, object value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {
            SqlDataRecordListTVPParameter.Set(parameter, value as IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord>, null);
        }
    }

    /// <summary>
    /// Used to pass a IEnumerable&lt;SqlDataRecord&gt; as a SqlDataRecordListTVPParameter
    /// </summary>
    sealed partial class SqlDataRecordListTVPParameter : ICustomQueryParameter
    {
        private readonly IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> data;
        private readonly string typeName;
        /// <summary>
        /// Create a new instance of SqlDataRecordListTVPParameter
        /// </summary>
        public SqlDataRecordListTVPParameter(IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> data, string typeName)
        {
            this.data = data;
            this.typeName = typeName;
        }
        static readonly Action<System.Data.SqlClient.SqlParameter, string> setTypeName;
        static SqlDataRecordListTVPParameter()
        {
            var prop = typeof(System.Data.SqlClient.SqlParameter).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.PropertyType == typeof(string) && prop.CanWrite)
            {
                setTypeName = (Action<System.Data.SqlClient.SqlParameter, string>)
                    Delegate.CreateDelegate(typeof(Action<System.Data.SqlClient.SqlParameter, string>), prop.GetSetMethod());
            }
        }
        void ICustomQueryParameter.AddParameter(IDbCommand command, string name)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            Set(param, data, typeName);
            command.Parameters.Add(param);
        }
        internal static void Set(IDbDataParameter parameter, IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord> data, string typeName)
        {
            parameter.Value = (object)data ?? DBNull.Value;
            var sqlParam = parameter as System.Data.SqlClient.SqlParameter;
            if (sqlParam != null)
            {
                sqlParam.SqlDbType = SqlDbType.Structured;
                sqlParam.TypeName = typeName;
            }
        }
    }

    /// <summary>
    /// Used to pass a DataTable as a TableValuedParameter
    /// </summary>
    sealed partial class TableValuedParameter : ICustomQueryParameter
    {
        private readonly DataTable table;
        private readonly string typeName;

        /// <summary>
        /// Create a new instance of TableValuedParameter
        /// </summary>
        public TableValuedParameter(DataTable table) : this(table, null) { }
        /// <summary>
        /// Create a new instance of TableValuedParameter
        /// </summary>
        public TableValuedParameter(DataTable table, string typeName)
        {
            this.table = table;
            this.typeName = typeName;
        }
        static readonly Action<System.Data.SqlClient.SqlParameter, string> setTypeName;
        static TableValuedParameter()
        {
            var prop = typeof(System.Data.SqlClient.SqlParameter).GetProperty("TypeName", BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.PropertyType == typeof(string) && prop.CanWrite)
            {
                setTypeName = (Action<System.Data.SqlClient.SqlParameter, string>)
                    Delegate.CreateDelegate(typeof(Action<System.Data.SqlClient.SqlParameter, string>), prop.GetSetMethod());
            }
        }
        void ICustomQueryParameter.AddParameter(IDbCommand command, string name)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            Set(param, table, typeName);
            command.Parameters.Add(param);
        }
        internal static void Set(IDbDataParameter parameter, DataTable table, string typeName)
        {
            parameter.Value = SqlMapper.SanitizeParameterValue(table);
            if (string.IsNullOrEmpty(typeName) && table != null)
            {
                typeName = SqlMapper.GetTypeName(table);
            }
            if (!string.IsNullOrEmpty(typeName))
            {
                var sqlParam = parameter as System.Data.SqlClient.SqlParameter;
                if (sqlParam != null)
                {
                    if (setTypeName != null) setTypeName(sqlParam, typeName);
                    sqlParam.SqlDbType = SqlDbType.Structured;
                }
            }
        }
    }
    /// <summary>
    /// This class represents a SQL string, it can be used if you need to denote your parameter is a Char vs VarChar vs nVarChar vs nChar
    /// </summary>
    internal sealed partial class DbString : ICustomQueryParameter
    {
        /// <summary>
        /// Default value for IsAnsi.
        /// </summary>
        public static bool IsAnsiDefault { get; set; }

        /// <summary>
        /// A value to set the default value of strings
        /// going through Dapper. Default is 4000, any value larger than this
        /// field will not have the default value applied.
        /// </summary>
        public const int DefaultLength = 4000;

        /// <summary>
        /// Create a new DbString
        /// </summary>
        public DbString()
        {
            Length = -1;
            IsAnsi = IsAnsiDefault;
        }
        /// <summary>
        /// Ansi vs Unicode 
        /// </summary>
        public bool IsAnsi { get; set; }
        /// <summary>
        /// Fixed length 
        /// </summary>
        public bool IsFixedLength { get; set; }
        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// The value of the string
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Add the parameter to the command... internal use only
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddParameter(IDbCommand command, string name)
        {
            if (IsFixedLength && Length == -1)
            {
                throw new InvalidOperationException("If specifying IsFixedLength,  a Length must also be specified");
            }
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = SqlMapper.SanitizeParameterValue(Value);
            if (Length == -1 && Value != null && Value.Length <= DefaultLength)
            {
                param.Size = DefaultLength;
            }
            else
            {
                param.Size = Length;
            }
            param.DbType = IsAnsi ? (IsFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString) : (IsFixedLength ? DbType.StringFixedLength : DbType.String);
            command.Parameters.Add(param);
        }
    }

    /// <summary>
    /// Handles variances in features per DBMS
    /// </summary>
    partial class FeatureSupport
    {
        private static readonly FeatureSupport
            @default = new FeatureSupport(false),
            postgres = new FeatureSupport(true);

        /// <summary>
        /// Gets the feature set based on the passed connection
        /// </summary>
        public static FeatureSupport Get(IDbConnection connection)
        {
            string name = connection == null ? null : connection.GetType().Name;
            if (string.Equals(name, "npgsqlconnection", StringComparison.OrdinalIgnoreCase)) return postgres;
            return @default;
        }
        private FeatureSupport(bool arrays)
        {
            Arrays = arrays;
        }
        /// <summary>
        /// True if the db supports array columns e.g. Postgresql
        /// </summary>
        public bool Arrays { get; private set; }
    }

    /// <summary>
    /// Represents simple member map for one of target parameter or property or field to source DataReader column
    /// </summary>
    sealed partial class SimpleMemberMap : IMemberMap
    {
        private readonly string _columnName;
        private readonly PropertyInfo _property;
        private readonly FieldInfo _field;
        private readonly ParameterInfo _parameter;

        /// <summary>
        /// Creates instance for simple property mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="property">Target property</param>
        public SimpleMemberMap(string columnName, PropertyInfo property)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (property == null)
                throw new ArgumentNullException("property");

            _columnName = columnName;
            _property = property;
        }

        /// <summary>
        /// Creates instance for simple field mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="field">Target property</param>
        public SimpleMemberMap(string columnName, FieldInfo field)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (field == null)
                throw new ArgumentNullException("field");

            _columnName = columnName;
            _field = field;
        }

        /// <summary>
        /// Creates instance for simple constructor parameter mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="parameter">Target constructor parameter</param>
        public SimpleMemberMap(string columnName, ParameterInfo parameter)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (parameter == null)
                throw new ArgumentNullException("parameter");

            _columnName = columnName;
            _parameter = parameter;
        }

        /// <summary>
        /// DataReader column name
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
        }

        /// <summary>
        /// Target member type
        /// </summary>
        public Type MemberType
        {
            get
            {
                if (_field != null)
                    return _field.FieldType;

                if (_property != null)
                    return _property.PropertyType;

                if (_parameter != null)
                    return _parameter.ParameterType;

                return null;
            }
        }

        /// <summary>
        /// Target property
        /// </summary>
        public PropertyInfo Property
        {
            get { return _property; }
        }

        /// <summary>
        /// Target field
        /// </summary>
        public FieldInfo Field
        {
            get { return _field; }
        }

        /// <summary>
        /// Target constructor parameter
        /// </summary>
        public ParameterInfo Parameter
        {
            get { return _parameter; }
        }
    }

    /// <summary>
    /// Represents default type mapping strategy used by Dapper
    /// </summary>
    sealed partial class DefaultTypeMap : ITypeMap
    {
        private readonly List<FieldInfo> _fields;
        private readonly List<PropertyInfo> _properties;
        private readonly Type _type;

        /// <summary>
        /// Creates default type map
        /// </summary>
        /// <param name="type">Entity type</param>
        public DefaultTypeMap(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _fields = GetSettableFields(type);
            _properties = GetSettableProps(type);
            _type = type;
        }
        internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
        {
            if (propertyInfo.DeclaringType == type) return propertyInfo.GetSetMethod(true);
            return propertyInfo.DeclaringType.GetProperty(
                   propertyInfo.Name,
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                   Type.DefaultBinder,
                   propertyInfo.PropertyType,
                   propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray(),
                   null).GetSetMethod(true);
        }

        internal static List<PropertyInfo> GetSettableProps(Type t)
        {
            return t
                  .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                  .Where(p => GetPropertySetter(p, t) != null)
                  .ToList();
        }

        internal static List<FieldInfo> GetSettableFields(Type t)
        {
            return t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        }

        /// <summary>
        /// Finds best constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Matching constructor or default one</returns>
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            var constructors = _type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (ConstructorInfo ctor in constructors.OrderBy(c => c.IsPublic ? 0 : (c.IsPrivate ? 2 : 1)).ThenBy(c => c.GetParameters().Length))
            {
                ParameterInfo[] ctorParameters = ctor.GetParameters();
                if (ctorParameters.Length == 0)
                    return ctor;

                if (ctorParameters.Length != types.Length)
                    continue;

                int i = 0;
                for (; i < ctorParameters.Length; i++)
                {
                    if (!String.Equals(ctorParameters[i].Name, names[i], StringComparison.OrdinalIgnoreCase))
                        break;
                    if (types[i] == typeof(byte[]) && ctorParameters[i].ParameterType.FullName == SqlMapper.LinqBinary)
                        continue;
                    var unboxedType = Nullable.GetUnderlyingType(ctorParameters[i].ParameterType) ?? ctorParameters[i].ParameterType;
                    if (unboxedType != types[i]
                        && !(unboxedType.IsEnum() && Enum.GetUnderlyingType(unboxedType) == types[i])
                        && !(unboxedType == typeof(char) && types[i] == typeof(string))
                        && !(unboxedType.IsEnum() && types[i] == typeof(string)))
                    {
                        break;
                    }
                }

                if (i == ctorParameters.Length)
                    return ctor;
            }

            return null;
        }

        /// <summary>
        /// Returns the constructor, if any, that has the ExplicitConstructorAttribute on it.
        /// </summary>
        public ConstructorInfo FindExplicitConstructor()
        {
            var constructors = _type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var withAttr = constructors.Where(c => c.GetCustomAttributes(typeof(ExplicitConstructorAttribute), true).Length > 0).ToList();

            if (withAttr.Count == 1)
            {
                return withAttr[0];
            }

            return null;
        }

        /// <summary>
        /// Gets mapping for constructor parameter
        /// </summary>
        /// <param name="constructor">Constructor to resolve</param>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            var parameters = constructor.GetParameters();

            return new SimpleMemberMap(columnName, parameters.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Gets member mapping for column
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public IMemberMap GetMember(string columnName)
        {
            var property = _properties.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
               ?? _properties.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (property == null && MatchNamesWithUnderscores)
            {
                property = _properties.FirstOrDefault(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.Ordinal))
                    ?? _properties.FirstOrDefault(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
            }

            if (property != null)
                return new SimpleMemberMap(columnName, property);

            var field = _fields.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
               ?? _fields.FirstOrDefault(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (field == null && MatchNamesWithUnderscores)
            {
                field = _fields.FirstOrDefault(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.Ordinal))
                    ?? _fields.FirstOrDefault(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
            }

            if (field != null)
                return new SimpleMemberMap(columnName, field);

            return null;
        }
        /// <summary>
        /// Should column names like User_Id be allowed to match properties/fields like UserId ?
        /// </summary>
        public static bool MatchNamesWithUnderscores { get; set; }
    }

    /// <summary>
    /// Implements custom property mapping by user provided criteria (usually presence of some custom attribute with column to member mapping)
    /// </summary>
    sealed partial class CustomPropertyTypeMap : ITypeMap
    {
        private readonly Type _type;
        private readonly Func<Type, string, PropertyInfo> _propertySelector;

        /// <summary>
        /// Creates custom property mapping
        /// </summary>
        /// <param name="type">Target entity type</param>
        /// <param name="propertySelector">Property selector based on target type and DataReader column name</param>
        public CustomPropertyTypeMap(Type type, Func<Type, string, PropertyInfo> propertySelector)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            _type = type;
            _propertySelector = propertySelector;
        }

        /// <summary>
        /// Always returns default constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Default constructor</returns>
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _type.GetConstructor(new Type[0]);
        }

        /// <summary>
        /// Always returns null
        /// </summary>
        /// <returns></returns>
        public ConstructorInfo FindExplicitConstructor()
        {
            return null;
        }

        /// <summary>
        /// Not implemented as far as default constructor used for all cases
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns property based on selector strategy
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Poperty member map</returns>
        public IMemberMap GetMember(string columnName)
        {
            var prop = _propertySelector(_type, columnName);
            return prop != null ? new SimpleMemberMap(columnName, prop) : null;
        }
    }

    internal class WrappedReader : IDataReader, IWrappedDataReader
    {
        private IDataReader reader;
        private IDbCommand cmd;

        public IDataReader Reader
        {
            get
            {
                var tmp = reader;
                if (tmp == null) throw new ObjectDisposedException(GetType().Name);
                return tmp;
            }
        }
        IDbCommand IWrappedDataReader.Command
        {
            get
            {
                var tmp = cmd;
                if (tmp == null) throw new ObjectDisposedException(GetType().Name);
                return tmp;
            }
        }
        public WrappedReader(IDbCommand cmd, IDataReader reader)
        {
            this.cmd = cmd;
            this.reader = reader;
        }

        void IDataReader.Close()
        {
            if (reader != null) reader.Close();
        }

        int IDataReader.Depth
        {
            get { return Reader.Depth; }
        }

        DataTable IDataReader.GetSchemaTable()
        {
            return Reader.GetSchemaTable();
        }

        bool IDataReader.IsClosed
        {
            get { return reader == null ? true : reader.IsClosed; }
        }

        bool IDataReader.NextResult()
        {
            return Reader.NextResult();
        }

        bool IDataReader.Read()
        {
            return Reader.Read();
        }

        int IDataReader.RecordsAffected
        {
            get { return Reader.RecordsAffected; }
        }

        void IDisposable.Dispose()
        {
            if (reader != null) reader.Close();
            if (reader != null) reader.Dispose();
            reader = null;
            if (cmd != null) cmd.Dispose();
            cmd = null;
        }

        int IDataRecord.FieldCount
        {
            get { return Reader.FieldCount; }
        }

        bool IDataRecord.GetBoolean(int i)
        {
            return Reader.GetBoolean(i);
        }

        byte IDataRecord.GetByte(int i)
        {
            return Reader.GetByte(i);
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return Reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        char IDataRecord.GetChar(int i)
        {
            return Reader.GetChar(i);
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return Reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        IDataReader IDataRecord.GetData(int i)
        {
            return Reader.GetData(i);
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            return Reader.GetDataTypeName(i);
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            return Reader.GetDateTime(i);
        }

        decimal IDataRecord.GetDecimal(int i)
        {
            return Reader.GetDecimal(i);
        }

        double IDataRecord.GetDouble(int i)
        {
            return Reader.GetDouble(i);
        }

        Type IDataRecord.GetFieldType(int i)
        {
            return Reader.GetFieldType(i);
        }

        float IDataRecord.GetFloat(int i)
        {
            return Reader.GetFloat(i);
        }

        Guid IDataRecord.GetGuid(int i)
        {
            return Reader.GetGuid(i);
        }

        short IDataRecord.GetInt16(int i)
        {
            return Reader.GetInt16(i);
        }

        int IDataRecord.GetInt32(int i)
        {
            return Reader.GetInt32(i);
        }

        long IDataRecord.GetInt64(int i)
        {
            return Reader.GetInt64(i);
        }

        string IDataRecord.GetName(int i)
        {
            return Reader.GetName(i);
        }

        int IDataRecord.GetOrdinal(string name)
        {
            return Reader.GetOrdinal(name);
        }

        string IDataRecord.GetString(int i)
        {
            return Reader.GetString(i);
        }

        object IDataRecord.GetValue(int i)
        {
            return Reader.GetValue(i);
        }

        int IDataRecord.GetValues(object[] values)
        {
            return Reader.GetValues(values);
        }

        bool IDataRecord.IsDBNull(int i)
        {
            return Reader.IsDBNull(i);
        }

        object IDataRecord.this[string name]
        {
            get { return Reader[name]; }
        }

        object IDataRecord.this[int i]
        {
            get { return Reader[i]; }
        }
    }

    /// <summary>
    /// Tell Dapper to use an explicit constructor, passing nulls or 0s for all parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class ExplicitConstructorAttribute : Attribute
    {

    }
}
