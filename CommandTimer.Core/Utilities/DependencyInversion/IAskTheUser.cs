using System.Threading.Tasks;

namespace CommandTimer.Core.Utilities.DependencyInversion;
public interface IAskTheUser {
    public Task<bool?> Ask(string title, string message, bool includeNo = false, bool includeCancel = false);
}

