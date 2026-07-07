using System;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
    struct QueueScheduleMoveOutEvent() : ILocalEvent<QueueScheduleMoveOutEvent>
    {
        public Entity QueueEntity;

        public void OnEvent()
        {
            QueueBehavior queue = QueueEntity.GetComponent<QueueBehavior>();
            queue.ScheduleMoveOut();
        }
    }

    struct QueueMoveOutEvent : ILocalEvent<QueueMoveOutEvent>
    {
        public Entity QueueEntity;

        public void OnEvent()
        {
            SubModel subModel = SubModel.Get();
            QueueBehavior queue = QueueEntity.GetComponent<QueueBehavior>();

            var relation = QueueEntity.GetComponent<RelationComponent>();
            subModel.UpdateParentOnEntity(relation.Value.First, queue.Target);

            // If there are more products left in the queue, schedule the move for the next one
            if (relation.Value.ChildCount > 0)
            {
                ulong retryDelay = queue.RetryTime * subModel.ModelPrecision;
                EventScheduler.ScheduleLocalEvent(0, retryDelay, new QueueScheduleMoveOutEvent() { QueueEntity = QueueEntity });
            }
        }
    }

    public class QueueBehavior : ScriptBehaviorComponent
    {
        public Entity Target;
        public ulong RetryTime = 3;

        /// <summary>
        /// Helper function to easily create a queue entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static QueueBehavior Create(string name, Vector3 pos, ulong capacity)
        {
            SubModel subModel = SubModel.Get();
            Entity entity = subModel.CreateEntity(name);
            var transform = entity.AddComponent<TransformComponent>();
            transform.Value.Position = pos;
            transform.Value.Scale = new Vector3(4, 2, 1);
            QueueBehavior queue = entity.AddComponent<QueueBehavior>();
            entity.AddComponent<Resource>().Value.Capacity = capacity;
            return queue;
        }

        public override void OnEntered(Entity newChild)
        {
            var relation = ConnectedEntity.GetComponent<RelationComponent>();
            ulong childCount = relation.Value.ChildCount;
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
        public void ScheduleMoveOut()
        {
            if (Target.GetComponent<RelationComponent>().Value.ChildCount >= Target.GetComponent<Resource>().Value.Capacity)
            {
                ulong retryDelay = RetryTime;
                retryDelay = SubModel.Get().ApplyModelPrecision(retryDelay);
                EventScheduler.ScheduleLocalEvent(0, retryDelay, new QueueScheduleMoveOutEvent() { QueueEntity = ConnectedEntity });
                return;
            }

            ulong delay = 1;
            delay = SubModel.Get().ApplyModelPrecision(delay);
            EventScheduler.ScheduleLocalEvent(0, delay, new QueueMoveOutEvent() { QueueEntity = ConnectedEntity });
        }
    }
}
