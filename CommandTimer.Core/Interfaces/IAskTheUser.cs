namespace CommandTimer.Core.Interfaces;

public interface IAskTheUser {
    public Task<bool?> Ask(string title, string message, bool includeNo = false, bool includeCancel = false);
}

