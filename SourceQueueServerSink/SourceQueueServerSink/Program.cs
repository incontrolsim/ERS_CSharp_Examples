using Ers;
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
            // Add component types
            ComponentRegistry<SourceBehavior>.Register();
            ComponentRegistry<QueueBehavior>.Register();
            ComponentRegistry<ServerBehavior>.Register();
            ComponentRegistry<SinkBehavior>.Register();
            ComponentRegistry<Product>.Register();
            ComponentRegistry<Resource>.Register();

            // Add event types
            LocalEventRegistry<SourceProduceProductEvent>.Register();
            LocalEventRegistry<QueueScheduleMoveOutEvent>.Register();
            LocalEventRegistry<QueueMoveOutEvent>.Register();
            LocalEventRegistry<ServerProcessEvent>.Register();
            LocalEventRegistry<ServerMoveOutEvent>.Register();

            ModelContainer modelContainer = ModelContainer.Create();
            Simulator simulator = modelContainer.AddSimulator("Sim1", SimulatorType.DiscreteEvent);
            simulator.EnterSubModel();

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
            ulong endTime = 86400 * model.Precision;
            while (model.CurrentTime < endTime)
            {
                // Run 1 second on each update step
                model.Update(1 * model.Precision);
            }
            ERS.Uninitialize();
        }
    }
}
