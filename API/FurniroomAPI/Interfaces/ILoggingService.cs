using FurniroomAPI.Models.Log;

namespace FurniroomAPI.Interfaces
{
    public interface ILoggingService
    {
        public Task AddLogAsync(LogModel log);
    }
}
