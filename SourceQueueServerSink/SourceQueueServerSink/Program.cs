using Ers;
using Ers.Model;
using Ers.Platform;
using System.Numerics;

namespace SourceQueueServerSink
{
    /// <summary>
    /// A Source, Queue, Server, Sink model that:
    /// <list type="number">
    ///     <item>Spawn an empty tote at the source.</item>
    ///     <item>Queues the tote before the server.</item>
    ///     <item>Fills one tote at a time at the server.</item>
    ///     <item>Exits totes via te sink.</item>
    /// </list>
    /// </summary>
    public class Model
    {
        public static ModelContainer Create()
        {
            ModelContainer modelContainer = ModelContainer.CreateModelContainer();
            Simulator simulator = modelContainer.AddSimulator("Sim1", SimulatorType.DiscreteEvent);

            simulator.EnterSubModel();
            SubModel subModel = SubModel.GetSubModel();

            // Add component types
            subModel.AddComponentType<SourceBehavior>();
            subModel.AddComponentType<QueueBehavior>();
            subModel.AddComponentType<ServerBehavior>();
            subModel.AddComponentType<SinkBehavior>();
            subModel.AddComponentType<Product>();
            subModel.AddComponentType<Resource>();

            SourceBehavior source1 = SourceBehavior.Create("Source1", new Vector3(0, 0, 0));
            QueueBehavior queue1 = QueueBehavior.Create("Queue1", new Vector3(5, 0, 0), 5);
            ServerBehavior server1 = ServerBehavior.Create("Server1" , new Vector3(10, 0, 0));
            SinkBehavior sink1 = SinkBehavior.Create("Sink1", new Vector3(15, 0, 0));

            source1.Target = queue1.ConnectedEntity;
            queue1.Target = server1.ConnectedEntity;
            server1.Target = sink1.ConnectedEntity;

            simulator.ExitSubModel();
            return modelContainer;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            ERS.Initialize();
            Logger.SetLogLevel(LogLevel.Trace);

            ModelContainer model = Model.Create();

            // Run for a total of 86400 seconds (1 day)
            ulong endTime = 3600 * model.GetPrecision();
            while (model.CurrentTime < endTime)
            {
                // Run 1 second on each update step
                model.Update(1 * model.GetPrecision());
            }
            ERS.Uninitialize();
        }
    }
}
