using System;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
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
            SubModel subModel = SubModel.GetSubModel();
            Entity entity = subModel.CreateEntity(name);
            var transform = entity.AddComponent<TransformComponent>();
            transform.Value.Position = pos;
            transform.Value.Scale = new Vector3(4, 2, 1);
            SourceBehavior source = entity.AddComponent<SourceBehavior>();
            return source;
        }

        public override void OnStart()
        {
            EventScheduler.ScheduleLocalEvent(0, GenerationTime, ProduceProduct);
        }

        private void ProduceProduct()
        {
            if (Target.GetComponent<RelationComponent>().Value.ChildCount() < Target.GetComponent<Resource>().Value.Capacity)
            {
                // Create new product
                SubModel subModel = SubModel.GetSubModel();
                Entity entity = subModel.CreateEntity($"Product{Produced + 1}");
                entity.AddComponent<Product>();
                Logger.Debug($"Source created product: {entity.GetName()}");

                // Move product
                subModel.UpdateParentOnEntity(entity, Target);
                Produced++;
            }

            ulong delay = GenerationTime;
            delay = SubModel.GetSubModel().ApplyModelPrecision(delay);
            EventScheduler.ScheduleLocalEvent(0, delay, ProduceProduct);
        }
    }
}
