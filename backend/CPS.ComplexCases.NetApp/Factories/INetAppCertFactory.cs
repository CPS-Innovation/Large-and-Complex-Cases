using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CPS.ComplexCases.NetApp.Factories;

public interface INetAppCertFactory
{
    X509Certificate2Collection GetTrustedCaCertificates();
    bool ValidateCertificateWithCustomCa(X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors, X509Certificate2Collection trustedCaCerts);
}