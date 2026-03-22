using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
//using System.ComponentModel;

namespace WinXound_Net
{

    public enum WinsockStates
    {
        Closed = 0,
        Open = 1,
        Listening = 2,
        ConnectionPending = 3,
        ResolvingHost = 4,
        HostResolved = 5,
        Connecting = 6,
        Connected = 7,
        Closing = 8,
        Error = 9
    }

    ///'''''''''''''''''''''''''''''''
    ///'EVENTS - DELEGATES
    //public delegate void DataArrivalHandler(object sender, string data);
    //public delegate void HandleErrorHandler(object sender, Int32 error_id, string error_string);
    //public delegate void StateChangedHandler(object sender, WinsockStates state);


    public class wxSocketBase
    {
        protected Form _WinForm = null;
        protected WinsockStates _State = WinsockStates.Closed;
        protected Socket _Client = null;

        protected Socket _Server = null;
        // State object for reading data asynchronously
        protected class SocketStateObject
        {
            // Active working socket.
            public Socket WorkingSocket = null;
            // Size of receiving buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize]; //+1 ???
        }


        ///'''''''''''''''''''''''''''''''
        ///'EVENTS - DELEGATES
        public delegate void DataArrivalHandler(object sender, string data);
        public event DataArrivalHandler DataArrival;

        public delegate void HandleErrorHandler(object sender, Int32 error_id, string error_string);
        public event HandleErrorHandler HandleError;

        public delegate void StateChangedHandler(object sender, WinsockStates state);
        public event StateChangedHandler StateChanged;


        ///'''''''''''''''''''''''''''''''
        ///'PROPERTIES
        public bool Connected
        {
            get { return _State == WinsockStates.Connected; }
        }
        public Form InvokeThrough
        {
            set { _WinForm = value; }
        }
        public WinsockStates GetState
        {
            get { return _State; }
        }


        ///'''''''''''''''''''''''''''''''
        ///'Methods for events
        protected void OnDataArrival(object sender, string data)
        {
            if (DataArrival != null)
                DataArrival(sender, data);
        }
        protected void OnHandleError(object sender, Int32 error_id, string error_string)
        {
            if (HandleError != null)
                HandleError(sender, error_id, error_string);
        }
        protected void OnStateChanged(object sender, WinsockStates state)
        {
            if (StateChanged != null)
                StateChanged(sender, state);
        }



        ///'''''''''''''''''''''''''''''''
        ///'METHODS
        public void Disconnect()
        {
            CloseSockets();
        }

        protected void CloseSockets()
        {
            try
            {
                //Close and release sockets
                if (_State != WinsockStates.Closed)
                {
                    if ((_Client != null))
                    {
                        //'_Client.Shutdown(SocketShutdown.Both)
                        _Client.Close();
                    }
                    if ((_Server != null))
                    {
                        _Server.Close();
                    }

                    ChangeState(WinsockStates.Closed);

                    _Client = null;
                    ///' ????
                    _Server = null;

                }

            }
            catch (SocketException ex)
            {
                ChangeState(WinsockStates.Error);
                //OnHandleError(ex.ErrorCode, ex.Message);
                //MyDebug("[CSocket.Close]" & vbCrLf & ex.Message)

            }
            catch (Exception ex)
            {
                //MyDebug("[CSocket.Close]" & vbCrLf & ex.Message)

            }
        }

        protected void ChangeState(WinsockStates new_state)
        {
            _State = new_state;
            //if (OnStateChanged != null)
                OnStateChanged(this, _State);
        }

        public Int32 SendLine(string Data)
        {
            if (_State == WinsockStates.Connected)
            {
                try
                {
                    //send the bytes that are passed
                    if ((_Client != null))
                    {
                        //"2C-9E-B4-F2-01-00-00-00" : two UINT32 (4 byte + 4 byte)
                        byte[] magic1 = BitConverter.GetBytes(Convert.ToUInt32(4071923244)); //L
                        byte[] magic2 = BitConverter.GetBytes(Convert.ToUInt32(Data.Length));
                        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(Data);
                        byte[] send = new byte[magic1.Length + magic2.Length + sendBytes.Length];
                        Int32 index = 0;
                        for (Int32 i = 0; i <= magic1.Length - 1; i++)
                        {
                            send[index] = magic1[i];
                            index += 1;
                        }
                        for (Int32 i = 0; i <= magic2.Length - 1; i++)
                        {
                            send[index] = magic2[i];
                            index += 1;
                        }
                        for (Int32 i = 0; i <= sendBytes.Length - 1; i++)
                        {
                            send[index] = sendBytes[i];
                            index += 1;
                        }

                        System.Diagnostics.Debug.WriteLine(BitConverter.ToString(send));

                        return _Client.Send(send);
                    }

                }
                catch (SocketException ex)
                {
                    OnHandleError(this, ex.ErrorCode, ex.Message);
                    ///'MyDebug("[SendLine]" & vbCrLf & ex.Message)
                    return 0;

                }
                catch (Exception ex)
                {
                    //MyDebug("[CSocket.SendLine]" & vbCrLf & ex.Message)
                    return 0;

                }
            }

            return 0;

        }

