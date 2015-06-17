using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    public interface ICertificateSigner
    {
        byte[] Sign(ICertificate certificate);

        ValidationResult Verify();
    }

    public class CertificateSigner : ICertificateSigner
    {
        #region Variables
        private byte[] _original = null;
        private byte[] _signature = null;
        private bool _detached = true;
        private ICertificate _certificate = null;
        #endregion

        #region Properties
        public byte[] Original
        { get { return _original; } }

        public byte[] Signature
        { get { return _signature; } }

        public bool Detached
        { get { return _detached; } }

        public ICertificate Certificate
        { get { return _certificate; } }
        #endregion

        #region Constructors
        public CertificateSigner(byte[] original, bool detached)
        {
            _original = original;
            _signature = null;
            _detached = detached;
        }

        public CertificateSigner(byte[] signature, byte[] original = null)
        {
            _signature = signature;
            _original = original;
            _detached = original != null;
        }
        #endregion

        #region Functions
        /// <summary>
        /// Подписание данных (файла) с помощью сертификата ЭП 
        /// </summary>
        /// <param name="certificate">Сертификат Электронной Подписи, которым будет подписан файл</param>
        /// <returns>Файл с подписью (в случае прикрепленной подписи будет файл с данными и подписью) </returns>
        public byte[] Sign(ICertificate certificate)
        {
            //создаем контейнер с данными, которые будут подписываться
            var content = new ContentInfo(this.Original);

            //создаем пакет, в который помещаем контейнер с данными и параметры подписи
            //это основной объект, в рамках которого формируются проверки и преобразования подписи
            var cms = new SignedCms(content, this.Detached);

            //создаем подписанта (объект на основе сертификата, который будет подписывать)
            var signer = new CmsSigner(certificate.CertificateX509);

            //с помощью подписанта подписываем пакет так,
            //что теперь в пакете находятся не сами данные,
            //а именно подписанные данные, то есть:
            //  - сама подпись в случае отсоединенной подписи
            //  - подпись с оригинальными данными в случае присоединенной подписи
            cms.ComputeSignature(signer, false);

            // сохраняем подписанный пакет
            byte[] result = cms.Encode();

            return result;
        }

        /// <summary>
        /// Проверка файла с подписью на валидность подписи
        /// </summary>
        /// <returns>Сообщения (или ошибки) о проверки подписи</returns>
        public ValidationResult Verify()
        {
            var result = new ValidationResult();

            if (this.Signature == null)
            {
                result.AddError("Отсутствует файл с подписью!");
                return result;
            }

            //Создаем пакет с подписью для проверки самой подписи
            SignedCms cms = null;

            if (this.Detached)
            {//отсоединенная подпись

                //создаем контейнер с оригинальными данными, подпись которых будет проверяться
                ContentInfo content = new ContentInfo(this.Original);

                //формируем пакет с оригинальными данными и параметрами подписи
                cms = new SignedCms(content, true);
            }
            else
            {// присоединенная подпись 

                //формируем пустой пакет с данными
                //так как в случае присоединенной подписи 
                //данные содержатся в самом подписанном файле
                cms = new SignedCms();
            }


            try
            {
                //декодируем файл, содержащий подпись
                //если вылетает ошибка - значит подпись не верна!!!
                cms.Decode(this.Signature);

                //возможно, информация о подписаниях отсутствует
                if (cms.SignerInfos.Count <= 0)
                {
                    result.AddError("Нет информации о подписях (возможно файл не подписан)");
                    return result;
                }

                result.AddInfo("Электронная Подпись Вернa.");
                result.AddNewLine();
                result.Add("Файл подписан следующим(и) сертификатом(и):");

                foreach (SignerInfo si in cms.SignerInfos)
                {
                    var certificate = new Certificate(si.Certificate);
                    if (_certificate == null)
                        _certificate = certificate;

                    result.AddNewLine();
                    result.Add(certificate.SerialNumber + " [" + certificate.Thumbprint + "]");
                    result.Add(certificate.SubjectCommonName);

                    //дергаем время подписания документа текущей подписью 
                    for (int i = 0; i < si.SignedAttributes.Count; i++)
                    {
                        if (si.SignedAttributes[i].Oid.Value == "1.2.840.113549.1.9.5") // Oid время подписания
                        {
                            Pkcs9SigningTime pkcs9_time = new Pkcs9SigningTime(si.SignedAttributes[i].Values[0].RawData);
                            result.Add("Дата и Время подписания:  " + pkcs9_time.SigningTime.ToString());
                            break;
                        }
                    }
                }
            }
            catch
            {
                result.AddError("Подпись НЕ верна!");
            }

            return result;
        }
        #endregion
    }
}
