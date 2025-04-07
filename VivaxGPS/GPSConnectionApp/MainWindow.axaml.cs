using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.Text.Json;

namespace GPSConnectionApp
{
    public partial class MainWindow : Window
    {
        // Déclarations des contrôles
        private TextBox? _udpPortTextBox;
        private TextBox? _broadcastAddressTextBox;
        private TextBox? _comPortTextBox;
        private TextBlock? _connectionStatus;
        private Button? _connectButton;
        private Button? _disconnectButton;

        // Chemin du fichier de configuration
        private const string SettingsFilePath = "connectionSettings.json";

        public MainWindow()
        {
            InitializeComponent(); // Initialisation des composants XAML

            // Attachement des événements aux boutons
            _connectButton.Click += ConnectButton_Click;
            _disconnectButton.Click += DisconnectButton_Click;

            // Charger les paramètres depuis le fichier
            LoadSettings();
        }

        // Méthode d'initialisation des composants
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Trouver les contrôles via XAML
            _udpPortTextBox = this.FindControl<TextBox>("UdpPortTextBox");
            _broadcastAddressTextBox = this.FindControl<TextBox>("BroadcastAddressTextBox");
            _comPortTextBox = this.FindControl<TextBox>("ComPortTextBox");
            _connectionStatus = this.FindControl<TextBlock>("ConnectionStatus");
            _connectButton = this.FindControl<Button>("ConnectButton");
            _disconnectButton = this.FindControl<Button>("DisconnectButton");
        }

        // Méthode pour charger les paramètres depuis le fichier JSON
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

        // Méthode pour sauvegarder les paramètres dans un fichier JSON
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

        // Gestion du clic sur le bouton Connecter
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer les valeurs des champs texte
            string udpPort = _udpPortTextBox?.Text ?? string.Empty;
            string broadcastAddress = _broadcastAddressTextBox?.Text ?? string.Empty;
            string comPort = _comPortTextBox?.Text ?? string.Empty;

            // Logique de connexion
            _connectionStatus?.SetValue(TextBlock.TextProperty, "Connecté");

            // Sauvegarder les paramètres après la connexion
            SaveSettings();
        }

        // Gestion du clic sur le bouton Déconnecter
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Logique de déconnexion
            _connectionStatus?.SetValue(TextBlock.TextProperty, "Non connecté");

            // Sauvegarder les paramètres après la déconnexion
            SaveSettings();
        }
    }
}

