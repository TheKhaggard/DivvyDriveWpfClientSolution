using System;

namespace DivvyDriveWpfClient.Models
{
    public class DosyaOlusturModel
    {
        public Guid? ticketID { get; set; }
        public string? klasorYolu { get; set; }
        public string? dosyaAdi { get; set; }
    }

    public class DosyaSilModel
    {
        public Guid? ticketID { get; set; }
        public string? klasorYolu { get; set; }
        public string? dosyaAdi { get; set; }
    }

    public class DosyaGuncelleModel
    {
        public Guid? ticketID { get; set; }
        public string? klasorYolu { get; set; }
        public string? dosyaAdi { get; set; }
        public string? yeniDosyaAdi { get; set; }
    }

    public class DosyaTasiModel
    {
        public Guid? ticketID { get; set; }
        public string? klasorYolu { get; set; }
        public string? dosyaAdi { get; set; }
        public string? yeniDosyaYolu { get; set; }
    }

    public class DosyaYayinlaBilgi
    {
        public Guid? ticketID { get; set; }
        public string? ID { get; set; }
        public string? dosyaAdi { get; set; }
        public string? klasorYolu { get; set; }
    }
}
