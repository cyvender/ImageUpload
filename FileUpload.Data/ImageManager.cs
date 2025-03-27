using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace FileUpload.Data
{
    public class ImageManager
    {
        private readonly string _connectionString;

        public ImageManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int AddImage(Image image)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Images (ImageName, ImagePath, Password) 
                                VALUES (@imageName, @path, @password)
                                SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@imageName", image.ImageName);
            cmd.Parameters.AddWithValue("@path", image.ImagePath);
            cmd.Parameters.AddWithValue("@password", image.Password);
            connection.Open();

            return (int)(decimal)cmd.ExecuteScalar();
        }

        public Image GetImageById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Images
                                WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", id);
            connection.Open();

            var reader = cmd.ExecuteReader();
            var image = new Image();
            while (reader.Read())
            {
                image.Password = (int)reader["Password"];
                image.ImagePath = (string)reader["ImagePath"];
                image.Id = (int)reader["Id"];
                image.Views = (int)reader["Views"];
            }
            return image;
        }

        public void UpdateViews(Image image)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Images
                                SET Views = @NewCount
                                WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", image.Id);
            image.Views++;
            cmd.Parameters.AddWithValue("NewCount", image.Views);
            connection.Open();

            cmd.ExecuteNonQuery();
        }
    }
}
