using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.NetApp.Models;

namespace CPS.ComplexCases.NetApp.Factories;

public class NetAppCertFactory(ILogger<NetAppCertFactory> logger, IOptions<NetAppOptions> options) : INetAppCertFactory
{
    private readonly ILogger<NetAppCertFactory> _logger = logger;
    private readonly NetAppOptions _options = options.Value;
    private X509Certificate2Collection? _trustedCaCertificates;

    public X509Certificate2Collection GetTrustedCaCertificates()
    {
        if (_trustedCaCertificates != null)
        {
            return _trustedCaCertificates;
        }

        _trustedCaCertificates = [];

        // Load Root CA certificate from environment variable (Base64 encoded)
        var rootCaBase64 = _options.RootCaCert;
        if (!string.IsNullOrEmpty(rootCaBase64))
        {
            try
            {
                var rootCaBytes = Convert.FromBase64String(rootCaBase64);
                var rootCaCert = new X509Certificate2(rootCaBytes);
                _trustedCaCertificates.Add(rootCaCert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Root CA certificate from KV. Error: {Message}.", ex.Message);
                _trustedCaCertificates = null;
                throw new InvalidOperationException("Failed to load Root CA certificate from Key Vault. See inner exception for details.", ex);
            }
        }

        // Load Issuing CA certificate from environment variable (Base64 encoded)
        var issuingCaBase64 = _options.IssuingCaCert;
        if (!string.IsNullOrEmpty(issuingCaBase64))
        {
            try
            {
                var issuingCaBytes = Convert.FromBase64String(issuingCaBase64);
                var issuingCaCert = new X509Certificate2(issuingCaBytes);
                _trustedCaCertificates.Add(issuingCaCert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Issuing CA certificate from KV. Error: {Message}.", ex.Message);
                _trustedCaCertificates = null;
                throw new InvalidOperationException("Failed to load Issuing CA certificate from Key Vault. See inner exception for details.", ex);
            }
        }

        // Load second Issuing CA certificate from environment variable (Base64 encoded)
        var issuingCa2Base64 = _options.IssuingCaCert2;
        if (!string.IsNullOrEmpty(issuingCa2Base64))
        {
            try
            {
                var issuingCa2Bytes = Convert.FromBase64String(issuingCa2Base64);
                var issuingCa2Cert = new X509Certificate2(issuingCa2Bytes);
                _trustedCaCertificates.Add(issuingCa2Cert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Issuing CA certificate 2 from KV. Error: {Message}.", ex.Message);
                _trustedCaCertificates = null;
                throw new InvalidOperationException("Failed to load Issuing CA certificate 2 from Key Vault. See inner exception for details.", ex);
            }
        }

        return _trustedCaCertificates;
    }

    public bool ValidateCertificateWithCustomCa(X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors, X509Certificate2Collection trustedCaCerts)
    {
        // If no errors, certificate is valid
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        _logger.LogInformation("SSL policy errors detected: {SslPolicyErrors}. Attempting custom CA validation.", sslPolicyErrors);

        // If the only error is untrusted root, try validating with our custom CAs
        if (certificate == null || chain == null)
        {
            _logger.LogError("Certificate or chain is null. Cannot validate.");
            return false;
        }

        _logger.LogInformation("Validating certificate: Subject={Subject}, Issuer={Issuer}",
            certificate.Subject, certificate.Issuer);
        _logger.LogInformation("Trusted CA certificates count: {Count}", trustedCaCerts.Count);

        // Create a new chain with custom trust settings
        using var customChain = new X509Chain();

        // SECURITY NOTE: Revocation checking is disabled for the following reasons:
        // 1. Internal CA certificates typically do not have publicly accessible CRL/OCSP endpoints
        // 2. The NetApp storage service uses an internal CA without revocation infrastructure
        // COMPENSATING CONTROLS:
        // - Explicit thumbprint verification against known trusted CA certificates (lines 214-221)
        // - CA certificates are securely stored in Azure Key Vault
        // - Connection restricted to specific internal service endpoint via _options.Url
        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
            | X509VerificationFlags.IgnoreEndRevocationUnknown
            | X509VerificationFlags.IgnoreCtlSignerRevocationUnknown
            | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
            | X509VerificationFlags.IgnoreRootRevocationUnknown;

        // Add our trusted CA certificates to the extra store
        foreach (var caCert in trustedCaCerts)
        {
            _logger.LogInformation("Adding trusted CA to chain: Subject={Subject}, Thumbprint={Thumbprint}",
                caCert.Subject, caCert.Thumbprint);
            customChain.ChainPolicy.ExtraStore.Add(caCert);
        }

        // Build the chain
        var isChainValid = customChain.Build(certificate);

        if (!isChainValid)
        {
            foreach (var status in customChain.ChainStatus)
            {
                _logger.LogError("Chain validation failed: Status={Status}, StatusInformation={StatusInformation}",
                    status.Status, status.StatusInformation);
            }
            return false;
        }

        // Verify that the chain ends with one of our trusted CAs
        var chainRoot = customChain.ChainElements[^1].Certificate;
        _logger.LogInformation("Chain root certificate: Subject={Subject}, Thumbprint={Thumbprint}",
            chainRoot.Subject, chainRoot.Thumbprint);

        foreach (var trustedCa in trustedCaCerts)
        {
            if (chainRoot.Thumbprint == trustedCa.Thumbprint)
            {
                _logger.LogInformation("Certificate validated successfully against trusted CA: {Subject}", trustedCa.Subject);
                return true;
            }
        }

        _logger.LogError("Chain root thumbprint {ChainRootThumbprint} did not match any trusted CA thumbprints.",
            chainRoot.Thumbprint);
        return false;
    }
}