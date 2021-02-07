using System.Collections.Generic;

namespace Flow.Launcher.Plugin.Putty
{
    public interface IPuttySessionService
    {
        /// <summary>
        /// Returns a List of all Putty Sessions
        /// </summary>
        /// <returns>A List of all Putty Sessions</returns>
        IEnumerable<PuttySession> GetAll();
    }
}