using System;

namespace LetsEncrypt.ApplicationGateway.Models
{
    public class CertbotCertificate
    {
        public string Name { get; set; }

        public string Domains { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int ValidDays => (int)Math.Floor((ExpiryDate - DateTime.Now).TotalDays);

        public string Path { get; set; }

        public string PrivateKeyPath { get; set; }
    }
}