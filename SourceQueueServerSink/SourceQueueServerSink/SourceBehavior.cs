using System;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
    struct SourceProduceProductEvent : ILocalEvent<SourceProduceProductEvent>
    {
        public Entity SourceEntity;

        public void OnEvent()
        {
            SubModel subModel = SubModel.Get();
            SourceBehavior source = SourceEntity.GetComponent<SourceBehavior>();

            if (source.Target.GetComponent<RelationComponent>().Value.ChildCount < source.Target.GetComponent<Resource>().Value.Capacity)
            {
                // Create new product
                Entity entity = subModel.CreateEntity($"Product{source.Produced + 1}");
                entity.AddComponent<Product>();
                Logger.Debug($"Source created product: {entity.GetName()}");

                // Move product
                subModel.UpdateParentOnEntity(entity, source.Target);
                source.Produced++;
            }

            ulong delay = source.GenerationTime;
            delay = SubModel.Get().ApplyModelPrecision(delay);
            EventScheduler.ScheduleLocalEvent(0, delay, new SourceProduceProductEvent() { SourceEntity = SourceEntity });
        }
    }

    public class SourceBehavior : ScriptBehaviorComponent
    {
        public Entity Target;
        public ulong GenerationTime = 5;
        public ulong Produced = 0;

        /// <summary>
        /// Helper function to easily create a source entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static SourceBehavior Create(string name, Vector3 pos)
        {
            SubModel subModel = SubModel.Get();
            Entity entity = subModel.CreateEntity(name);
            var transform = entity.AddComponent<TransformComponent>();
            transform.Value.Position = pos;
            transform.Value.Scale = new Vector3(4, 2, 1);
            SourceBehavior source = entity.AddComponent<SourceBehavior>();
            return source;
        }

        public override void OnStart()
        {
            EventScheduler.ScheduleLocalEvent(0, GenerationTime, new SourceProduceProductEvent() { SourceEntity = ConnectedEntity });
        }
    }
}
