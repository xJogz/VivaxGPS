using System;
using System.Globalization;
using System.IO;
using MySql.Data.MySqlClient;

namespace GPSConnectionApp
{
    public static class DataBaseHelper
    {
        private const string ConnectionString = "Server=localhost;Database=vivaxdb;Uid=louis;Pwd=Reglisse15.;";

        public static void InsertTramesFromFile(string line)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();

                var parts = line.Split(',');

                if (parts.Length >= 22)
                {
                    string latitude = parts[14].Trim();
                    string latDir = parts[15].Trim();
                    string longitude = parts[16].Trim();
                    string lonDir = parts[17].Trim();

                    double latDecimal = ConvertToDecimalDegrees(latitude, latDir);
                    double lonDecimal = ConvertToDecimalDegrees(longitude, lonDir);

                    using var cmd = new MySqlCommand(@"
                        INSERT INTO vivax (
                            type_of_vivax_feature, name_of_device, gain, frequency, depth, inc_distance,
                            var_1, var_2, var_3, var_4, var_5, var_6,
                            gpgga_timestamp, gpgga_latitude, gpgga_latitude_direction,
                            gpgga_longitude, gpgga_longitude_direction, gpgga_fix,
                            gpgga_satellites, gpgga_quality, gpgga_altitude,
                            geometry, timestamp
                        ) VALUES (
                            @type, @name, @gain, @freq, @depth, @inc,
                            @v1, @v2, @v3, @v4, @v5, @v6,
                            @ts, @lat, @lat_dir, @lon, @lon_dir, @fix,
                            @sats, @quality, @alt,
                            ST_GeomFromText(@geom, 4326), NOW()
                        )", conn);

                    cmd.Parameters.AddWithValue("@type", parts[0].Trim());
                    cmd.Parameters.AddWithValue("@name", parts[1].Trim());
                    cmd.Parameters.AddWithValue("@gain", int.Parse(parts[2].Trim()));
                    cmd.Parameters.AddWithValue("@freq", int.Parse(parts[3].Trim()));
                    cmd.Parameters.AddWithValue("@depth", int.Parse(parts[4].Trim()));
                    cmd.Parameters.AddWithValue("@inc", int.Parse(parts[5].Trim()));
                    cmd.Parameters.AddWithValue("@v1", int.Parse(parts[6].Trim()));
                    cmd.Parameters.AddWithValue("@v2", int.Parse(parts[7].Trim()));
                    cmd.Parameters.AddWithValue("@v3", int.Parse(parts[8].Trim()));
                    cmd.Parameters.AddWithValue("@v4", int.Parse(parts[9].Trim()));
                    cmd.Parameters.AddWithValue("@v5", int.Parse(parts[10].Trim()));
                    cmd.Parameters.AddWithValue("@v6", int.Parse(parts[11].Trim()));

                    cmd.Parameters.AddWithValue("@ts", parts[13].Trim());
                    cmd.Parameters.AddWithValue("@lat", latitude);
                    cmd.Parameters.AddWithValue("@lat_dir", latDir);
                    cmd.Parameters.AddWithValue("@lon", longitude);
                    cmd.Parameters.AddWithValue("@lon_dir", lonDir);
                    cmd.Parameters.AddWithValue("@fix", int.Parse(parts[18].Trim()));
                    cmd.Parameters.AddWithValue("@sats", int.Parse(parts[19].Trim()));
                    cmd.Parameters.AddWithValue("@quality", double.Parse(parts[20].Trim(), CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@alt", double.Parse(parts[21].Trim(), CultureInfo.InvariantCulture));

                    string geomText = $"POINT({lonDecimal.ToString(CultureInfo.InvariantCulture)} {latDecimal.ToString(CultureInfo.InvariantCulture)})";
                    cmd.Parameters.AddWithValue("@geom", geomText);

                    cmd.ExecuteNonQuery();

                    Console.WriteLine("Ligne insérée dans la base de données MySQL : " + line);
                }
                else
                {
                    Console.WriteLine("Ligne invalide : " + line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur MySQL : " + ex.Message);
            }
        }

        private static double ConvertToDecimalDegrees(string value, string direction)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var coord))
            {
                double degrees = Math.Floor(coord / 100);
                double minutes = coord - (degrees * 100);
                double decimalDegrees = degrees + (minutes / 60);

                if (direction == "S" || direction == "W")
                    decimalDegrees *= -1;

                return decimalDegrees;
            }
            return 0.0;
        }
    }
}
