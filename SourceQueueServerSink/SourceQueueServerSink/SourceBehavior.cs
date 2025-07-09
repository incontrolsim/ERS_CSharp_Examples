using System;
using System.ComponentModel;
using System.Numerics;
using Ers;

namespace SourceQueueServerSink
{
    public class SourceBehavior : ScriptBehaviorComponent
    {
        [Category("Source")]
        public Entity Target { get; set; }

        [Category("Source"), Description("The time (in seconds) it takes to generate a product.")]
        public ulong GenerationTime { get; set; } = 5;

        [Category("Source")]
        public ulong Produced { get; set; }

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
            transform.Value.SetPosition(pos);
            transform.Value.SetScale(4, 2, 1);
            SourceBehavior source = entity.AddComponent<SourceBehavior>();
            return source;
        }

        public override void OnStart()
        {
            EventScheduler.ScheduleLocalEvent(0, GenerationTime, ProduceProduct);
        }

        private void ProduceProduct()
        {
            SubModel subModel = SubModel.GetSubModel();

            if (Target.GetComponent<RelationComponent>().Value.ChildCount() < Target.GetComponent<Resource>().Value.Capacity)
            {
                // Create new product
                Entity entity = subModel.CreateEntity($"Product{Produced + 1}");
                entity.AddComponent<Product>();
                Logger.Debug($"Source created product: {entity.GetName()}");

                // Move product
                subModel.UpdateParentOnEntity(entity, Target);
                Produced++;
            }

            ulong delay = GenerationTime * subModel.ModelPrecision;
            EventScheduler.ScheduleLocalEvent(0, delay, ProduceProduct);
        }
    }
}
