using System.Numerics;
using Ers;
using Ers.Model;
using Ers.Visualization;
using Ers.WinForms;
using SourceQueueServerSink;

namespace GUI
{
    public partial class FormMain : Form
    {
        private ModelContainer modelContainer;
        private Texture productTexture;

        public FormMain()
        {
            InitializeComponent();

            this.comboBox2D3D.SelectedIndex = 0;
            this.comboBox3DCameraMode.Enabled = false;
            this.comboBox3DCameraMode.SelectedIndex = 0;
            this.KeyPreview = true;
            this.KeyDown += this.ersVisualization1.OnKeyDown;
            this.KeyUp += this.ersVisualization1.OnKeyUp;

            ersVisualization1.TargetFrameTime = 1000.0f / 144.0f;
            ersVisualization1.RenderEvent2D += Render2D;
            ersVisualization1.RenderEvent3D += Render3D;
            ersVisualization1.SelectedEntityChanged += Visualization_SelectedEntityChanged;
            ersVisualization1.Init();
            ersVisualization1.Camera2D.Position = new Vector2(5, 0);
            ersVisualization1.Camera2D.Zoom = 50.0f;
            ersVisualization1.Camera3D.LookAt = new Vector3(5, 0, 0);
            ersVisualization1.Camera3D.ZFar = 1000.0f;
            ersVisualization1.SetBackgroundColor(new Vector3(0.7f, 0.7f, 1.0f));

            ersRunControl1.Stop += StopTick;
            ersRunControl1.AttachObjects(Tick, ersClock1);

            ersTreeView1.AfterSelect += ErsTreeView_AfterSelect;

            modelContainer = Model.Create();

            ersTreeView1.AttachedModelContainer = modelContainer;
            Logger.SetLogLevel(LogLevel.Info);

            productTexture = new Texture("Assets/Tote_Top_White.png");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            productTexture.Dispose();
        }

        private void Tick(object? sender, EventArgs e)
        {
            modelContainer.Update(ersRunControl1.StepSize * modelContainer.GetPrecision());
        }

        private void StopTick(object? sender, EventArgs e)
        {
            ersTreeView1.ClearEntityTree();
            ersTreeView1.RebuildEntityTree();
            ersTreeView1.ExpandAll();
        }

