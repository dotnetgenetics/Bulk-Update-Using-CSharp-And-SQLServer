using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BulkUpdateCSharpSQL
{
    /// <summary>
    /// change internal to public
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var listPerson = new List<Person>
            {
                new Person() {Id = 1001, Name = "James A.", Address = "US"},
                new Person() {Id = 1002, Name = "Troy B.", Address = "UK"},
                new Person() {Id = 1003, Name = "Mike C.", Address = "Philippines"},
                new Person() {Id = 1004, Name = "Angel D.", Address = "Japan"}
            };

            UpdateData(listPerson, "dbo.Person");

            Console.ReadLine();
        }

        /// <summary>
        /// Perform a bulk insert from the data into a temp table, and then use a command or 
        /// stored procedure to update the data relating the temp table with the destination 
        /// table. 
        /// </summary>
        public static void UpdateData<T>(List<T> list, string TableName)
        {
            DataTable dt = new DataTable("PersonTable");
            dt = list.AsDataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["dbConnection"]))
            {
                using (SqlCommand command = new SqlCommand("", conn))
                {
                    try
                    {
                        conn.Open();

                        //Creating temp table on database
                        command.CommandText = "CREATE TABLE #tblTmpPerson([ID] [int] NOT NULL, [Name][varchar](50) NULL, [Address] [varchar] (50) NULL);";
                        command.ExecuteNonQuery();

                        //Bulk insert into temp table
                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = "#tblTmpPerson";
                            bulkcopy.WriteToServer(dt);
                            bulkcopy.Close();
                        }

                        // Updating destination table, and dropping temp table
                        command.CommandTimeout = 300;
                        command.CommandText = "UPDATE T SET Name=Temp.Name, Address=Temp.Address FROM " + TableName + " T INNER JOIN #tblTmpPerson Temp ON T.ID = Temp.ID; DROP TABLE #tblTmpPerson;";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Handle exception properly
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}
