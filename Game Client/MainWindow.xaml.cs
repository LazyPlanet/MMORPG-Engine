using Game_Client.Database;
using Game_Client.Graphics;
using Game_Client.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Game_Client {

    public partial class MainWindow : Window {

        private D3DImageSlimDX  d3dimagecontainer;
        private D3DScene        d3dbackdrop;

        private static MainWindow instance;

        public MainWindow() {
            Loaded += Window_Loaded;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Set the login window as our visible element.
            LoginMenu.Visibility = Visibility.Visible;

            // Initialize D3D and set up some basic settings.
            d3dimagecontainer = new D3DImageSlimDX();
            d3dimagecontainer.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;

            GameWindow.Source = d3dimagecontainer;
            d3dbackdrop = new D3DScene(1280, 720);
            SlimDX.Direct3D10.Texture2D Texture = d3dbackdrop.SharedTexture;

            d3dimagecontainer.SetBackBufferSlimDX(Texture);
            BeginRenderingScene();
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e) {
            // This fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (d3dimagecontainer.IsFrontBufferAvailable) {
                BeginRenderingScene();
            } else {
                StopRenderingScene();
            }
        }

        private void OnRendering(object sender, EventArgs e) {
            d3dbackdrop.Render(0);
            d3dimagecontainer.InvalidateD3DImage();
        }

        private void BeginRenderingScene() {
            if (d3dimagecontainer.IsFrontBufferAvailable) {
                SlimDX.Direct3D10.Texture2D Texture = d3dbackdrop.SharedTexture;
                d3dimagecontainer.SetBackBufferSlimDX(Texture);
                CompositionTarget.Rendering += OnRendering;
            }
        }

        void StopRenderingScene() {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void btnlogin_Click(object sender, RoutedEventArgs e) {
            // Retrieve our username and password, and check if they are the right format.
            var username = txtusername.Text;
            var password = txtpassword.Password;

            if (username.Length < 4 || password.Length < 4) {
                ShowLoginWarning("Input too short!");
                return;
            }

            var invalidchars = new[] { ';', '/', '#' };     // Whatever you don't want in passwords/usernames.
            if((from c in username where invalidchars.Contains(c) select true).Contains(true) || (from c in password where invalidchars.Contains(c) select true).Contains(true)) {
                ShowLoginWarning("Invalid Input!");
                return;
            }

            LoginMenu.Visibility = Visibility.Hidden;
            Send.LoginRequest(username, password);
        }

        public static MainWindow Instance() {
            if (instance == null) instance = new MainWindow();
            return instance;
        }

        public void ShowRealmList() {
            // Clear out all the old items from the list.
            lstrealms.Items.Clear();

            // Add all the realms as we have them now.
            foreach (var item in Data.RealmList) {
                lstrealms.Items.Add(String.Format("{0} - {1}", item.Name, item.Online == true ? "ONLINE" : "OFFLINE"));
            }

            // Show the realmlist menu.
            RealmListMenu.Visibility = Visibility.Visible;
        }

        public void ShowLoginWarning(String warning) {
            lblwarning.Content = warning;
            lblwarning.Visibility = Visibility.Visible;

            LoginMenu.Visibility = Visibility.Visible;
        }

        private void btnconnect_Click(object sender, RoutedEventArgs e) {
            // Set our current realm.
            Data.CurrentRealm = lstrealms.SelectedIndex >= 0 ? lstrealms.SelectedIndex : 0;

            // Disconnect the client gracefully and reconnect it to the chosen Realm.
            var client = NetClient.Instance();
            client.Close();
            client.Reset();
            client.Hostname = Data.RealmList.ElementAt(Data.CurrentRealm).Hostname;
            client.Port     = Data.RealmList.ElementAt(Data.CurrentRealm).Port;
            client.Connect();

            RealmListMenu.Visibility = Visibility.Hidden;

        }
    }
}
