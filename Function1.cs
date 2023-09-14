using System;
using System.Diagnostics;
using System.IO;
using Azure;
using Azure.Communication.Email;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocxStorageUpdated
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public void Run([BlobTrigger("blobcontainer/{name}", Connection = "")] string blob, string name)
        {
	        var blobClient = new BlobClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "blobcontainer", name);
			
	        BlobProperties properties = blobClient.GetProperties();
            
	        var metadata = properties.Metadata;
	        var email = metadata["email"];

	        var sasToken = SetSasToken(name);

	        var sasUrl = GetSasUrl(name, sasToken);

			SendNotification(email, sasUrl);
        }

        public string SetSasToken(string blobName)
        {
	        BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
	        {
		        BlobContainerName = Environment.GetEnvironmentVariable("BlobContainerName"),
		        BlobName = blobName,
		        ExpiresOn = DateTime.UtcNow.AddHours(1)
	        };

	        blobSasBuilder.SetPermissions(BlobSasPermissions.Write);

	        var storageSharedKeyCredential = new StorageSharedKeyCredential(Environment.GetEnvironmentVariable("AccountName"), Environment.GetEnvironmentVariable("AccountKey"));

	        BlobSasQueryParameters sasQueryParameters = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential);

	        return sasQueryParameters.ToString();
        }

        public string GetSasUrl(string blobName, string sasToken)
        {
			//return "https" + $"{Environment.GetEnvironmentVariable("AccountName")}.blob.core.windows.net" + 
			UriBuilder fullUri = new UriBuilder()
	        {
		        Scheme = "https",
		        Host = string.Format("{0}.blob.core.windows.net", Environment.GetEnvironmentVariable("AccountName")),
		        Path = string.Format("{0}/{1}", Environment.GetEnvironmentVariable("BlobContainerName"), blobName),
		        Query = sasToken
	        };

			return fullUri.ToString();
		}

        public bool SendNotification(string receiverEmail, string fileUrl)
        {
	        string connectionString = Environment.GetEnvironmentVariable("EmailServiceConnectionString");
	        var emailClient = new EmailClient(connectionString);

	        var emailContent = new EmailContent("Docx file was uploaded")
	        {
		        PlainText = "File url with sas: " + fileUrl,
	        };

	        var emailMessage = new EmailMessage(
		        senderAddress: "DoNotReply@81000b7a-b30c-40c7-bb46-af81ee4ae59c.azurecomm.net",
				recipientAddress: receiverEmail,
		        content: emailContent);

	        try
	        {
		        EmailSendOperation emailSendOperation = emailClient.Send(WaitUntil.Completed, emailMessage);
				return true;
	        }
	        catch (RequestFailedException ex)
	        {
		        return false;
	        }
        }
    }
}
