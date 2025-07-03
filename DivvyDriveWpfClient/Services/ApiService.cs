using DivvyDriveWpfClient.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes("NDSServis:ca5094ef-eae0-4bd5-a94a-14db3b8f3950"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // MD5 hesapla
                using var md5 = MD5.Create();
                var hashBytes = md5.ComputeHash(fileBytes);
                var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                model.dosyaHash = hash;

                // Metadata gönder
                var meta = new DosyaMetaDataKaydiOlusturModel
                {
                    ticketID = model.ticketID,
                    dosyaAdi = model.dosyaAdi,
                    parcaSayisi = 1,
                    herBirParcaninBoyutuByte = fileBytes.LongLength
                };

                var metaContent = new StringContent(JsonConvert.SerializeObject(meta), Encoding.UTF8, "application/json");
                var metaResponse = await client.PostAsync("https://test.divvydrive.com/Test/Staj/DosyaMetaDataKaydiOlustur", metaContent);
                var metaStr = await metaResponse.Content.ReadAsStringAsync();

                if (!metaResponse.IsSuccessStatusCode)
                    return new GenelSonucModel { Sonuc = false, Mesaj = "Metadata gönderimi başarısız: " + metaStr };

                var parsedMeta = JsonConvert.DeserializeObject<KlasorVeDosyaIslemleriDonenSonuc>(metaStr);
                if (parsedMeta == null || !parsedMeta.Sonuc)
                    return new GenelSonucModel { Sonuc = false, Mesaj = "Metadata cevabı başarısız: " + parsedMeta?.Mesaj };

                // Upload URL oluştur
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["ticketID"] = model.ticketID.ToString();
                query["dosyaAdi"] = model.dosyaAdi;
                query["klasorYolu"] = model.klasorYolu;
                query["dosyaHash"] = model.dosyaHash;
                string uploadUrl = "https://test.divvydrive.com/Test/Staj/DosyaDirektYukle?" + query.ToString();

                // Dosyayı multipart ile yükle
                var multipart = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                multipart.Add(fileContent, "file", model.dosyaAdi);

                var uploadResp = await client.PostAsync(uploadUrl, multipart);
                var uploadStr = await uploadResp.Content.ReadAsStringAsync();

                if (!uploadResp.IsSuccessStatusCode)
                    return new GenelSonucModel { Sonuc = false, Mesaj = "Upload başarısız: " + uploadResp.StatusCode + " | " + uploadStr };

                return JsonConvert.DeserializeObject<GenelSonucModel>(uploadStr);
            }
            catch (Exception ex)
            {
                return new GenelSonucModel { Sonuc = false, Mesaj = "Hata: " + ex.Message };
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
