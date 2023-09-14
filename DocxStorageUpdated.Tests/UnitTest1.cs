using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace DocxStorageUpdated.Tests
{
	public class UnitTest1
	{
		[Fact]
		public void GetSasUrl_BlobNameSasToken_ReturnCorrectSasUrl()
		{
			var mock = new Mock<ILoggerFactory>();
			ILoggerFactory loggerFactory = mock.Object;

			var func1 = new Function1(loggerFactory);

			var blobName = "blob";
			var sasToken = "token";
			var correctResult = "https://novikovstorageacc.blob.core.windows.net/blobcontainer/blob?token";

			var result = func1.GetSasUrl(blobName, sasToken);

			Assert.True(result.Equals(correctResult));
		}

		[Fact]
		public void SendNotification_ReceiverEmailFileUrl_ReturnTrue()
		{
			var mock = new Mock<ILoggerFactory>();
			ILoggerFactory loggerFactory = mock.Object;

			var func1 = new Function1(loggerFactory);

			var email = "novikov.oleksandr10@gmail.com";
			var url = "url";

			FillEnvironment();

			var res = func1.SendNotification(email, url);

			Assert.True(res);
		}

		[Fact]
		public void SendNotification_ReceiverEmailFileUrl_ReturnFalse()
		{
			var mock = new Mock<ILoggerFactory>();
			ILoggerFactory loggerFactory = mock.Object;

			var func1 = new Function1(loggerFactory);

			var email = "invalidemail";
			var url = "url";

			FillEnvironment();

			var res = func1.SendNotification(email, url);

			Assert.False(res);
		}
		private void FillEnvironment()
		{
			var settings = JsonConvert.DeserializeObject<LocalSettings>(
				File.ReadAllText("local.settings.json"));
			foreach (var setting in settings.Values)
			{
				Environment.SetEnvironmentVariable(setting.Key, setting.Value);
			}
		}
		class LocalSettings
		{
			public bool IsEncrypted { get; set; }
			public Dictionary<string, string> Values { get; set; }
		}
	}
}