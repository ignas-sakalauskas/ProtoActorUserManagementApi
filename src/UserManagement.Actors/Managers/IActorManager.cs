using Proto;

namespace UserManagement.Actors.Managers
{
    public interface IActorManager
    {
        IRootContext Context { get; }
        PID GetParentActor();
        PID GetChildActor(string id, IContext parent);
        void RegisterChildActor<T>(T actor, string id, IContext parent) where T : IActor;
    }
}