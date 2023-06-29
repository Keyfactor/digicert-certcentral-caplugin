using Keyfactor.Logging;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAGateway.DigiCert.Client
{
	/// <summary>
	/// Static class containing some utility methods for the cert central client.
	/// </summary>
	public static class CertCentralClientUtilities
	{
		/// <summary>
		/// Private instance of the logger.
		/// </summary>
		private static ILogger Logger => LogHandler.GetClassLogger<CertCentralCAConnector>();

		/// <summary>
		/// Uses the <see cref="CertCentralConfig"/> to build a DigiCert client.
		/// </summary>
		/// <param name="Config"></param>
		/// <returns></returns>
		public static CertCentralClient BuildCertCentralClient(CertCentralConfig Config)
		{
			Logger.LogTrace("Entered BuildCertCentralClient");
			try
			{
				Logger.LogTrace("Building CertCentralClient with retrieved configuration information");
				string apiKey = Config.APIKey;
				string region = Config.Region;
				return new CertCentralClient(apiKey, region);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to build CertCentralClient Client web service client", ex);
			}
		}
	}
}
