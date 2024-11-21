using Error = (System.String name, System.String desc);

namespace CheckpointDownloader;

internal static class Downloader {
    private static readonly List<Error> errors = [];

    public static async Task RunAsync(String[] args) {

        var obtainedToken =
            Environment.GetEnvironmentVariable(Constants.TokenEnvName);
        (obtainedToken is not null).OrThen(static () => errors.Add(Errors.TokenIsNotSet));

        var isUrlGood =
            Uri.TryCreate(args[0], UriKind.Absolute, out var uriResult)
            && uriResult.Scheme == Uri.UriSchemeHttps;
        isUrlGood.OrThen(static () => errors.Add(Errors.BadInput));

        var downloadingPath =
            Environment.GetEnvironmentVariable(Constants.DownloadPathEnvName);
        (downloadingPath is not null).OrThen(static () => errors.Add(Errors.PathNotSpecified));
        CheckErrors();
        IsPathValid(downloadingPath!).OrThen(static () => errors.Add(Errors.PathSpecifiedIncorrectly));
        CheckErrors();
        Console.WriteLine("Downloading path: " + downloadingPath);
        DeletePreviousSafeTensors(downloadingPath!);

        var downloadingUrl = uriResult!.OriginalString.Contains('?') switch {
            true => uriResult.OriginalString + '&' + "token=" + obtainedToken,
            false => uriResult.OriginalString + '?' + "token=" + obtainedToken,
        };

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(downloadingUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        contentLength.HasValue.OrThen(static () => errors.Add(Errors.ContentLength));
        CheckErrors();

        using var contentStream = await response.Content.ReadAsStreamAsync();
        await SaveToFileWithProgressAsync(contentStream, downloadingPath + "privet-ot-detey-kubani.safetensors", contentLength!.Value);

    }

    private static async Task SaveToFileWithProgressAsync(Stream contentStream, String filePath, Int64 totalBytes) {
        const Int32 bufferSize = 81920;
        Int64 totalRead = 0;
        var buffer = new Byte[bufferSize];
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

        Int32 bytesRead;
        do {
            bytesRead = await contentStream.ReadAsync(buffer);
            if (bytesRead > 0) {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                var progress = (Double)totalRead / totalBytes * 100;
                Console.Write($"\rProgress: {progress:F2}% ({totalRead.ToGb():F2}/{totalBytes.ToGb():F2} Gb)");
            }
        } while (bytesRead > 0);
        Console.WriteLine("That's all.");
    }

    private static void CheckErrors() => (errors.Count != 0).AndThen(static () => {
        errors.ForEach(static error => Console.WriteLine(error.desc));
        Environment.Exit(128);
    });

    private static Boolean IsPathValid(String filePath) {
        try {
            if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
                return false;
            }
            var directoryPath = Path.GetDirectoryName(filePath);
            if (String.IsNullOrEmpty(directoryPath)) {
                return false;
            }
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            var testFile = Path.Combine(directoryPath, "test.tmp");
            using var fs = File.Create(testFile, 1, FileOptions.DeleteOnClose);

            return true;
        }
        catch {
            errors.Add(Errors.PathSpecifiedIncorrectly);
            return false;
        }
    }

    private static void DeletePreviousSafeTensors(String directoryPath) {
        var files = Directory.GetFiles(directoryPath, "*.safetensors");
        foreach (var f in files) {
            try {
                File.Delete(f);
                Console.WriteLine($"Deleted: {f}");
            }
            catch (Exception ex) {
                Console.WriteLine($"Error while deleting {f}: {ex.Message}");
            }
        }
    }
}

