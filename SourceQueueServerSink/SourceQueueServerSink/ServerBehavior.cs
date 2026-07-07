using System;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
    struct ServerProcessEvent : ILocalEvent<ServerProcessEvent>
    {
        public Entity ServerEntity;

        public void OnEvent()
        {
            ServerBehavior server = ServerEntity.GetComponent<ServerBehavior>();

            Entity child = ServerEntity.GetComponent<RelationComponent>().Value.First;
            var product = child.GetComponent<Product>();
            product.Value.Filled = true;
            Logger.Debug($"Server finished processing {child.GetName()}");
            server.ScheduleMoveOut();
        }
    }

    struct ServerMoveOutEvent : ILocalEvent<ServerMoveOutEvent>
    {
        public Entity ServerEntity;

        public void OnEvent()
        {
            ServerBehavior server = ServerEntity.GetComponent<ServerBehavior>();
            Entity child = ServerEntity.GetComponent<RelationComponent>().Value.First;
            SubModel.Get().UpdateParentOnEntity(child, server.Target);
        }
    }

    public class ServerBehavior : ScriptBehaviorComponent
    {
        public Entity Target;
        public ulong ProcessTime = 7;
        public ulong MoveOutTime = 3;

        /// <summary>
        /// Helper function to easily create a server entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static ServerBehavior Create(string name, Vector3 pos)
        {
            SubModel subModel = SubModel.Get();
            Entity entity = subModel.CreateEntity(name);
            var transform = entity.AddComponent<TransformComponent>();
            transform.Value.Position = pos;
            transform.Value.Scale = new Vector3(4, 2, 1);
            ServerBehavior server = entity.AddComponent<ServerBehavior>();
            entity.AddComponent<Resource>().Value.Capacity = 1;
            return server;
        }

        public override void OnEntered(Entity newChild)
        {
            ulong delay = ProcessTime;
            delay = SubModel.Get().ApplyModelPrecision(delay);
            EventScheduler.ScheduleLocalEvent(0, delay, new ServerProcessEvent() { ServerEntity = ConnectedEntity });
            Logger.Debug($"Server started processing {newChild.GetName()}");
        }

        public void ScheduleMoveOut()
        {
            ulong delay = MoveOutTime;
            delay = SubModel.Get().ApplyModelPrecision(delay);
            EventScheduler.ScheduleLocalEvent(0, delay, new ServerMoveOutEvent() { ServerEntity = ConnectedEntity });
        }
    }
}
