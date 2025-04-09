using System;
using System.IO;
using Npgsql;

namespace GPSConnectionApp
{
    public static class DataBaseHelper
    {
        // Chaîne de connexion à la base de données PostgreSQL
        private const string ConnectionString = "Host=localhost;Username=louis;Password=Reglisse15.;Database=vivaxdb";

        // Méthode pour insérer une seule trame dans la base de données
        public static void InsertTramesFromFile(string line)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                conn.Open();

                var parts = line.Split(',');

                if (parts.Length >= 22)
                {
                    using var cmd = new NpgsqlCommand(@"
                        INSERT INTO vivax (
                            type_of_vivax_feature, name_of_device, gain, frequency, depth, inc_distance,
                            var_1, var_2, var_3, var_4, var_5, var_6,
                            gpgga_timestamp, gpgga_latitude, gpgga_latitude_direction,
                            gpgga_longitude, gpgga_longitude_direction, gpgga_fix,
                            gpgga_satellites, gpgga_quality, gpgga_altitude, timestamp
                        ) VALUES (
                            @type, @name, @gain, @freq, @depth, @inc,
                            @v1, @v2, @v3, @v4, @v5, @v6,
                            @ts, @lat, @lat_dir, @lon, @lon_dir, @fix,
                            @sats, @quality, @alt, NOW()
                        )", conn);

                    cmd.Parameters.AddWithValue("type", parts[0].Trim());
                    cmd.Parameters.AddWithValue("name", parts[1].Trim());
                    cmd.Parameters.AddWithValue("gain", parts[2].Trim());
                    cmd.Parameters.AddWithValue("freq", parts[3].Trim());
                    cmd.Parameters.AddWithValue("depth", parts[4].Trim());
                    cmd.Parameters.AddWithValue("inc", parts[5].Trim());
                    cmd.Parameters.AddWithValue("v1", parts[6].Trim());
                    cmd.Parameters.AddWithValue("v2", parts[7].Trim());
                    cmd.Parameters.AddWithValue("v3", parts[8].Trim());
                    cmd.Parameters.AddWithValue("v4", parts[9].Trim());
                    cmd.Parameters.AddWithValue("v5", parts[10].Trim());
                    cmd.Parameters.AddWithValue("v6", parts[11].Trim());
                    cmd.Parameters.AddWithValue("ts", parts[12].Trim());
                    cmd.Parameters.AddWithValue("lat", parts[13].Trim());
                    cmd.Parameters.AddWithValue("lat_dir", parts[14].Trim());
                    cmd.Parameters.AddWithValue("lon", parts[15].Trim());
                    cmd.Parameters.AddWithValue("lon_dir", parts[16].Trim());
                    cmd.Parameters.AddWithValue("fix", parts[17].Trim());
                    cmd.Parameters.AddWithValue("sats", parts[18].Trim());
                    cmd.Parameters.AddWithValue("quality", parts[19].Trim());
                    cmd.Parameters.AddWithValue("alt", parts[20].Trim());

                    cmd.ExecuteNonQuery();

                    Console.WriteLine("Ligne insérée dans la base de données : " + line);
                }
                else
                {
                    Console.WriteLine("La ligne ne contient pas assez de données : " + line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'insertion dans la BDD : " + ex.Message);
            }
        }
    }

}