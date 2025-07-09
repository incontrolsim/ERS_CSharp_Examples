using System;
using System.ComponentModel;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
    public class QueueBehavior : ScriptBehaviorComponent
    {
        [Category("Queue")]
        public Entity Target { get; set; }

        [Category("Queue"), Description("The time (in seconds) to wait before retrying to move an entity.")]
        public ulong RetryTime { get; set; } = 3;

        /// <summary>
        /// Helper function to easily create a queue entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static QueueBehavior Create(string name, Vector3 pos, ulong capacity)
        {
            SubModel subModel = SubModel.GetSubModel();
            Entity entity = subModel.CreateEntity(name);
            var transform = entity.AddComponent<TransformComponent>();
            transform.Value.SetPosition(pos);
            transform.Value.SetScale(4, 2, 1);
            QueueBehavior queue = entity.AddComponent<QueueBehavior>();
            entity.AddComponent<Resource>().Value.Capacity = capacity;
            return queue;
        }

        public override void OnEntered(Entity newChild)
        {
            var relation = ConnectedEntity.GetComponent<RelationComponent>();
            ulong childCount = relation.Value.ChildCount();
            ulong capacity = ConnectedEntity.GetComponent<Resource>().Value.Capacity;

            Logger.Debug($"Queue received {newChild.GetName()}, capacity: {childCount}/{capacity}");
            // Only schedule if there previously was no child (thus no exit scheduled)
            if (childCount == 1)
            {
                ScheduleMoveOut();
            }
        }

        /// <summary>
        /// Move products directly to the target after one second.
        /// </summary>
        private void ScheduleMoveOut()
        {
            SubModel subModel = SubModel.GetSubModel();

            if (Target.GetComponent<RelationComponent>().Value.ChildCount() >= Target.GetComponent<Resource>().Value.Capacity)
            {
                ulong retryDelay = RetryTime * subModel.ModelPrecision;
                EventScheduler.ScheduleLocalEvent(0, retryDelay, ScheduleMoveOut);
                return;
            }

            ulong delay = subModel.ApplyModelPrecision(1);
            EventScheduler.ScheduleLocalEvent(0, delay, () =>
            {
                SubModel subModel = SubModel.GetSubModel();

                var relation = ConnectedEntity.GetComponent<RelationComponent>();
                subModel.UpdateParentOnEntity(relation.Value.First(), Target);

                // If there are more products left in the queue, schedule the move for the next one
                if (relation.Value.ChildCount() > 0)
                {
                    ulong retryDelay = RetryTime * subModel.ModelPrecision;
                    EventScheduler.ScheduleLocalEvent(0, retryDelay, ScheduleMoveOut);
                }
            });
        }
    }
}
