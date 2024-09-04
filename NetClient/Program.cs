using System.Net;
using System.Net.Sockets;
using Serilog;


//初始化日志环境
Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug() //debug , info , warn , error
        .WriteTo.Console()
        .WriteTo.File("logs\\client-log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();


//服务器的IP、端口
var host = "127.0.0.1";
int port = 32510;
//服务器终端
IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(host), port);
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
socket.Connect(ipe);

Log.Information("成功连接到服务器");