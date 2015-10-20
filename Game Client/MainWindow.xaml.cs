using Game_Client.Database;
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

        private static MainWindow instance;

        public MainWindow() {
            InitializeComponent();
        }

        private void btnlogin_Click(object sender, RoutedEventArgs e) {
            // Retrieve our username and password, and check if they are the right format.
            var username = txtusername.Text;
            var password = txtpassword.Password;

            // TODO: Actually validate these fields.

            LoginMenu.Visibility = Visibility.Hidden;
            Send.LoginRequest(username, password);
        }

        public static MainWindow Instance() {
            if (instance == null) instance = new MainWindow();
            return instance;
        }

        public void ShowRealmList(List<Realm> list) {
            lstrealms.Items.Clear();
            foreach (var item in list) {
                lstrealms.Items.Add(String.Format("{0} - {1}", item.Name, item.Online == true ? "ONLINE" : "OFFLINE"));
            }
            RealmListMenu.Visibility = Visibility.Visible;
        }

        private void btnconnect_Click(object sender, RoutedEventArgs e) {

        }
    }
}
