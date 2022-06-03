namespace FLio.Updater.Client.Ipc;

public interface IUpdaterPipeClient
{
    void Shutdown();
    void NotifyNewVersion(Version version);
}