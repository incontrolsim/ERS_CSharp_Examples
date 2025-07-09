using System;
using System.ComponentModel;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
    public class ServerBehavior : ScriptBehaviorComponent
    {
        [Category("Server")]
        public Entity Target { get; set; }

        [Category("Server")]
        public ulong ProcessTime { get; set; } = 7;

        [Category("Server")]
        public ulong MoveOutTime { get; set; } = 3;

        /// <summary>
        /// Helper function to easily create a server entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static ServerBehavior Create(string name, Vector3 pos)
        {
            SubModel subModel = SubModel.GetSubModel();
            Entity entity = subModel.CreateEntity(name);
            var transform = entity.AddComponent<TransformComponent>();
            transform.Value.SetPosition(pos);
            transform.Value.SetScale(4, 2, 1);
            ServerBehavior server = entity.AddComponent<ServerBehavior>();
            entity.AddComponent<Resource>().Value.Capacity = 1;
            return server;
        }

        public override void OnEntered(Entity newChild)
        {
            ulong delay = ProcessTime * SubModel.GetSubModel().ModelPrecision;
            EventScheduler.ScheduleLocalEvent(0, delay, ProcessProduct);
            Logger.Debug($"Server started processing {newChild.GetName()}");
        }

        private void ProcessProduct()
        {
            Entity child = ConnectedEntity.GetComponent<RelationComponent>().Value.First();
            var product = child.GetComponent<Product>();
            product.Value.Filled = true;
            Logger.Debug($"Server finished processing {child.GetName()}");
            ScheduleMoveOut();
        }

        private void ScheduleMoveOut()
        {
            ulong delay = MoveOutTime * SubModel.GetSubModel().ModelPrecision;
            EventScheduler.ScheduleLocalEvent(0, delay, () =>
            {
                Entity child = ConnectedEntity.GetComponent<RelationComponent>().Value.First();
                SubModel.GetSubModel().UpdateParentOnEntity(child, Target);
            });
        }
    }
}
