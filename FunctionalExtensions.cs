namespace CheckpointDownloader;

internal static class Extensions {
    public static void OrThen(this Boolean v, Action action) {
        if (!v) {
            action();
        }
    }
    public static void AndThen(this Boolean v, Action action) {
        if (v) {
            action();
        }
    }

    public static Double ToGb(this Int64 bytes) {
        return (Double)bytes / 1024 / 1024 / 1024;
    }
}

