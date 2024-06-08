using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

public class ImageRecord
{
    public required string Name { get; set; }
    public required string BerkasCitra { get; set; } // Path to the image file
}


public class PersonRecord
{
    public required string NIK { get; set; }
    public required string Nama { get; set; }
    public required string Tempat_lahir { get; set; }
    public DateTime Tanggal_lahir { get; set; }
    public required string Jenis_kelamin { get; set; }
    public required string Golongan_darah { get; set; }
    public required string Alamat { get; set; }
    public required string Agama { get; set; }
    public required string Status_perkawinan { get; set; }
    public required string Pekerjaan { get; set; }
    public required string Kewarganegaraan { get; set; }
}


public class DatabaseHelper
{
    private string connectionString = "Server=localhost;Database=tubesStima;User=root;Password=Nuel123.";

    public List<ImageRecord> GetAllImages()
    {
        var images = new List<ImageRecord>();

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (var command = new MySqlCommand("SELECT nama, berkas_citra FROM sidik_jari", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var image = new ImageRecord
                        {
                            Name = reader.GetString("nama"),
                            BerkasCitra = reader.GetString("berkas_citra")
                        };

                        images.Add(image);
                    }
                }
            }
        }

        return images;
    }

    public List<PersonRecord> GetAllPeople()
    {
        var people = new List<PersonRecord>();

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (var command = new MySqlCommand("SELECT * FROM biodata", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var person = new PersonRecord
                        {
                            NIK = reader.GetString("NIK"),
                            Nama = reader.GetString("nama"),
                            Tempat_lahir = reader.GetString("tempat_lahir"),
                            Tanggal_lahir = reader.GetDateTime("tanggal_lahir"),
                            Jenis_kelamin = reader.GetString("jenis_kelamin"),
                            Golongan_darah = reader.GetString("golongan_darah"),
                            Alamat = reader.GetString("alamat"),
                            Agama = reader.GetString("agama"),
                            Status_perkawinan = reader.GetString("status_perkawinan"),
                            Pekerjaan = reader.GetString("pekerjaan"),
                            Kewarganegaraan = reader.GetString("kewarganegaraan")
                        };

                        people.Add(person);
                    }
                }
            }
        }

        return people;
    }


}
