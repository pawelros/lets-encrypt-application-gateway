namespace LetsEncrypt.ApplicationGateway
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using LetsEncrypt.ApplicationGateway.Models;
    using Microsoft.Extensions.Logging;

    public class CertbotClient
    {
        private readonly ILogger<CertbotClient> logger;

        public CertbotClient(ILogger<CertbotClient> logger)
        {
            this.logger = logger;
        }

        public List<CertbotCertificate> GetCertificates()
        {
            var list = new List<CertbotCertificate>();

            var result = "certbot certificates".Bash();
            this.logger.LogInformation(result);

            if (result.Contains("Found the following certs"))
            {
                int position = 0;

                var lines = result.Split(Environment.NewLine).Skip(3);

                while (lines.Count() >= position + 5)
                {
                    var certLines = lines.Skip(position).Take(5);
                    position += 5;

                    var cert = this.ParseCertificate(certLines);
                    list.Add(cert);
                }

                return list;
            }

            return null;
        }

        private CertbotCertificate ParseCertificate(IEnumerable<string> lines)
        {
            if (lines.Count() < 4)
            {
                return null;
            }

            var cert = new CertbotCertificate();

            var regex = new Regex(@"^\s*Certificate Name:\s(.*)$");
            var match = regex.Match(lines.ElementAt(0));
            if (match.Success)
            {
                cert.Name = match.Groups[1].Value.Trim();
            }

            regex = new Regex(@"^\s*Domains:\s(.*)$");
            match = regex.Match(lines.ElementAt(1));
            if (match.Success)
            {
                cert.Domains = match.Groups[1].Value.Trim();
            }

            regex = new Regex(@"^\s*Expiry Date:\s(\d\d\d\d-\d\d-\d\d\s\d\d:\d\d:\d\d\+\d\d:\d\d)");
            match = regex.Match(lines.ElementAt(2));
            if (match.Success)
            {
                string expiryDate = match.Groups[1].Value.Trim();
                cert.ExpiryDate = DateTime.Parse(expiryDate);
            }

            regex = new Regex(@"^\s*Certificate Path:\s(.*)$");
            match = regex.Match(lines.ElementAt(3));
            if (match.Success)
            {
                cert.Path = match.Groups[1].Value.Trim();
            }

            regex = new Regex(@"^\s*Private Key Path:\s(.*)$");
            match = regex.Match(lines.ElementAt(4));
            if (match.Success)
            {
                cert.PrivateKeyPath = match.Groups[1].Value.Trim();
            }

            return cert;
        }

    }
}