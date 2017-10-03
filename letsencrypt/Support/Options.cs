﻿using CommandLine;
using System;
using System.IO;

namespace letsencrypt.Support
{
    public class Options
    {
        public Options()
        {
            BaseUri = "https://acme-v01.api.letsencrypt.org/";
            CertificateStore = "WebHosting";
            FileDateFormat = "yyyy/M/d h:m:s tt";
            PrivateKeyExportable = true;
            RenewalPeriod = 60;
            RSAKeyBits = 2048;
            UseDefaultTaskUser = true;
            WebRoot = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\inetpub\wwwroot");
        }

        [Option(Default = "https://acme-v01.api.letsencrypt.org/", HelpText = "The address of the ACME server to use.")]
        public string BaseUri { get; set; }

        [Option(HelpText = "Use the default user for the renew task.")]
        public bool UseDefaultTaskUser { get; set; }

        [Option(HelpText = "Provide email contact address.")]
        public string EmailAddress { get; set; }

        [Option(HelpText = "Accept the terms of service.")]
        public bool AcceptTos { get; set; }

        [Option(HelpText = "Email address (not public) to use for renewal fail notices - only gets set on first run for each Let's Encrypt server")]
        public string SignerEmail { get; set; }

        [Option(HelpText = "Check for renewals.")]
        public bool Renew { get; set; }

        [Option(HelpText = "Overrides BaseUri setting to https://acme-staging.api.letsencrypt.org/")]
        public bool Test { get; set; }

        [Option(HelpText = "Host names to get a certificate for. (comma separated)")]
        public string Hostname { get; set; }

        [Option(Default = @"%SystemDrive%\inetpub\wwwroot",
            HelpText = "The web root to use for manual host name authentication.")]
        public string WebRoot { get; set; }

        [Option(HelpText = "A script for installation of non IIS Plugin.")]
        public string Script { get; set; }

        [Option(HelpText = "Parameters for the script for installation of non IIS Plugin.")]
        public string ScriptParameters { get; set; }

        [Option(HelpText = "Whether to use the Centralized Certificate Store")]
        public bool CentralSsl { get; set; }

        [Option(HelpText = @"Path for Centralized Certificate Store (This enables Centralized SSL). Ex. \\storage\central_ssl\")]
        public string CentralSslStore { get; set; }

        [Option(HelpText = @"Path for certificate files to be output. Ex. C:\Sites\MyWeb.com\certs")]
        public string CertOutPath { get; set; }

        [Option(HelpText = "Hide sites that have existing HTTPS bindings")]
        public bool HideHttps { get; set; }

        [Option(HelpText = "Certificates per site instead of per host")]
        public bool San { get; set; }

        [Option(HelpText = "Keep existing HTTPS bindings, and certificates")]
        public bool KeepExisting { get; set; }

        [Option(HelpText = "Warmup sites before authorization")]
        public bool Warmup { get; set; }

        [Option(HelpText = "Which plugin to use")]
        public string Plugin { get; set; }

        [Option(HelpText = "A web proxy address to use.")]
        public string Proxy { get; set; }

        [Option(HelpText = "Execute silently - no prompts.  If any information is needed, you must pass it in.")]
        public bool Silent { get; set; }

        [Option(HelpText = "Path to a folder where configuration files will be saved.")]
        public string ConfigPath { get; set; }

        [Option(HelpText = "Path to the plugin configuration file.")]
        public string PluginConfig { get; set; }

        [Option(HelpText = "The name of the Windows certificate store where certificates will be installed (Default is WebHosting.")]
        public string CertificateStore { get; set; }

        [Option(HelpText = "The number of days after the certificate issuance to renew it. (Default is 60)")]
        public int RenewalPeriod { get; set; }

        [Option(HelpText = "The password to use on the certificate PFX file")]
        public string PFXPassword { get; set; }

        [Option(HelpText = "Whether to delete empty folders created for /.well-known/acme-challenge")]
        public bool CleanupFolders { get; set; }

        [Option(HelpText = "Either 2048 (default) or 1024")]
        public int RSAKeyBits { get; set; }

        [Option(HelpText = "The date format used for the certificate friendly name (Default is \"yyyy/M/d h:m:s tt\")")]
        public string FileDateFormat { get; set; }

        [Option(HelpText = "Whether the private key is exportable (Default is true)")]
        public bool PrivateKeyExportable { get; set; }
    }
}