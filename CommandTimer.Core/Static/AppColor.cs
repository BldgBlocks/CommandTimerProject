namespace CommandTimer.Core.Static;

public readonly record struct AppColor(byte A, byte R, byte G, byte B) {
    public static AppColor Transparent => new(0, 0, 0, 0);
    public static AppColor Fallback => new(255, 128, 0, 128);
}
