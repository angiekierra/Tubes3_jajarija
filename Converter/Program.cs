using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using static AES;
class EncryptDBAuto
{
    public static async Task Main(string[] args)
    {
        byte[] key = Encoding.UTF8.GetBytes("tubesstimaterakhirohyeah12345678");

        // Set these values correctly for your database server
        var builder = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            UserID = "root",
            Password = "173146",
            Database = "testdekripsi",
        };

        // Open a connection asynchronously
        using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        // Create a DB command and set the SQL statement
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM encryptedbiodata;";

        // Execute the command and read the results
        var dataToDump = new List<Dictionary<string, object>>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var record = new Dictionary<string, object>
            {
                { "NIK", reader["NIK"].ToString() },
                { "nama", reader["nama"].ToString() },
                { "tempat_lahir", reader["tempat_lahir"].ToString() },
                { "tanggal_lahir", DateTime.Parse(reader["tanggal_lahir"].ToString()).ToString("yyyy-MM-dd") },
                { "jenis_kelamin", reader["jenis_kelamin"].ToString() },
                { "golongan_darah", reader["golongan_darah"].ToString() },
                { "alamat", reader["alamat"].ToString() },
                { "agama", reader["agama"].ToString() },
                { "status_perkawinan", reader["status_perkawinan"].ToString() },
                { "pekerjaan", reader["pekerjaan"].ToString() },
                { "kewarganegaraan", reader["kewarganegaraan"].ToString() }
            };
            dataToDump.Add(record);
        }

        // Insert encrypted records into the database
        await InsertEncryptedRecords(dataToDump, builder.ConnectionString, key);
    }

    public static async Task InsertEncryptedRecords(List<Dictionary<string, object>> dataToDump, string connectionString, byte[] key)
    {
        AES aes = new AES(key);

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Drop the existing table if it exists
            await DropTableIfExists(connection);

            // Create the new table
            await CreateEncryptedBiodataTable(connection);

            // Insert records into the newly created table
            foreach (var record in dataToDump)
            {
                // Escape and encrypt each value before inserting it into the database
                string encryptedNIK = EscapeAndEncryptSqlString(record["NIK"].ToString(), aes);
                string encryptedNama = EscapeAndEncryptSqlString(record["nama"].ToString(), aes);
                string encryptedTempatLahir = EscapeAndEncryptSqlString(record["tempat_lahir"].ToString(), aes);
                string encryptedTanggalLahir = EscapeAndEncryptSqlString(record["tanggal_lahir"].ToString(), aes);
                string encryptedJenisKelamin = record["jenis_kelamin"].ToString(); // This value is not encrypted
                string encryptedGolonganDarah = EscapeAndEncryptSqlString(record["golongan_darah"].ToString(), aes);
                string encryptedAlamat = EscapeAndEncryptSqlString(record["alamat"].ToString(), aes);
                string encryptedAgama = EscapeAndEncryptSqlString(record["agama"].ToString(), aes);
                string encryptedStatusPerkawinan = EscapeAndEncryptSqlString(record["status_perkawinan"].ToString(), aes);
                string encryptedPekerjaan = EscapeAndEncryptSqlString(record["pekerjaan"].ToString(), aes);
                string encryptedKewarganegaraan = EscapeAndEncryptSqlString(record["kewarganegaraan"].ToString(), aes);

                // Create an SQL INSERT query with parameterized values to prevent SQL injection
                string insertQuery = "INSERT INTO EncryptedBiodata (NIK, nama, tempat_lahir, tanggal_lahir, jenis_kelamin, golongan_darah, alamat, agama, status_perkawinan, pekerjaan, kewarganegaraan) VALUES (@NIK, @nama, @tempat_lahir, @tanggal_lahir, @jenis_kelamin, @golongan_darah, @alamat, @agama, @status_perkawinan, @pekerjaan, @kewarganegaraan);";

                // Create and configure the SQL command
                using var command = new MySqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@NIK", encryptedNIK);
                command.Parameters.AddWithValue("@nama", encryptedNama);
                command.Parameters.AddWithValue("@tempat_lahir", encryptedTempatLahir);
                command.Parameters.AddWithValue("@tanggal_lahir", encryptedTanggalLahir);
                command.Parameters.AddWithValue("@jenis_kelamin", encryptedJenisKelamin);
                command.Parameters.AddWithValue("@golongan_darah", encryptedGolonganDarah);
                command.Parameters.AddWithValue("@alamat", encryptedAlamat);
                command.Parameters.AddWithValue("@agama", encryptedAgama);
                command.Parameters.AddWithValue("@status_perkawinan", encryptedStatusPerkawinan);
                command.Parameters.AddWithValue("@pekerjaan", encryptedPekerjaan);
                command.Parameters.AddWithValue("@kewarganegaraan", encryptedKewarganegaraan);

                // Execute the SQL command
                await command.ExecuteNonQueryAsync();
            }

            Console.WriteLine("Encrypted records inserted into the database successfully.");
        }
    }

    public static async Task DropTableIfExists(MySqlConnection connection)
    {
        // Drop the table if it exists
        using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = "DROP TABLE IF EXISTS EncryptedBiodata;";
        await dropCommand.ExecuteNonQueryAsync();
        Console.WriteLine("Existing table dropped (if it existed).");
    }

    public static async Task CreateEncryptedBiodataTable(MySqlConnection connection)
    {
        // Create the new table
        using var createCommand = connection.CreateCommand();
        createCommand.CommandText = "CREATE TABLE EncryptedBiodata (NIK varchar(255) NOT NULL, nama varchar(255), tempat_lahir varchar(255), tanggal_lahir varchar(255), jenis_kelamin varchar(255), golongan_darah varchar(255), alamat varchar(255), agama varchar(255), status_perkawinan varchar(255), pekerjaan varchar(255), kewarganegaraan varchar(255));";
        await createCommand.ExecuteNonQueryAsync();
        Console.WriteLine("New table created.");
    }

    public static string EscapeAndEncryptSqlString(string value, AES aes)
    {
        // Escape single quotes
        string escapedValue = value.Replace("'", "''");
        // Encrypt the escaped value
        byte[] encryptedBytes = aes.Encrypt(escapedValue);
        // Convert the encrypted bytes to a base64 string for storage
        return Convert.ToBase64String(encryptedBytes);
    }
}