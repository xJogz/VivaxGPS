using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class gps
{
    static void Main(string[] args)
    {
        // Crée un écouteur TCP pour l'adresse IP et le port spécifiés
        IPAddress ip = IPAddress.Parse("92.142.1.246");  // Remplace par l'IP du GPS si nécessaire
        TcpListener server = new TcpListener(ip, 9001);  // Utilise le bon port

        try
        {
            server.Start();
            Console.WriteLine("Serveur démarré, en attente de connexion...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connecté");

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Trame reçue : " + receivedData);

                    // Traite la trame ici
                    // Par exemple, tu peux enregistrer ces données dans un fichier
                    System.IO.File.AppendAllText("donnees_gps_reseau.txt", receivedData + "\n");
                }

                client.Close();  // Ferme la connexion après réception des données
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur : " + ex.Message);
        }
    }
}