using System;
using System.Data;
using System.Reflection;

namespace DataAccess
{
    /// <summary>
    /// Implement this interface to pass an arbitrary db specific parameter to Dapper
    /// </summary>
    [AssemblyNeutral]
    public interface ICustomQueryParameter
    {
        /// <summary>
        /// Add the parameter needed to the command before it executes
        /// </summary>
        /// <param name="command">The raw command prior to execution</param>
        /// <param name="name">Parameter name</param>
        void AddParameter(IDbCommand command, string name);
    }

    /// <summary>
    /// Implement this interface to perform custom type-based parameter handling and value parsing
    /// </summary>
    [AssemblyNeutral]
    public interface ITypeHandler
    {
        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        void SetValue(IDbDataParameter parameter, object value);

        /// <summary>
        /// Parse a database value back to a typed value
        /// </summary>
        /// <param name="value">The value from the database</param>
        /// <param name="destinationType">The type to parse to</param>
        /// <returns>The typed value</returns>
        object Parse(Type destinationType, object value);
    }

    /// <summary>
    /// Implement this interface to pass an arbitrary db specific set of parameters to Dapper
    /// </summary>
    public interface IDynamicParameters
    {
        /// <summary>
        /// Add all the parameters needed to the command just before it executes
        /// </summary>
        /// <param name="command">The raw command prior to execution</param>
        /// <param name="identity">Information about the query</param>
        void AddParameters(IDbCommand command, Identity identity);
    }

    /// <summary>
    /// Extends IDynamicParameters providing by-name lookup of parameter values
    /// </summary>
    public interface IParameterLookup : IDynamicParameters
    {
        /// <summary>
        /// Get the value of the specified parameter (return null if not found)
        /// </summary>
        object this[string name] { get; }
    }

    /// <summary>
    /// Extends IDynamicParameters with facilities for executing callbacks after commands have completed
    /// </summary>
    public interface IParameterCallbacks : IDynamicParameters
    {
        /// <summary>
        /// Invoked when the command has executed
        /// </summary>
        void OnCompleted();
    }

    /// <summary>
    /// Implement this interface to change default mapping of reader columns to type members
    /// </summary>
    public interface ITypeMap
    {
        /// <summary>
        /// Finds best constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Matching constructor or default one</returns>
        ConstructorInfo FindConstructor(string[] names, Type[] types);

        /// <summary>
        /// Returns a constructor which should *always* be used.
        /// 
        /// Parameters will be default values, nulls for reference types and zero'd for value types.
        /// 
        /// Use this class to force object creation away from parameterless constructors you don't control.
        /// </summary>
        ConstructorInfo FindExplicitConstructor();

        /// <summary>
        /// Gets mapping for constructor parameter
        /// </summary>
        /// <param name="constructor">Constructor to resolve</param>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName);

        /// <summary>
        /// Gets member mapping for column
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        IMemberMap GetMember(string columnName);
    }

    /// <summary>
    /// Implements this interface to provide custom member mapping
    /// </summary>
    public interface IMemberMap
    {
        /// <summary>
        /// Source DataReader column name
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        ///  Target member type
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// Target property
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Target field
        /// </summary>
        FieldInfo Field { get; }

        /// <summary>
        /// Target constructor parameter
        /// </summary>
        ParameterInfo Parameter { get; }
    }

    /// <summary>
    /// Describes a reader that controls the lifetime of both a command and a reader,
    /// exposing the downstream command/reader as properties.
    /// </summary>
    public interface IWrappedDataReader : IDataReader
    {
        /// <summary>
        /// Obtain the underlying reader
        /// </summary>
        IDataReader Reader { get; }
        /// <summary>
        /// Obtain the underlying command
        /// </summary>
        IDbCommand Command { get; }
    }
}