        private void Render2D(object? sender, RenderEventArgs e)
        {
            Simulator simulator = modelContainer.GetSimulator(0);

            e.Context.DrawInfiniteGrid2D();

            simulator.EnterSubModel();
            SubModel subModel = SubModel.GetSubModel();

            // Visualize sources
            var sourceView = subModel.GetView<SourceBehavior, TransformComponent>([]);
            while (sourceView.Next())
            {
                var transform = sourceView.GetComponent<TransformComponent>();
                e.Context.DrawRect2D(transform.Value.GetPosition2D(), transform.Value.GetScale2D(), 0, new Vector4(0.01f, 0.39f, 0.43f, 1));
                Vector2 textPos = transform.Value.GetPosition2D() + new Vector2(-1.5f, 0.1f);
                e.Context.DrawText2D(sourceView.GetEntity().GetName(), textPos, 1);
            }
            sourceView.Dispose();

            // Visualize queues
            var queueView = subModel.GetView<QueueBehavior, TransformComponent, RelationComponent, Resource>([]);
            while (queueView.Next())
            {
                var transform = queueView.GetComponent<TransformComponent>();
                e.Context.DrawRect2D(transform.Value.GetPosition2D(), transform.Value.GetScale2D(), 0, new Vector4(0.0f, 0.5f, 0.75f, 1));
                Vector2 textPos = transform.Value.GetPosition2D() + new Vector2(-1.5f, 0.1f);
                e.Context.DrawText2D(queueView.GetEntity().GetName(), textPos, 1);

                ulong childCount = queueView.GetComponent<RelationComponent>().Value.ChildCount();
                ulong capacity = queueView.GetComponent<Resource>().Value.Capacity;
                e.Context.DrawText2D($"{childCount}/{capacity}", textPos + new Vector2(0.9f, -1), 1);
            }
            queueView.Dispose();

            // Visualize servers
            var serverView = subModel.GetView<ServerBehavior, TransformComponent, RelationComponent>([]);
            while (serverView.Next())
            {
                var transform = serverView.GetComponent<TransformComponent>();
                e.Context.DrawRect2D(transform.Value.GetPosition2D(), transform.Value.GetScale2D(), 0, new Vector4(0.86f, 0.46f, 0.02f, 1));
                Vector2 textPos = transform.Value.GetPosition2D() + new Vector2(-1.5f, 0.1f);
                e.Context.DrawText2D(serverView.GetEntity().GetName(), textPos, 1);

                var relation = serverView.GetComponent<RelationComponent>();
                if (relation.Value.ChildCount() > 0)
                {
                    Entity productEntity = relation.Value.First();
                    var product = productEntity.GetComponent<Product>();
                    Vector2 productPos = transform.Value.GetPosition2D() - new Vector2(0, 2);
                    Vector3 productColor;
                    if (product.Value.Filled)
                        productColor = new Vector3(0.0f, 0.89f, 0.47f);
                    else
                        productColor = new Vector3(1.0f, 0.18f, 0.18f);
                    e.Context.DrawTexture2D(productTexture, productPos, Vector2.One, 0, productColor);
                }
            }
            serverView.Dispose();

            // Visualize sinks
            var sinkView = subModel.GetView<SinkBehavior, TransformComponent>([]);
            while (sinkView.Next())
            {
                var transform = sinkView.GetComponent<TransformComponent>();
                e.Context.DrawRect2D(transform.Value.GetPosition2D(), transform.Value.GetScale2D(), 0, new Vector4(0.01f, 0.39f, 0.43f, 1));
                Vector2 textPos = transform.Value.GetPosition2D() + new Vector2(-1.5f, 0.1f);
                e.Context.DrawText2D(sinkView.GetEntity().GetName(), textPos, 1);

                SinkBehavior sink = sinkView.GetComponent<SinkBehavior>();
                e.Context.DrawText2D($"{sink.Received}", textPos - new Vector2(0, 1), 1);
            }
            sinkView.Dispose();

            simulator.ExitSubModel();
        }

        private void Render3D(object? sender, RenderEventArgs e)
        {
            e.Context.DrawInfiniteGrid3D();
        }

        private void Visualization_SelectedEntityChanged(object? sender, SelectedEntityEventArgs e)
        {
            propertyGrid1.SetSelectedEntity(ersVisualization1.SelectedEntity, e.ModelContainer.GetSimulator(e.SimulatorID));
        }

        private void ErsTreeView_AfterSelect(object? sender, ErsTreeViewEventArgs e)
        {
            if (ersTreeView1.SelectedNode.Tag == null)
            {
                propertyGrid1.SelectedObject = new object();
                return;
            }

            object selected = ersTreeView1.SelectedNode.Tag;
            if (selected.GetType() == typeof(Entity))
                propertyGrid1.SetSelectedEntity((Entity)selected, e.ModelContainer!.GetSimulator(e.SimulatorID!.Value));
            else
                propertyGrid1.SelectedObject = selected;
        }

        private void comboBox2D3D_SelectedIndexChanged(object sender, EventArgs e)
        {
            ersVisualization1.RenderMode = (RenderMode)((ComboBox)sender).SelectedIndex;
            this.comboBox3DCameraMode.Enabled = ersVisualization1.RenderMode == RenderMode.Render3D;
        }

        private void comboBox3DCameraMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.comboBox3DCameraMode.Enabled)
                return;

            int index = ((ComboBox)sender).SelectedIndex;
            ersVisualization1.SwitchCamera3DMode((Camera3DMode)index);
            this.splitContainerLeftMiddle.Panel2.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Application.Exit();
    }
}
