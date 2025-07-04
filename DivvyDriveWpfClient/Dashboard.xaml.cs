﻿using DivvyDriveWpfClient.Models;
using DivvyDriveWpfClient.Services;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DivvyDriveWpfClient
{
    public partial class Dashboard : Window
    {
        private readonly ApiService _apiService;
        private readonly Ticket _ticket;
        private string? _seciliKlasorAdi;

        public Dashboard(Ticket ticket)
        {
            InitializeComponent();
            _ticket = ticket;
            _apiService = new ApiService();

            KlasorYoluTextBox.Text = "";
            ListeleKlasorleri();
        }

        private async void ListeleKlasorleri()
        {
            await LoadFolderContentsAsync(_ticket.ID, KlasorYoluTextBox.Text.Trim());
        }

        private async Task LoadFolderContentsAsync(Guid ticketId, string folderPath)
        {
            try
            {
                var sonuc = await _apiService.KlasorListesiGetirAsync(ticketId, folderPath);
                KlasorItemsControl.Items.Clear();

                if (sonuc?.SonucKlasorListe != null)
                    foreach (var klasor in sonuc.SonucKlasorListe)
                        KlasorItemsControl.Items.Add(klasor.Adi);

                if (sonuc?.SonucDosyaListe != null)
                    foreach (var dosya in sonuc.SonucDosyaListe)
                        KlasorItemsControl.Items.Add($"{dosya.Adi} ({dosya.Boyut} bytes)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klasör listelenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnaDizinListeleButton_Click(object sender, RoutedEventArgs e)
        {
            KlasorYoluTextBox.Text = "";
            ListeleKlasorleri();
        }

        private void UstKlasoreDonButton_Click(object sender, RoutedEventArgs e)
        {
            string yol = KlasorYoluTextBox.Text.Trim();
            KlasorYoluTextBox.Text = string.IsNullOrEmpty(yol) || !yol.Contains("/")
                ? ""
                : yol.Substring(0, yol.LastIndexOf("/"));
            ListeleKlasorleri();
        }

        private void KlasorKart_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is string secilen && !secilen.Contains("("))
            {
                _seciliKlasorAdi = secilen.Trim();
                string yol = KlasorYoluTextBox.Text.Trim();
                KlasorYoluTextBox.Text = string.IsNullOrEmpty(yol) ? _seciliKlasorAdi : $"{yol}/{_seciliKlasorAdi}";
                ListeleKlasorleri();
            }
        }

        private void CardOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string klasorAdi)
            {
                _seciliKlasorAdi = klasorAdi;
                var menu = (ContextMenu)this.FindResource("CardOptionsMenu");
                menu.PlacementTarget = button;
                menu.IsOpen = true;
            }
        }

        private async void DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_seciliKlasorAdi))
            {
                MessageBox.Show("Silinecek klasör seçilmedi.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string yol = string.IsNullOrEmpty(KlasorYoluTextBox.Text.Trim())
                ? _seciliKlasorAdi
                : $"{KlasorYoluTextBox.Text.Trim()}/{_seciliKlasorAdi}";

            if (MessageBox.Show($"'{yol}' klasörünü silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var model = new KlasorSilModel
                    {
                        ticketID = _ticket.ID,
                        klasorYolu = yol,
                        klasorAdi = _seciliKlasorAdi
                    };

                    var sonuc = await _apiService.KlasorSilAsync(model);
                    if (sonuc != null && sonuc.Sonuc)
                    {
                        MessageBox.Show("Klasör başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        ListeleKlasorleri();
                    }
                    else
                    {
                        MessageBox.Show($"Klasör silinemedi: {sonuc?.Mesaj ?? "Bilinmeyen hata"}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Klasör silinirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void MoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_seciliKlasorAdi))
            {
                MessageBox.Show("Taşınacak klasör seçilmedi.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentPath = string.IsNullOrEmpty(KlasorYoluTextBox.Text.Trim())
                ? _seciliKlasorAdi
                : $"{KlasorYoluTextBox.Text.Trim()}/{_seciliKlasorAdi}";

            string newPath = Interaction.InputBox("Klasörü taşımak istediğiniz yeni yolu giriniz:", "Klasör Taşı", "");
            if (string.IsNullOrWhiteSpace(newPath)) return;

            try
            {
                var model = new KlasorTasiModel
                {
                    ticketID = _ticket.ID,
                    eskiKlasorYolu = currentPath,
                    yeniKlasorYolu = newPath,
                    klasorAdi = _seciliKlasorAdi
                };

                var sonuc = await _apiService.KlasorTasiAsync(model);
                if (sonuc != null && sonuc.Sonuc)
                {
                    MessageBox.Show("Klasör başarıyla taşındı.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    ListeleKlasorleri();
                }
                else
                {
                    MessageBox.Show($"Klasör taşınamadı: {sonuc?.Mesaj ?? "Bilinmeyen hata"}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klasör taşınırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RenameFolder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_seciliKlasorAdi))
            {
                MessageBox.Show("İşlem yapılacak klasör seçilmedi.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentPath = string.IsNullOrEmpty(KlasorYoluTextBox.Text.Trim()) ? _seciliKlasorAdi : $"{KlasorYoluTextBox.Text.Trim()}/{_seciliKlasorAdi}";
            string newName = Interaction.InputBox("Yeni klasör adını giriniz:", "Klasör Yeniden Adlandır");

            if (string.IsNullOrWhiteSpace(newName)) return;

            try
            {
                var model = new KlasorGuncelleModel
                {
                    ticketID = _ticket.ID,
                    klasorYolu = currentPath,
                    klasorAdi = _seciliKlasorAdi,
                    yeniKlasorAdi = newName
                };

                var sonuc = await _apiService.KlasorGuncelleAsync(model);
                if (sonuc?.Sonuc == true)
                {
                    MessageBox.Show("Klasör yeniden adlandırıldı.", "Başarılı");
                    ListeleKlasorleri();
                }
                else
                {
                    MessageBox.Show($"Klasör yeniden adlandırılamadı: {sonuc?.Mesaj ?? "Bilinmeyen hata"}", "Hata");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yeniden adlandırma sırasında hata: {ex.Message}", "Hata");
            }
        }

        private async void OlusturButton_Click(object sender, RoutedEventArgs e)
        {
            string yeniKlasorAdi = YeniKlasorAdiTextBox.Text.Trim();
            if (string.IsNullOrEmpty(yeniKlasorAdi))
            {
                MessageBox.Show("Klasör adı girin.", "Uyarı");
                return;
            }

            try
            {
                var model = new KlasorOlusturModel
                {
                    ticketID = _ticket.ID,
                    klasorAdi = yeniKlasorAdi,
                    klasorYolu = KlasorYoluTextBox.Text.Trim()
                };

                var sonuc = await _apiService.KlasorOlusturAsync(model);
                if (sonuc?.Sonuc == true)
                {
                    MessageBox.Show($"Klasör '{yeniKlasorAdi}' oluşturuldu.", "Başarılı");
                    ListeleKlasorleri();
                    YeniKlasorAdiTextBox.Clear();
                }
                else
                {
                    MessageBox.Show($"Klasör oluşturulamadı: {sonuc?.Mesaj ?? "Bilinmeyen hata"}", "Hata");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klasör oluştururken hata: {ex.Message}", "Hata");
            }
        }

        private async void DosyaYukleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() != true)
                    return;

                string filePath = openFileDialog.FileName;

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Seçilen dosya bulunamadı.", "Hata");
                    return;
                }

                string fileName = Path.GetFileName(filePath);

                // ✅ Dosyayı ham byte olarak oku
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                Debug.WriteLine($"[LOG] Dosya byte uzunluğu: {fileBytes.Length}");

                // ✅ MD5 hash hesapla
                string hashString;
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(fileBytes);
                    hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }

                Debug.WriteLine($"[LOG] Hesaplanan MD5: {hashString}");
                MessageBox.Show($"Hesaplanan Hash: {hashString}");

                // ✅ Modeli oluştur
                var model = new DosyaDirektYukleModel
                {
                    ticketID = _ticket.ID,
                    dosyaAdi = fileName,
                    klasorYolu = KlasorYoluTextBox.Text.Trim(),
                    dosyaHash = hashString
                };

                // ✅ API fonksiyonuna gönder
                var result = await _apiService.DosyaDirektYukleAsync(model, fileBytes);

                if (result?.Sonuc == true)
                {
                    MessageBox.Show($"Dosya '{fileName}' başarıyla yüklendi.", "Başarılı");
                    ListeleKlasorleri();
                }
                else
                {
                    MessageBox.Show($"Yükleme başarısız: {result?.Mesaj ?? "Bilinmeyen hata"}", "Hata");
                    Debug.WriteLine($"[LOG] Sunucu hatası: {result?.Mesaj}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata");
                Debug.WriteLine($"[LOG] Exception: {ex}");
            }
        }

        private async void DosyaIndirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dosyaAdi = Interaction.InputBox("İndirilecek dosya adını girin:", "Dosya İndir");
                if (string.IsNullOrWhiteSpace(dosyaAdi)) return;

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = dosyaAdi,
                    Filter = "Tüm Dosyalar|*.*",
                    Title = "Dosya Kaydet"
                };

                if (saveFileDialog.ShowDialog() != true) return;

                var model = new DosyaIndirModel
                {
                    ticketID = _ticket.ID,
                    klasorYolu = KlasorYoluTextBox.Text.Trim(),
                    dosyaAdi = dosyaAdi
                };

                byte[] fileBytes = await _apiService.DosyaIndirAsync(model);
                await File.WriteAllBytesAsync(saveFileDialog.FileName, fileBytes);

                MessageBox.Show($"Dosya '{dosyaAdi}' indirildi.", "Başarılı");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya indirirken hata: {ex.Message}", "Hata");
            }
        }

        // Yeni eklenen refresh butonu click eventi
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_ticket != null)
            {
                _ = LoadFolderContentsAsync(_ticket.ID, KlasorYoluTextBox.Text.Trim());
            }
        }

    }
}
