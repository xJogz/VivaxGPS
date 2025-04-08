using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using System.IO;
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
        private bool _isConnected = false;

   
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

                // Initialisation du client UDP
                _udpClient = new UdpClient();
                _udpClient.EnableBroadcast = true;
                
                // Test de la connexion avec un message ping
                _isConnected = await TestConnectionAsync(ip, port);
                
                if (_isConnected)
                {
                    _connectionStatus?.SetValue(TextBlock.TextProperty, "Connecté");

                    // Lancer la réception des trames dans un thread séparé
                    await Task.Run(() => ReceiveTrameAsync(ip, port)); // Pour éviter de bloquer l'UI
                }
                else
                {
                    _connectionStatus?.SetValue(TextBlock.TextProperty, "Erreur de connexion");
                }

                // Sauvegarder les paramètres
                SaveSettings();
            }
            catch (Exception ex)
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

                Console.WriteLine($"Envoi du message de ping à {ip}:{port}");
                await _udpClient.SendAsync(pingData, pingData.Length);
        
                // Attendre une réponse du serveur
                var result = await _udpClient.ReceiveAsync();
                string response = Encoding.ASCII.GetString(result.Buffer);
        
                Console.WriteLine($"Réponse reçue : {response}");
        
                return response.Contains("pong"); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion : {ex.Message}");
                return false;
            }
        }


        private void ReceiveTrameAsync(string ip, int port)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);

                // Boucle de réception des trames
                while (_isConnected)
                {
                    byte[] data = _udpClient.Receive(ref remoteEP); // Attend une trame

                    string trame = Encoding.ASCII.GetString(data);
                    string cheminFichier = "trames_gps.txt";
                    
                    // Sauvegarde la trame dans un fichier texte
                    File.AppendAllText(cheminFichier, trame + Environment.NewLine);
                    
                    Console.WriteLine("Trame reçue : " + trame);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de réception : " + ex.Message);
            }
        }





        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            
            _udpClient.Close();
            _isConnected = false;

            if (_isConnected == false)
            {

                _connectionStatus?.SetValue(TextBlock.TextProperty, "Non connecté");
            }
            

     
            SaveSettings();
        }
    }
}

