using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    public interface ICertificateReader
    {
        ICollection<CertificateInfo> GetCertificatesFromStore();
        CertificateInfo GetCertificateFromFile(byte[] data, string password);
    }

    public class CertificateReader : ICertificateReader
    {
        public ICollection<CertificateInfo> GetCertificatesFromStore()
        {
            //открываем хранилище сертификатов (личные сертификаты для текущего пользователя)
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            //открываем это хранилище для чтения
            store.Open(OpenFlags.ReadOnly);

            //фильтруем все доступные сертификаты
            return store.Certificates.Cast<X509Certificate2>()
                                    .Where(cert => cert.NotBefore < DateTime.Now && DateTime.Now < cert.NotAfter)
                                    .Select(cert => new CertificateInfo(cert))
                                    .ToArray();
        }

        public CertificateInfo GetCertificateFromFile(byte[] data, string password)
        {
            //проверяем, что данные не пусты
            if (data == null || data.Length <= 0)
                return null;

            //создаем новый сертификат из данных файла
            return new CertificateInfo(new X509Certificate2(data, password));
        }        
    }
}
