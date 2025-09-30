using SWM.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SWM.Data.Repositories
{
    public class SupplierRepository : BaseRepository
    {
        public SupplierRepository(string connectionString) : base(connectionString) { }

        public List<Supplier> GetActiveSuppliers()
        {
            var suppliers = new List<Supplier>();
            var sql = "SELECT * FROM Suppliers WHERE IsActive = 1 ORDER BY SupplierName";

            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    suppliers.Add(new Supplier
                    {
                        SupplierID = Convert.ToInt32(reader["SupplierID"]),
                        SupplierName = reader["SupplierName"].ToString(),
                        ContactPerson = reader["ContactPerson"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Address = reader["Address"]?.ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"])
                    });
                }
            }
            return suppliers;
        }
    }
}