global using System.Collections.Concurrent;
global using System.Net;
global using System.Net.Sockets;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Runtime.InteropServices;
using chatter.Tests;

// queue.Add(new DefaultInit());
// queue.Add(new DebugExHandlerInit());
// queue.Add(new HelloUser());

var ctx = new DefaultContext();

ctx.Enqueue(new PrinterTest(ctx));

var t = new Thread(ctx.Run);

t.Start();
