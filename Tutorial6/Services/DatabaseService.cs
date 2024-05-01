using Microsoft.Data.SqlClient;
using System.Data;
using Tutorial6.Dto;

namespace Tutorial6.Services
{
    public class DatabaseService : IDatabaseService
    {
        private string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public bool CheckData(int IdProduct, int IdWarehouse, int Amount)
        {
            var warehouse = new List<Warehouse>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;
            connection.Open();

            cmd.CommandText = "SELECT IdProduct,IdWarehouse FROM Product,Warehouse;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                warehouse.Add(new Warehouse()
                {
                    IdProduct = reader.GetInt32("IdProduct"),
                    IdWarehouse = reader.GetInt32("IdWarehouse")
                });
            }

            Warehouse result = warehouse.SingleOrDefault(x => x.IdProduct == IdProduct && x.IdWarehouse == IdWarehouse);

            if (result == null || Amount <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckOrder(int IdProduct, int Amount, DateTime CreatedAt)
        {
            var warehouse = new List<Warehouse>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;
            connection.Open();

            cmd.CommandText = "SELECT IdProduct, Amount, CreatedAt FROM [Order];";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                warehouse.Add(new Warehouse()
                {
                    IdProduct = reader.GetInt32("IdProduct"),
                    Amount = reader.GetInt32("Amount"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            Warehouse result = warehouse.SingleOrDefault(x => x.IdProduct == IdProduct && x.Amount == Amount && x.CreatedAt < CreatedAt);

            if (result == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IfCompleted(int IdProduct)
        {
            var warehouse = new List<Warehouse>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;
            connection.Open();

            cmd.CommandText = "SELECT P.IdProduct, P.IdOrder FROM Product_Warehouse P INNER JOIN [Order] O on P.IdOrder = O.IdOrder WHERE P.IdProduct = " + IdProduct + ";";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                warehouse.Add(new Warehouse()
                {
                    IdProduct = reader.GetInt32("IdProduct"),
                });
            }

            Warehouse result = warehouse.SingleOrDefault(x => x.IdProduct == IdProduct);

            if (result == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateFullfill(int IdProduct)
        {
            var warehouse = new List<Warehouse>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;
            connection.Open();

            cmd.CommandText = "UPDATE [Order] SET FulfilledAt = " + "'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + "'" + " WHERE IdProduct = " + IdProduct + ";";

            using var reader = cmd.ExecuteReader();
        }

        public void RegisterProduct(int IdProduct, int IdWarehouse, int Amount, DateTime CreatedAt)
        {
            var test = new List<int>();

            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;
            connection.Open();

            cmd.CommandText = "SELECT P.IdProduct, P.IdOrder FROM Product_Warehouse P INNER JOIN [Order] O on P.IdOrder = O.IdOrder WHERE P.IdProduct = " + IdProduct + ";";

            using var reader = cmd.ExecuteReader();

            int IdOrder;
            while (reader.Read())
            {
                test.Add(IdOrder = reader.GetInt32("P.IdOrder"));
            }
            connection.Close();

            cmd.Connection = connection;
            connection.Open();

            cmd.CommandText = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, Price*Amount, '@CreatedAt)';";
            cmd.Parameters.AddWithValue("@IdWarehouse", IdWarehouse);
            cmd.Parameters.AddWithValue("@IdProduct", IdProduct);
            cmd.Parameters.AddWithValue("@IdOrder", test[0]);
            cmd.Parameters.AddWithValue("@Amount", Amount);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));


            using var reader2 = cmd.ExecuteReader();
        }

        public string AddProductUsingStoredProc(Warehouse warehouse)
        {
            try
            {
                using (var connection = new SqlConnection("YourConnectionStringHere"))
                {
                    connection.Open();
                    using (var command = new SqlCommand("spAddProduct", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
                        command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
                        command.Parameters.AddWithValue("@Amount", warehouse.Amount);
                        command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);

                        var resultParameter = command.Parameters.Add("@Result", SqlDbType.VarChar, 500);
                        resultParameter.Direction = ParameterDirection.Output;

                        command.ExecuteNonQuery();

                        return resultParameter.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }


}
