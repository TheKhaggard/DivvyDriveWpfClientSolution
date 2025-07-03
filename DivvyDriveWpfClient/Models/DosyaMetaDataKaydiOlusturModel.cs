using System;

namespace DivvyDriveWpfClient.Models
{
    public class DosyaMetaDataKaydiOlusturModel
    {
        public Guid ticketID { get; set; }
        public int parcaSayisi { get; set; }
        public string dosyaAdi { get; set; }
        public long herBirParcaninBoyutuByte { get; set; }
    }
}
