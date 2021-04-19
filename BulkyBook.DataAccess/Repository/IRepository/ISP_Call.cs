using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    // Repository for Stored procedures
    public interface ISP_Call : IDisposable
    {
        // using Dapper to pass parameters
        // Single<T> uses Execute Scalar that returns int or bool (expl - result first column in first row)
        T Single<T>(string procedureName, DynamicParameters param = null);

        void Execute(string procedureName, DynamicParameters param = null);

        // Retrieve complete row
        T OneRecord<T>(string procedureName, DynamicParameters param = null);

        // Get All rows
        IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null);

        // Return two tables
        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null);

    }
}
