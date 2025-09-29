using System.Data.SQLite;
using System.Collections.Generic;
using SWM.Core.Models;

namespace SWM.Data.Repositories
{
    public class SupplierRepository : BaseRepository<Supplier>
    {
        public SupplierRepository(string connectionString) : base(connectionString) { }

        public List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Suppliers ORDER BY Name";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(new Supplier
                        {
                            SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            ContactDetails = reader.GetString(reader.GetOrdinal("ContactDetails"))
                        });
                    }
                }
            }

            return suppliers;
        }

        public void CreateSupplier(Supplier supplier)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Suppliers (Name, ContactDetails) VALUES (@Name, @ContactDetails)";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", supplier.Name);
                    command.Parameters.AddWithValue("@ContactDetails", supplier.ContactDetails);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}