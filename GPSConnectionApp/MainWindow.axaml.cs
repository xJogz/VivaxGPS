using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace GPSConnectionApp
{
    public partial class MainWindow : Window
    {
    
        private TextBox? _udpPortTextBox;
        private TextBox? _broadcastAddressTextBox;
        private TextBox? _comPortTextBox;
        private TextBlock? _connectionStatus;
        private Button? _connectButton;
        private Button? _disconnectButton;
        private UdpClient? _udpClient;
        private SerialPort? _vivaxPort;
        private bool _isConnected = false;
        private RadioButton? _gpsRadioButton;
        private RadioButton? _vivaxRadioButton;

   
        private const string SettingsFilePath = "connectionSettings.json";

        public MainWindow()
        {
            InitializeComponent(); 


            _connectButton.Click += ConnectButton_Click;
            _disconnectButton.Click += DisconnectButton_Click;

      
            LoadSettings();
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _gpsRadioButton = this.FindControl<RadioButton>("GpsRadioButton");
            _vivaxRadioButton = this.FindControl<RadioButton>("VivaxRadioButton");
            _udpPortTextBox = this.FindControl<TextBox>("UdpPortTextBox");
            _broadcastAddressTextBox = this.FindControl<TextBox>("BroadcastAddressTextBox");
            _comPortTextBox = this.FindControl<TextBox>("ComPortTextBox");
            _connectionStatus = this.FindControl<TextBlock>("ConnectionStatus");
            _connectButton = this.FindControl<Button>("ConnectButton");
            _disconnectButton = this.FindControl<Button>("DisconnectButton");
        }


        private void LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<ConnectionSettings>(json);

                if (settings != null)
                {
                    _udpPortTextBox.Text = settings.UdpPort;
                    _broadcastAddressTextBox.Text = settings.BroadcastAddress;
                    _comPortTextBox.Text = settings.ComPort;
                }
            }
        }

    
        private void SaveSettings()
        {
            var settings = new ConnectionSettings
            {
                UdpPort = _udpPortTextBox.Text,
                BroadcastAddress = _broadcastAddressTextBox.Text,
                ComPort = _comPortTextBox.Text
            };

            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(SettingsFilePath, json);
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string udpPort = _udpPortTextBox?.Text ?? string.Empty;
                string broadcastAddress = _broadcastAddressTextBox?.Text ?? string.Empty;
                string comPort = _comPortTextBox?.Text ?? string.Empty;

                int port = int.Parse(udpPort);
                string ip = broadcastAddress;

                bool isGpsSelected = _gpsRadioButton.IsChecked ?? false;
                bool isVivaxSelected = _vivaxRadioButton.IsChecked ?? false;



                if (isGpsSelected)
                {
                    // Connexion GPS
                    await ConnectGPS(port, ip);
                }
                else if (isVivaxSelected)
                {
                    // Connexion Vivax
                    ConnectVivax(comPort);
                }
                else
                {
                    _connectionStatus?.SetValue(TextBlock.TextProperty, "Veuillez sélectionner un type de connexion");
                }
            }
            catch (Exception ex)
            {
                _connectionStatus?.SetValue(TextBlock.TextProperty, "Erreur : " + ex.Message);
            }
        }


        private async Task ConnectGPS(int port, string ip)
        {
            try
            {
                // Initialisation du client UDP
                _udpClient = new UdpClient();
                _udpClient.EnableBroadcast = true;

                // Test de la connexion avec un message ping
                _isConnected = await TestConnectionAsync(ip, port);

                if (_isConnected)
                {
                    _connectionStatus?.SetValue(TextBlock.TextProperty, "Connecté (GPS)");

                    // Lancer la réception des trames dans un thread séparé
                    await Task.Run(() => ReceiveTrameAsync(ip, port)); // Pour éviter de bloquer l'UI
                }
                else
                {
                    _connectionStatus?.SetValue(TextBlock.TextProperty, "Erreur de connexion");
                }

                // Sauvegarder les paramètres
                SaveSettings();
            }catch (Exception ex)
            {
                _connectionStatus?.SetValue(TextBlock.TextProperty, "Erreur : " + ex.Message);
            }
        }

        

        private async Task<bool> TestConnectionAsync(string ip, int port)
        {
            try
            {
                string pingMessage = "ping";
                byte[] pingData = Encoding.ASCII.GetBytes(pingMessage);

                // Créer un UdpClient local
                using (UdpClient udpClient = new UdpClient())
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                    await udpClient.SendAsync(pingData, pingData.Length, endPoint);  // Envoie le message
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'envoi du message de ping : " + ex.Message);
                return false;
            }
        }



        private void ReceiveTrameAsync(string ip, int port)
        {
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);  // L'adresse locale et le port à écouter
                _udpClient = new UdpClient(localEndPoint);  // Créer un UdpClient lié à ce port

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);

                while (_isConnected)
                {
                    byte[] data = _udpClient.Receive(ref remoteEP);  // Recevoir les données depuis la cible

                    string trame = Encoding.ASCII.GetString(data);

                    // Sauvegarder la trame reçue dans un fichier
                    string cheminFichier = "trames.txt";
                    File.AppendAllText(cheminFichier, trame + Environment.NewLine);

                    Console.WriteLine("Trame reçue : " + trame);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de réception : " + ex.Message);
            }
        }
        
        private void ConnectVivax(string comPort)
        {
            try
            {
                // Initialisation du port COM pour Vivax
                _vivaxPort = new SerialPort(comPort, 9600);

                _vivaxPort.Open();
                _connectionStatus?.SetValue(TextBlock.TextProperty, "Connecté (Vivax)");

                // Lancer la réception des trames Vivax dans un thread séparé
                Task.Run(() => ReadVivaxData());
            }
            catch (Exception ex)
            {
                _connectionStatus?.SetValue(TextBlock.TextProperty, "Erreur : " + ex.Message);
            }
        }

        private void ReadVivaxData()
        {
            try
            {
                while (_vivaxPort.IsOpen)
                {
                    string trame = _vivaxPort.ReadLine(); 

                    // Sauvegarder la trame reçue dans un fichier
                    string cheminFichier = "trames.txt";
                    File.AppendAllText(cheminFichier, trame + Environment.NewLine);

                    Console.WriteLine("Trame reçue (Vivax) : " + trame);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de réception Vivax : " + ex.Message);
            }
        }






        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_udpClient != null)
            {
                _udpClient.Close();  // Ferme la connexion UDP et libère le port
                _isConnected = false;
            }

            if (_vivaxPort != null && _vivaxPort.IsOpen)
            {
                _vivaxPort.Close();  // Ferme la connexion Vivax et libère le port
            }

            _connectionStatus?.SetValue(TextBlock.TextProperty, "Non connecté");

            SaveSettings();
        }

    }
}