        protected void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                SocketStateObject so = (SocketStateObject)ar.AsyncState;
                Socket handler = so.WorkingSocket;
                int bytesRead = 0;

                // Read data from client socket. 
                bytesRead = handler.EndReceive(ar);

                //If bytesRead < 1 Then
                //    closesockets()
                //    OnHandleError(0, "DoStreamReceive")
                //    ''''MyDebug("[DoStreamReceive - intCount]")
                //    Exit Sub
                //End If


                if (bytesRead > 0)
                {
                    string strContent = null;
                    strContent = Encoding.Default.GetString(so.buffer, 8, bytesRead - 8);
                    //strContent = BitConverter.ToString(so.buffer, 0, bytesRead)

                    OnDataArrival(this, strContent);

                    // Get the rest of the data.
                    handler.BeginReceive(so.buffer, 0, SocketStateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), so);

                }

            }
            catch (SocketException ex)
            {
                CloseSockets();

                OnHandleError(this, ex.ErrorCode, ex.Message);

                ///'MyDebug("[ReadCallback]" & vbCrLf & ex.Message)

            }
            catch (Exception ex)
            {
                //MyDebug("[CSocket.Close]" & vbCrLf & ex.Message)

            }

        }

    }


    //***********************************************************************************************
    //******************************************* CLIENT ********************************************
    //***********************************************************************************************
    public class wxSocketClient : wxSocketBase
    {
        ///'''''''''''''''''''''''''''''''
        ///'CONSTRUCTOR
        public wxSocketClient(Form OwnerForm, string RemoteIP, Int32 RemotePort, Int32 IDNumber)
        {
            _WinForm = OwnerForm;
            _RemoteIP = RemoteIP;
            _RemotePort = RemotePort;
            _IDNumber = IDNumber;
        }


        ///'''''''''''''''''''''''''''''''
        ///'LOCAL TYPES
        protected string _RemoteIP = "";
        protected Int32 _RemotePort = 0;

        protected Int32 _IDNumber = 0;
        ///'Private _Client As Socket = Nothing


        ///'''''''''''''''''''''''''''''''
        ///'PROPERTIES
        public int RemotePort
        {
            get { return _RemotePort; }
            set
            {
                if (_State != WinsockStates.Connected)
                {
                    _RemotePort = value;
                }
            }
        }
        public string RemoteIP
        {
            get { return _RemoteIP; }
            set
            {
                if (_State == WinsockStates.Closed)
                {
                    _RemoteIP = value;
                }
            }
        }
        public Int32 GetIDNumber
        {
            get { return _IDNumber; }
        }


        ///'''''''''''''''''''''''''''''''
        ///'CONNECT

        public virtual void Connect(string RemoteIP, int RemotePort)
        {
            switch (_State)
            {
                case WinsockStates.Connected:
                case WinsockStates.ResolvingHost:
                case WinsockStates.HostResolved:
                case WinsockStates.Connecting:
                    return;

                    break;
            }

            if (!string.IsNullOrEmpty(RemoteIP))
            {
                _RemoteIP = RemoteIP;
            }
            if (RemotePort != 0)
            {
                _RemotePort = RemotePort;
            }



            try
            {
                if ((_Client != null))
                {
                    _Client.Close();
                }

                _Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                _Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, 1);
                _Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

                IPEndPoint rEP = new IPEndPoint(IPAddress.Parse(_RemoteIP), _RemotePort);
                System.Diagnostics.Debug.WriteLine("IPENDPOINT: " + rEP.ToString());
                ChangeState(WinsockStates.Connecting);
                _Client.BeginConnect(rEP, new AsyncCallback(ConnectCallback), _Client);

            }
            catch (SocketException ex)
            {
                ChangeState(WinsockStates.Error);
                //'CloseSockets()

                OnHandleError(this, ex.ErrorCode, ex.Message);

            }
            catch (Exception ex)
            {
                //MyDebug("[CSocket.Close]" & vbCrLf & ex.Message)

            }
        }


        protected void ConnectCallback(IAsyncResult ar)
        {

            try
            {
                Socket tempClient = (Socket)ar.AsyncState;

                //If Not tempClient.Connected Then
                //    ChangeState(WinsockStates.Error)
                //    OnHandleError(10061, "Connection refused")
                //    Exit Sub
                //End If

                // Complete the connection.
                tempClient.EndConnect(ar);

                ChangeState(WinsockStates.Connected);

                SocketStateObject so = new SocketStateObject();
                so.WorkingSocket = tempClient;
                tempClient.BeginReceive(so.buffer, 0, SocketStateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), so);

            }
            catch (SocketException ex)
            {
                ChangeState(WinsockStates.Error);
                //'CloseSockets()
                OnHandleError(this, ex.ErrorCode, ex.Message);

            }
            catch (Exception ex)
            {
                //MyDebug("[CSocket.Close]" & vbCrLf & ex.Message)

            }
        }


    }
    ///'CSOCKET CLIENT




    //***********************************************************************************************
    //******************************************* SERVER ********************************************
    //***********************************************************************************************
    public class wxSocketServer : wxSocketBase
    {
        ///'''''''''''''''''''''''''''''''
        ///'CONSTRUCTOR
        public wxSocketServer(Form OwnerForm, string LocalIP, Int32 LocalPort, Int32 IDNumber)
        {
            _WinForm = OwnerForm;
            _LocalIP = LocalIP;
            _LocalPort = LocalPort;
            _IDNumber = IDNumber;
        }


        ///'''''''''''''''''''''''''''''''
        ///'LOCAL TYPES
        protected string _LocalIP = "";
        protected Int32 _LocalPort = 0;
        //Protected _Server As Socket = Nothing
        //Protected _Client As Socket = Nothing

        protected Int32 _IDNumber = 0;

        ///'''''''''''''''''''''''''''''''
        ///'PROPERTIES
        public int LocalPort
        {
            get { return _LocalPort; }
            set
            {
                if (_State == WinsockStates.Closed)
                {
                    _LocalPort = value;
                }
            }
        }
        public string LocalIP
        {
            get { return _LocalIP; }
            set
            {
                if (_State == WinsockStates.Closed)
                {
                    _LocalIP = value;
                }
            }
        }
        public Int32 GetIDNumber
        {
            get { return _IDNumber; }
        }
        public string GetRemoteHostIP
        {
            get
            {
                if ((_Client != null))
                {
                    IPEndPoint iEP = (IPEndPoint)_Client.RemoteEndPoint;
                    return iEP.Address.ToString();
                }
                else
                {
                    return "";
                }
            }
        }


        ///'''''''''''''''''''''''''''''''
        ///SERVER LISTEN

        public void Listen()
        {
            switch (_State)
            {
                case WinsockStates.Listening:
                case WinsockStates.Connected:
                    ///'Debug.WriteLine(_State)
                    return;

                    break;
            }


            try
            {
                _Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                _Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, 1);
                _Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(_LocalIP), _LocalPort);
                _Server.Bind(ipLocal);
                _Server.Listen(1);
                ChangeState(WinsockStates.Listening);
                _Server.BeginAccept(new AsyncCallback(AcceptCallback), _Server);

            }
            catch (SocketException ex)
            {
                CloseSockets();
                OnHandleError(this, ex.ErrorCode, ex.Message);
                //MyDebug("[CSocket.StartListening]" & vbCrLf & ex.Message)

            }
            catch (Exception ex)
            {
                //MyDebug("[CSocket.StartListening]" & vbCrLf & ex.Message)

            }

        }

        protected void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                ChangeState(WinsockStates.Connected);

                // Create the state object.
                SocketStateObject so = new SocketStateObject();
                so.WorkingSocket = handler;
                _Client = handler;
                handler.BeginReceive(so.buffer, 0, SocketStateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), so);

            }
            catch (SocketException ex)
            {
                CloseSockets();
                OnHandleError(this, ex.ErrorCode, ex.Message);
                //MyDebug("[CSocket.AcceptCallback]" & vbCrLf & ex.Message)

            }
            catch (Exception ex)
            {
                //MyDebug("[CSocket.AcceptCallback]" & vbCrLf & ex.Message)

            }
        }

    }
    ///'CSOCKET SERVER





}
