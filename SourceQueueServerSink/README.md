# Source Queue Server Sink ERS example model

This model serves as an example of how to use ERS to build a model with a source, queue, server, and sink—similar to those found in Enterprise Dynamics.

The simulation creates empty products in the source, which are empty at the start.
The products are then moved to a queue, where they wait until they can be processed by the server.
The server fills the products one at a time.
Finally, the products leave the simulation through the sink, which keeps a counter of the received number of products.

The solution consists of two projects:
- `SourceQueueServerSink`: The simulation components and model.
- `GUI`: A visualization of the model using the ERS Debugger. It depends on the SourceQueueServerSink project to run the same model.
