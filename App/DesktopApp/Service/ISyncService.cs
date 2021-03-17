using System;

namespace DesktopApp.Service
{
    public interface ISyncService
    {
        void Start();
        event EventHandler OnSync;
    }
}