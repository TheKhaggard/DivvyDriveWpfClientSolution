namespace DivvyDriveWpfClient.Models
{
    public class KlasorGuncelleModel
    {
        public Guid ticketID { get; set; }
        public string klasorYolu { get; set; }
        public string klasorAdi { get; set; } // mevcut adı
        public string yeniKlasorAdi { get; set; } // yeni ad
    }
}
