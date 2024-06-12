using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert
{
	public static class Utilities
	{
		public const string BEGIN_CERTIFICATE = "-----BEGIN CERTIFICATE-----";
		public const string END_CERTIFICATE = "-----END CERTIFICATE-----";
		public const string END_CERTIFICATE_W_EXTRACHARACTERS = "\\n-----END CERTIFICATE-----(\\n|)";
		public const string PKCS10_HEADER = "-----BEGIN CERTIFICATE REQUEST-----";
		public const string PKCS10_FOOTER = "-----END CERTIFICATE REQUEST-----";
		public const string PKCS10_HEADER_NEW = "-----BEGIN NEW CERTIFICATE REQUEST-----";
		public const string PKCS10_FOOTER_NEW = "-----END NEW CERTIFICATE REQUEST-----";
		public const string BASE64_REGEX_STRING = @"^[a-zA-Z0-9\+/]*={0,2}$";

		public static string OnlyBase64CertContent(string contents)
		{
			try
			{
				Regex base64RegexPattern = new Regex(BASE64_REGEX_STRING, RegexOptions.Compiled);

				contents = RemoveExtraCharactersFromCertContent(contents);

				string base64Encoded = Regex.Replace(contents, BEGIN_CERTIFICATE, "");
				base64Encoded = Regex.Replace(base64Encoded, END_CERTIFICATE, "");
				base64Encoded = Regex.Replace(base64Encoded, END_CERTIFICATE_W_EXTRACHARACTERS, "");
				base64Encoded = Regex.Replace(base64Encoded, PKCS10_HEADER, "");
				base64Encoded = Regex.Replace(base64Encoded, PKCS10_FOOTER, "");
				base64Encoded = Regex.Replace(base64Encoded, PKCS10_HEADER_NEW, "");
				base64Encoded = Regex.Replace(base64Encoded, PKCS10_FOOTER_NEW, "");

				//removes any whitespaces
				contents = Regex.Replace(base64Encoded, @"\s+", "");

				if (contents.Length % 4 != 0 || contents.Contains("\t") || contents.Contains("\r") || contents.Contains("\n") || (base64RegexPattern.Match(contents, 0).Success) == false)
				{
					string error = "Invalid certificate content as it contains non-Base64 characters.";
					return error;
				}

				return contents;
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static string RemoveExtraCharactersFromCertContent(string certContents)
		{
			//remove newline if any
			if (certContents.Contains("\r\n"))
			{
				certContents = Regex.Replace(certContents, "\r\n", "");
			}
			if (certContents.Contains("\r"))
			{
				certContents = Regex.Replace(certContents, "\r", "");
			}
			if (certContents.Contains("\n"))
			{
				certContents = Regex.Replace(certContents, "\n", "");
			}
			if (certContents.Contains("\t"))
			{
				certContents = Regex.Replace(certContents, "\t", "");
			}

			return certContents;
		}
	}
}
