using System.Collections.Generic;

namespace DivvyDriveWpfClient.Models
{
    public class KlasorVeDosyaIslemleriDonenSonuc
    {
        public string Mesaj { get; set; }
        public bool Sonuc { get; set; }
        public List<Klasor> SonucKlasorListe { get; set; }
        public List<Dosya> SonucDosyaListe { get; set; }
    }
}
