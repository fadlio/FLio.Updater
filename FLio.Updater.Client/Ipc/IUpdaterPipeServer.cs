namespace FLio.Updater.Client.Ipc;

public interface IUpdaterPipeServer
{
    Task<Version?> CheckForUpdate();
    Task Update();
}