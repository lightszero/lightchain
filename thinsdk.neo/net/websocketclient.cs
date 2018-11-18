using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace lightdb.sdk
{

    public class BaseWebSocketClient
    {
        public event OnClientRecv OnRecv;
        public event OnDisConnect OnDisconnect;


        System.Net.WebSockets.ClientWebSocket websocket;

        System.Collections.Concurrent.ConcurrentQueue<byte[]> wantsend
            = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

        ulong sendMsgID = 0;

        public async Task Connect(Uri uri)
        {
            this.websocket = new System.Net.WebSockets.ClientWebSocket();
            try
            {
                await websocket.ConnectAsync(uri, System.Threading.CancellationToken.None);
                //peer.OnConnect(websocket);
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;
                Console.WriteLine("error on connect." + err.Message);
            }
            //此时调用一个不等待的msgprocessr
            MessageProcesser();
            MessageSender();

            return;
        }
        public async Task<UInt64> Send(sdk.NetMessage msg)
        {
            UInt64 _id = 0;
            lock (this)
            {
                _id = this.sendMsgID;
                this.sendMsgID++;
            }
            msg.Params["_id"] = BitConverter.GetBytes(_id);

            wantsend.Enqueue(msg.ToBytes());
            return _id;
        }

        async void MessageSender()
        {
            while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                if (wantsend.TryDequeue(out byte[] data))
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(data);
                    await websocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
                }
                else
                {
                    await Task.Delay(1);
                }
            }
        }
        async void MessageProcesser()
        {
            //recv
            try
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(1024 * 1024))
                {
                    byte[] buf = new byte[1024];
                    ArraySegment<byte> buffer = new ArraySegment<byte>(buf);
                    while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        var _recv = websocket.ReceiveAsync(buffer, System.Threading.CancellationToken.None);
                        Task.WaitAll(_recv);
                        var recv = _recv.Result;

                        ms.Write(buf, 0, recv.Count);
                        if (recv.EndOfMessage)
                        {
                            var count = ms.Position;
                            ms.Position = 0;
                            var msg = NetMessage.Unpack(ms);
                            var posend = ms.Position;
                            if (posend != count)
                                throw new Exception("error msg.");

                            //重置pos
                            ms.Position = 0;
                            await OnRecv(msg);// .onEvent(httpserver.WebsocketEventType.Recieve, websocket, bytes);
                        }
                        //Console.WriteLine("recv=" + recv.Count + " end=" + recv.EndOfMessage);
                    }
                }
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;

                Console.WriteLine("error on recv." + err.Message);
            }
            //disconnect
            try
            {
                await this?.OnDisconnect();
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;

                Console.WriteLine("error on disconnect." + err.Message);
            }
        }
    }
    public class Client_Snapshot
    {

    }
    public delegate Task OnClientRecv(NetMessage msg);
    public delegate Task OnDisConnect();
    public class Client
    {
        System.Net.WebSockets.ClientWebSocket websocket;

        public event OnClientRecv OnRecv_Unknown;
        public event OnDisConnect OnDisconnect;

        public bool Connected
        {
            get;
            private set;
        }

        ulong sendMsgID = 0;
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, string> wantMessage
            = new System.Collections.Concurrent.ConcurrentDictionary<ulong, string>();
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, sdk.NetMessage> gotMessage
            = new System.Collections.Concurrent.ConcurrentDictionary<ulong, NetMessage>();

        //System.Collections.Concurrent.ConcurrentQueue<byte[]> wantsend = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

        /// <summary>
        /// 链接
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// 
        public async Task Connect(Uri uri)
        {
            this.websocket = new System.Net.WebSockets.ClientWebSocket();
            try
            {
                await websocket.ConnectAsync(uri, System.Threading.CancellationToken.None);
                //peer.OnConnect(websocket);
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;
                Console.WriteLine("error on connect." + err.Message);
            }
            //此时调用一个不等待的msgprocessr
            MessageProcesser();
            //MessageSender();

            return;
        }
        public async Task<UInt64> Send(sdk.NetMessage msg)
        {
            UInt64 _id = 0;
            lock (this)
            {
                _id = this.sendMsgID;
                this.sendMsgID++;
            }

            msg.Params["_id"] = BitConverter.GetBytes(_id);

            var bytes = msg.ToBytes();
            ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
            await websocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);

            return _id;
        }
        public async Task<int> Ping()
        {
            Console.WriteLine("ping 00");
            var msg = sdk.NetMessage.Create("_ping");
            DateTime t0 = DateTime.Now;
            //_id是自动填的uint64
            //msg.Params["_id"] = new byte[4];
            try
            {
                Console.WriteLine("ping 01");
                var msgrecv = await PostMsg(msg, "_pingback");
                Console.WriteLine("ping 02");

            }
            catch (Exception err)
            {
                Console.WriteLine("err=Task<int> Ping");
            }
            DateTime t1 = DateTime.Now;
            return (int)((t1 - t0).TotalMilliseconds);
        }
        public async Task<bool> GetDBState()
        {
            var msg = sdk.NetMessage.Create("_getdbstate");
            var msgrecv = await PostMsg(msg, "_getdbstateback");

            return msgrecv.Params["db.open"][0] > 0;
        }
        public async Task<Client_Snapshot> UseSnapShot(long height = -1)
        {
            var msg = sdk.NetMessage.Create("_usesnapshot");
            var msgrecv = await PostMsg(msg, "_usesnapshotback");
            return null;
        }


        /////////////////////////////////////////////////////
        /// 内部逻辑

        public async Task<NetMessage> Wait(UInt64 msgid)
        {
            NetMessage recvmsg = null;
            while (true)
            {
                if (gotMessage.TryRemove(msgid, out recvmsg))
                {
                    break;
                }
                await Task.Yield();
            }
            return recvmsg;
        }
        private async Task<NetMessage> PostMsg(sdk.NetMessage msg, string backcmd)
        {
            //var _id = this.sendMsgID;
            //msg.Params["_id"] = BitConverter.GetBytes(this.sendMsgID);
            //this.sendMsgID++;
            UInt64 _id = await Send(msg);

            //想要这个消息
            wantMessage[_id] = backcmd;
            //加入发送队列
            //wantsend.Enqueue(msg.ToBytes());
            //var bytes = msg.ToBytes();
            //ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
            //await websocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);


            //还没搞清楚为何postmsg 一旦await 等待就会闪退
            var _wait = Wait(_id);
            Task.WaitAll(_wait);
            return _wait.Result;
            //return await Wait(_id);
            //await Task.Delay(1);
            //return  await Wait(_id);
        }
        //private async Task<NetMessage> Wait(UInt64 _id)
        //{

        //}
        async Task OnRecv(NetMessage message)
        {
            //如果有id
            if (message.Params.ContainsKey("_id") == true)
            {
                var iddata = message.Params["_id"];
                //如果8字节
                if (iddata.Length == 8)
                {
                    ulong id = BitConverter.ToUInt64(iddata, 0);
                    //在想要表里
                    if (wantMessage.TryRemove(id, out string wantcmd))
                    {
                        //名字也对的上
                        if (wantcmd == message.Cmd)
                        {
                            //放进得到表中
                            gotMessage[id] = message;
                            return;
                        }
                    }

                }
            }

            await this?.OnRecv_Unknown(message);

        }
        async void MessageSender()
        {
            //try
            //{
            //    while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
            //    {
            //        if (wantsend.TryDequeue(out byte[] data))
            //        {
            //            ArraySegment<byte> buffer = new ArraySegment<byte>(data);
            //            await websocket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
            //        }
            //        else
            //        {
            //            await Task.Delay(1);
            //        }
            //    }
            //}
            //catch (Exception err)
            //{
            //    Console.CursorLeft = 0;

            //    Console.WriteLine("error on recv." + err.Message);
            //}
        }
        async void MessageProcesser()
        {
            System.Threading.Thread t = new System.Threading.Thread(() =>
              {
                  //recv
                  try
                  {
                      using (System.IO.MemoryStream ms = new System.IO.MemoryStream(1024 * 1024))
                      {
                          byte[] buf = new byte[1024];
                          ArraySegment<byte> buffer = new ArraySegment<byte>(buf);
                          while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
                          {
                              var _recv = websocket.ReceiveAsync(buffer, System.Threading.CancellationToken.None);
                              Task.WaitAll(_recv);
                              var recv = _recv.Result;

                              ms.Write(buf, 0, recv.Count);
                              if (recv.EndOfMessage)
                              {
                                  var count = ms.Position;
                                  ms.Position = 0;
                                  var msg = NetMessage.Unpack(ms);
                                  Console.WriteLine("got msg:" + msg.Cmd);
                                  Console.WriteLine("got s " + recv.ToString());
                                  var posend = ms.Position;
                                  Console.WriteLine("posend=" + posend + " ,count=" + count);
                                  if (posend != count)
                                      throw new Exception("error msg.");

                                  //重置pos
                                  ms.Position = 0;
                                  Console.WriteLine("onrecv");
                                  Task.WaitAll(OnRecv(msg));
                                  //await OnRecv(msg);// .onEvent(httpserver.WebsocketEventType.Recieve, websocket, bytes);
                              }
                              //Console.WriteLine("recv=" + recv.Count + " end=" + recv.EndOfMessage);
                          }
                      }
                  }
                  catch (Exception err)
                  {
                      Console.CursorLeft = 0;

                      Console.WriteLine("error on recv." + err.Message);
                  }
                  //disconnect
                  try
                  {
                      Task.WaitAll(this?.OnDisconnect());
                  }
                  catch (Exception err)
                  {
                      Console.CursorLeft = 0;

                      Console.WriteLine("error on disconnect." + err.Message);
                  }
              });
            t.IsBackground = true;
            t.Start();
        }

    }
}
