using System.Numerics;
using Ers;
using SourceQueueServerSink;

namespace GUI
{
    internal static class Program
    {
        static ModelContainer? modelContainer;
        static Texture? productTexture;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ERS.Initialize();
            Ers.Debugger.Open();
            Logger.SetLogLevel(LogLevel.Trace);

            modelContainer = SourceQueueServerSink.Model.Create();

            Simulator sim = modelContainer.GetSimulatorByIndex(0);
            sim.EnterSubModel();
            productTexture = new Texture("Tote_Top_White.png");
            sim.ExitSubModel();

            Ers.Debugger.Run(modelContainer, Render2D);

            ERS.Uninitialize();
        }

        static void Render2D(Debugger debugger, Simulator simulator)
        {
            RenderContext context = debugger.RenderContext;

            simulator.EnterSubModel();
            SubModel subModel = SubModel.Get();

            // Visualize sources
            var sourceView = subModel.GetView<SourceBehavior, TransformComponent>([]);
            while (sourceView.Next())
            {
                var transform = sourceView.GetComponent<TransformComponent>();
                context.DrawRect2D(transform.Value.Position.XY(), transform.Value.Scale.XY(), 0, Ers.Color.FromFloats(0.01f, 0.39f, 0.43f, 1));
                Vector2 textPos = transform.Value.Position.XY() + new Vector2(-1.5f, 0.1f);
                context.DrawText2D(sourceView.GetEntity().GetName(), textPos, 1);
            }
            sourceView.Dispose();

            // Visualize queues
            var queueView = subModel.GetView<QueueBehavior, TransformComponent, RelationComponent, Resource>([]);
            while (queueView.Next())
            {
                var transform = queueView.GetComponent<TransformComponent>();
                context.DrawRect2D(transform.Value.Position.XY(), transform.Value.Scale.XY(), 0, Ers.Color.FromFloats(0.0f, 0.5f, 0.75f, 1));
                Vector2 textPos = transform.Value.Position.XY() + new Vector2(-1.5f, 0.1f);
                context.DrawText2D(queueView.GetEntity().GetName(), textPos, 1);

                ulong childCount = queueView.GetComponent<RelationComponent>().Value.ChildCount;
                ulong capacity = queueView.GetComponent<Resource>().Value.Capacity;
                context.DrawText2D($"{childCount}/{capacity}", textPos + new Vector2(0.9f, -1), 1);
            }
            queueView.Dispose();

            // Visualize servers
            var serverView = subModel.GetView<ServerBehavior, TransformComponent, RelationComponent>([]);
            while (serverView.Next())
            {
                var transform = serverView.GetComponent<TransformComponent>();
                context.DrawRect2D(transform.Value.Position.XY(), transform.Value.Scale.XY(), 0, Ers.Color.FromFloats(0.86f, 0.46f, 0.02f, 1));
                Vector2 textPos = transform.Value.Position.XY() + new Vector2(-1.5f, 0.1f);
                context.DrawText2D(serverView.GetEntity().GetName(), textPos, 1);

                var relation = serverView.GetComponent<RelationComponent>();
                if (relation.Value.ChildCount > 0)
                {
                    Entity productEntity = relation.Value.First;
                    var product = productEntity.GetComponent<Product>();
                    Vector2 productPos = transform.Value.Position.XY() - new Vector2(0, 2);
                    Color productColor;
                    if (product.Value.Filled)
                        productColor = Ers.Color.FromFloats(0.0f, 0.89f, 0.47f);
                    else
                        productColor = Ers.Color.FromFloats(1.0f, 0.18f, 0.18f);
                    context.DrawTexture2D(productTexture!, productPos, Vector2.One, 0, productColor);
                }
            }
            serverView.Dispose();

            // Visualize sinks
            var sinkView = subModel.GetView<SinkBehavior, TransformComponent>([]);
            while (sinkView.Next())
            {
                var transform = sinkView.GetComponent<TransformComponent>();
                context.DrawRect2D(transform.Value.Position.XY(), transform.Value.Scale.XY(), 0, Ers.Color.FromFloats(0.01f, 0.39f, 0.43f, 1));
                Vector2 textPos = transform.Value.Position.XY() + new Vector2(-1.5f, 0.1f);
                context.DrawText2D(sinkView.GetEntity().GetName(), textPos, 1);

                SinkBehavior sink = sinkView.GetComponent<SinkBehavior>();
                context.DrawText2D($"{sink.Received}", textPos - new Vector2(0, 1), 1);
            }
            sinkView.Dispose();

            RenderSystem.Render2D(subModel, context);
            simulator.ExitSubModel();
        }
    }
}
