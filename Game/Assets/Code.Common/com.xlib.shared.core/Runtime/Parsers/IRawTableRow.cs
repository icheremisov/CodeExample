using System;
using System.Collections.Generic;

namespace XLib.Core.Parsers {

	public interface IRawTableRow {

		/// <summary>
		///     current location string for error messages
		/// </summary>
		string Location { get; }

		/// <summary>
		///     all header names
		/// </summary>
		IEnumerable<string> Headers { get; }

		/// <summary>
		///     check if specified header is present
		/// </summary>
		bool HasColumnInHeader(string columnName);

		/// <summary>
		///     check if row is fully empty
		/// </summary>
		bool IsEmpty();

		/// <summary>
		///     check if column value is empty
		/// </summary>
		bool IsEmpty(string columnName);

		/// <summary>
		///     get value as-is. return null in case of any error
		/// </summary>
		string RawValue(string columnName);

		/// <summary>
		///     get value as-is. return false in case of any error
		/// </summary>
		bool RawValue(string columnName, out string value);

		/// <summary>
		///     format error with valid line index and table name
		///     usage:
		///     throw row.MakeError("column 'Id' is not valid!");
		/// </summary>
		Exception MakeError(string message);

	}

}