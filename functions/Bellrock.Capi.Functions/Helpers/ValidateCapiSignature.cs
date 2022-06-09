using System;
using System.Buffers.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bellrock.Capi.Helpers;

public static class ValidateCapiSignature
{
    public static async Task<bool> IsValid(HttpRequest request, string secret, ILogger log)
    {
        if (!request.ContentLength.HasValue)
        {
            log.LogWarning("Request has no content length header. Cannot verify payload.");
            return false;
        }

        string signature = request.Headers["X-CAPI-SIGNATURE"];

        if (string.IsNullOrEmpty(signature))
        {
            log.LogWarning("Request has no capi signature header. Cannot verify payload.");
            return false;
        }

        // Create a Byte Array of size request.length + secret.length
        var bodyWithSecret = new byte[request.ContentLength.Value + secret.Length];

        using MemoryStream memoryStream = new MemoryStream();
        await request.Body.CopyToAsync(memoryStream);

        var bodyArray = memoryStream.ToArray();

        //  Populate the byte array with the contents of request.body
        memoryStream.ToArray().CopyTo(bodyWithSecret, 0);

        // Fill the remainder of the array with the bytes from your secret
        Encoding.ASCII.GetBytes(secret).CopyTo(bodyWithSecret, memoryStream.Length);

        // Generate an SHA256 hash of this new array
        var sha256Hash = SHA256.Create().ComputeHash(bodyWithSecret);

        // Base64 encode the hash
        var base64 = System.Convert.ToBase64String(sha256Hash);

        // Compare the value you created with the value of the X-CAPI-SIGNATURE header
        // If the values do not match, the request did not originate from this service
        return base64 == signature;
    }
}