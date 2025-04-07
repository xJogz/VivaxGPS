using System;
using System.IO.Ports;

class vivax
{
    static void Main(string[] args)
    {
        SerialPort vivaxPort = new SerialPort("/tmp/vivax1", 9600); 

        vivaxPort.DataReceived += VivaxDataReceived; //lie l'event à la méthode

        try
        {
            vivaxPort.Open();
            Console.WriteLine("Connexion au Vivax ouverte. En attente de données...");
            Console.WriteLine("Appuie sur une touche pour arrêter.");
            Console.ReadKey();
            vivaxPort.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur : " + ex.Message);
        }
    }

    private static void VivaxDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort sp = (SerialPort)sender;
        string trame = sp.ReadLine(); 

        Console.WriteLine("Trame reçue : " + trame);

   
        string[] valeurs = trame.Split(',');


        if (valeurs.Length >= 12)
        {
         
            string log = valeurs[0];      
            string location = valeurs[1]; 
            int coord1 = int.Parse(valeurs[2]); 
            int profondeur = int.Parse(valeurs[3]); 

            string ligne = $"LOG: {log}, Location: {location}, Coordonnée 1: {coord1}, Profondeur: {profondeur}";

         
            using (StreamWriter writer = new StreamWriter("trames.txt", true))
            {
                writer.WriteLine(ligne); 
            }

            
            Console.WriteLine(ligne);
        }
        else
        {
            Console.WriteLine("Trame mal formée, nombre d'éléments incorrect.");
        }
    }
}