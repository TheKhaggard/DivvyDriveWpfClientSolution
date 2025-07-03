using DivvyDriveWpfClient.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DivvyDriveWpfClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly string baseUrl = "https://test.divvydrive.com/Test/Staj/";

        public ApiService()
        {
            _client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes("NDSServis:ca5094ef-eae0-4bd5-a94a-14db3b8f3950");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<Ticket> GetTicketAsync(Kullanici kullanici)
        {
            var json = JsonConvert.SerializeObject(kullanici);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(baseUrl + "TicketAl", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Ticket>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> KlasorListesiGetirAsync(Guid ticketID, string klasorYolu)
        {
            var response = await _client.GetAsync($"{baseUrl}KlasorListesiGetir?ticketID={ticketID}&klasorYolu={Uri.EscapeDataString(klasorYolu)}");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> KlasorOlusturAsync(KlasorOlusturModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUrl}KlasorOlustur", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaDirektYukleAsync(DosyaDirektYukleModel model)
        {
            string url = $"{baseUrl}DosyaDirektYukle" +
                         $"?ticketID={model.ticketID}" +
                         $"&dosyaAdi={Uri.EscapeDataString(model.dosyaAdi)}" +
                         $"&klasorYolu={Uri.EscapeDataString(model.klasorYolu)}" +
                         $"&dosyaHash={model.dosyaHash}";

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<byte[]> DosyaIndirAsync(DosyaIndirModel model)
        {
            model.indirilecekYol = ""; // API boş bekliyorsa

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUrl}DosyaIndir", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> KlasorSilAsync(KlasorSilModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{baseUrl}KlasorSil"),
                Content = content
            };

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> KlasorTasiAsync(KlasorTasiModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUrl}KlasorTasi", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> KlasorGuncelleAsync(KlasorGuncelleModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUrl}KlasorGuncelle", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }
    }
}
