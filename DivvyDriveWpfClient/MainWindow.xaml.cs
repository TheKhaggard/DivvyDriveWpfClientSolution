using System;
using System.Windows;
using DivvyDriveWpfClient.Models;
using DivvyDriveWpfClient.Services;

namespace DivvyDriveWpfClient
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService = new ApiService();

        public MainWindow()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string kullaniciAdi = txtUsername.Text.Trim();
            string sifre = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Kullanıcı adı ve şifre boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var kullanici = new Kullanici
                {
                    KullaniciAdi = kullaniciAdi,
                    Sifre = sifre
                };

                var ticket = await _apiService.GetTicketAsync(kullanici);
                if (ticket != null && ticket.Sonuc)
                {
                    errorBorder.Visibility = Visibility.Collapsed;

                    var dashboard = new Dashboard(ticket);
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    errorBorder.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Giriş sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseError_Click(object sender, RoutedEventArgs e)
        {
            errorBorder.Visibility = Visibility.Collapsed;
        }
    }
}
