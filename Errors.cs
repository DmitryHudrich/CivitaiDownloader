using Error = (System.String name, System.String desc);

internal static class Errors {
    public static Error BadInput = (name: nameof(BadInput), desc: "Url is not recognized.");
    public static Error PathNotSpecified = (name: nameof(PathNotSpecified), desc: $"Specify FULL downloding path via {Constants.DownloadPathEnvName}");
    public static Error PathSpecifiedIncorrectly = (name: nameof(PathSpecifiedIncorrectly), desc: $"Incorrect downloading path or specified path is inaccessible.");
    public static Error TokenIsNotSet = (name: nameof(TokenIsNotSet), desc: $"{Constants.TokenEnvName} is not set.");
    public static Error ContentLength = (name: nameof(ContentLength), desc: $"Zero size file.");
}

