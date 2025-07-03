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

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> KlasorGuncelleAsync(KlasorGuncelleModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUrl}KlasorGuncelle", content);
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

        public async Task<GenelSonucModel> DosyaDirektYukleAsync(DosyaDirektYukleModel model, byte[] fileBytes)
        {
            try
            {
                using var client = new HttpClient();

                // Basic Authentication ekle
                var byteArray = Encoding.ASCII.GetBytes("NDSServis:ca5094ef-eae0-4bd5-a94a-14db3b8f3950");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // URL oluştur
                string baseUrl = "https://test.divvydrive.com/Test/Staj/DosyaDirektYukle";
                var queryParams = new Dictionary<string, string>
        {
            { "ticketID", model.ticketID.ToString() },
            { "dosyaAdi", model.dosyaAdi },
            { "klasorYolu", model.klasorYolu },
            { "dosyaHash", model.dosyaHash }
        };

                string urlWithParams = baseUrl + "?" + string.Join("&", queryParams.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

                // multipart/form-data içeriği
                var content = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = $"\"{model.dosyaAdi}\""
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                content.Add(fileContent, "file", model.dosyaAdi);

                var response = await client.PostAsync(urlWithParams, content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || !json.TrimStart().StartsWith("{"))
                {
                    return new GenelSonucModel
                    {
                        Sonuc = false,
                        Mesaj = $"Hatalı yanıt: {response.StatusCode}, içerik: {json}"
                    };
                }

                return JsonConvert.DeserializeObject<GenelSonucModel>(json);
            }
            catch (Exception ex)
            {
                return new GenelSonucModel { Sonuc = false, Mesaj = $"İstek hatası: {ex.Message}" };
            }
        }

        public async Task<byte[]> DosyaIndirAsync(DosyaIndirModel model)
        {
            model.indirilecekYol = "";

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUrl}DosyaIndir", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaOlusturAsync(DosyaOlusturModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUrl}DosyaOlustur", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaSilAsync(DosyaSilModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}DosyaSil")
            {
                Content = content
            };

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaGuncelleAsync(DosyaGuncelleModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUrl}DosyaGuncelle", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaTasiAsync(DosyaTasiModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseUrl}DosyaTasi", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaMetaDataKaydiOlusturAsync(DosyaMetaDataKaydiOlusturModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUrl}DosyaMetaDataKaydiOlustur", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaYayinlaAsync(DosyaYayinlaBilgi model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseUrl}DosyaYayinla", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }

        public async Task<KlasorVeDosyaIslemleriDonenSonuc> DosyaParcaYukleAsync(Guid ticketID, string tempKlasorID, string parcaHash, int parcaNumarasi)
        {
            string url = $"{baseUrl}DosyaParcalariYukle" +
                         $"?ticketID={ticketID}" +
                         $"&tempKlasorID={tempKlasorID}" +
                         $"&parcaHash={parcaHash}" +
                         $"&parcaNumarasi={parcaNumarasi}";

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(responseContent);
        }
    }
}
