using IrcDotNet;
using libtoxtmi.Events;
using libtoxtmi.Models;
using libtoxtmi.Tmi;
using libtoxtmi.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace libtoxtmi
{
    public class TmiChat
    {
        public const string IRC_HOSTNAME = "irc.chat.twitch.tv";
        public const int IRC_PORT_NONSSL = 6667;

        public enum ClientStatus
        {
            Stopped = 0,
            Connecting = 1,
            Started = 2
        }
        
        public ClientStatus Status
        {
            get;
            protected set;
        }

        protected bool KeepThreadsAlive
        {
            get
            {
                return Status == ClientStatus.Connecting ||
                    Status == ClientStatus.Started;
            }
        }

        protected string userName;
        protected string password;
        protected TwitchIrcClient client;
        protected List<string> channelList;
        protected Queue<string> channelJoinQueue;
        protected Thread channelJoinQueueThread;

        private object controlSyncLock = new object();

        /// <summary>
        /// Event for client state change: started and connected.
        /// </summary>
        public event EventHandler<EventArgs> OnStarted;
        /// <summary>
        /// Event for client state change: disconnected and shutting down.
        /// </summary>
        public event EventHandler<EventArgs> OnStopped;
        /// <summary>
        /// Event for incoming TMI chat messages.
        /// </summary>
        public event EventHandler<TmiChatMessageEventArgs> OnChatMessage;

        public TmiChat(string userName, string password)
        {
            Status = ClientStatus.Stopped;

            this.userName = userName;
            this.password = password;

            channelList = new List<string>();
            channelJoinQueue = new Queue<string>();
        }

        #region Start / Stop / Status
        /// <summary>
        /// Starts the client, connecting to IRC server, and beginning the join queue.
        /// </summary>
        public void Start()
        {
            lock (controlSyncLock)
            {
                Stop();

                Status = ClientStatus.Connecting;
                TmiLog.Log($"Starting client. Connecting to irc://{IRC_HOSTNAME}:{IRC_PORT_NONSSL}...");

                // Init IRC
                client = new TwitchIrcClient();

                client.Connected += Client_Connected;
                client.Registered += Client_Registered;
                client.ConnectFailed += Client_ConnectFailed;
                client.ProtocolError += Client_ProtocolError;
                client.ErrorMessageReceived += Client_ErrorMessageReceived;
                client.Error += Client_Error;
                client.MotdReceived += Client_MotdReceived;
                client.Disconnected += Client_Disconnected;
                client.RawMessageReceived += Client_RawMessageReceived;
                client.RawMessageSent += Client_RawMessageSent; ;

                client.Connect(new DnsEndPoint(IRC_HOSTNAME, IRC_PORT_NONSSL), false, new IrcUserRegistrationInfo()
                {
                    UserName = this.userName,
                    NickName = this.userName,
                    Password = this.password
                });

                // Fill the channel join queue, and start the queue thread
                channelJoinQueue.Clear();
                channelList.ForEach((channel) => channelJoinQueue.Enqueue(channel));

                // Start the queue thread
                channelJoinQueueThread = new Thread(new ThreadStart(__RunChannelJoinQueue));
                channelJoinQueueThread.Name = "Tmi__RunChannelJoinQueue";
                channelJoinQueueThread.Priority = ThreadPriority.BelowNormal;
                channelJoinQueueThread.Start();
            }
        }

        /// <summary>
        /// Shuts down the client, disconnecting from the IRC server, and shutting down any of our threads.
        /// </summary>
        public void Stop()
        {
            lock (controlSyncLock)
            {
                // Ensure client is killed dead
                if (client != null)
                {
                    if (client.IsConnected)
                    {
                        try
                        {
                            client.Disconnect();
                        }
                        catch (Exception) { }
                    }
                }

                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }

                // Switch status & raise event if we aren't already in stopped state
                if (Status != ClientStatus.Stopped)
                {
                    TmiLog.Log("Stopping client...");

                    if (OnStopped != null)
                        OnStopped.Invoke(this, new EventArgs());
                }

                Status = ClientStatus.Stopped;

                // Kick the queue thread so it quits faster
                if (channelJoinQueueThread != null)
                {
                    try
                    {
                        channelJoinQueueThread.Interrupt();
                    }
                    catch (Exception) { }

                    channelJoinQueueThread = null;
                }
            }
        }
        #endregion

        #region Client events
        /// <summary>
        /// IRC event: connection established.
        /// </summary>
        private void Client_Connected(object sender, EventArgs e)
        {
            TmiLog.Log("Connected to Twitch IRC server.");

            client.SendRawMessage("CAP REQ :twitch.tv/membership");
            client.SendRawMessage("CAP REQ :twitch.tv/commands");
            client.SendRawMessage("CAP REQ :twitch.tv/tags");
            client.SendRawMessage("CAP REQ :twitch.tv/tags twitch.tv/commands");
        }

        /// <summary>
        /// IRC event: authenticated was OK, and we received a welcome message from the server.
        /// </summary>
        private void Client_Registered(object sender, EventArgs e)
        {
            TmiLog.Log("Authed on Twitch IRC server.");

            if (Status == ClientStatus.Connecting)
                Status = ClientStatus.Started;

            if (OnStarted != null)
                OnStarted.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// IRC event: failed to establish a connection to the server (pre auth).
        /// </summary>
        private void Client_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            TmiLog.Error($"Connection to Twitch IRC failed: {e.Error}");
            Stop();
        }

        /// <summary>
        /// IRC event: a protocol error message was received by us, e.g. because we sent an invalid command.
        /// </summary>
        private void Client_ProtocolError(object sender, IrcProtocolErrorEventArgs e)
        {
            TmiLog.Error($"Error in IRC protocol: {e.Message}");
        }

        /// <summary>
        /// IRC event: the IRC client library has encountered an error.
        /// </summary>
        private void Client_Error(object sender, IrcErrorEventArgs e)
        {
            TmiLog.Error("Error in client: " + e.Error.Message);
        }

        /// <summary>
        /// IRC event: the server sent us an error message.
        /// </summary>
        private void Client_ErrorMessageReceived(object sender, IrcErrorMessageEventArgs e)
        {
            TmiLog.Error("Error message received: " + e.Message);
        }

        /// <summary>
        /// IRC event: MOTD was received and can be read from the client library.  
        /// </summary>
        private void Client_MotdReceived(object sender, EventArgs e)
        {
            // You are in a maze of twisty passages, all alike.
            // >
        }

        /// <summary>
        /// IRC event: connection was closed, by any means.
        /// May also indicate failed authentication or general timeout. 
        /// </summary>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            TmiLog.Error("Disconnected from Twitch IRC.");
            Stop();
        }

        /// <summary>
        /// IRC event: a raw message was received by us.
        /// This is where custom parsing needs to take place.
        /// </summary>
        private void Client_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            TmiLog.Debug("<<<", e.RawContent);

            if (e.RawContent.StartsWith('@'))
            {
                // Looks like a TMI message with state info that we can parse & process
                var tmiMsg = TmiMessageParser.Parse(e.RawContent);

                if (tmiMsg is TmiChatMessage)
                {
                    if (OnChatMessage != null)
                    {
                        OnChatMessage.Invoke(this, new TmiChatMessageEventArgs((TmiChatMessage)tmiMsg));
                    }
                }
            }
        }

        /// <summary>
        /// IRC event: a raw message was sent to the server by us.
        /// </summary>
        private void Client_RawMessageSent(object sender, IrcRawMessageEventArgs e)
        {
            TmiLog.Debug(">>>", e.RawContent);
        }
        #endregion

        #region Channel join queue
        /// <summary>
        /// Adds a channel that should be joined.
        /// </summary>
        public void AddChannel(string channelName)
        {
            if (!channelList.Contains(channelName))
            {
                channelList.Add(channelName);
            }

            channelJoinQueue.Enqueue(channelName);
        }

        /// <summary>
        /// Removes a channel that should be joined.
        /// </summary>
        public bool RemoveChannel(string channelName)
        {
            return channelList.Remove(channelName);
        }

        /// <summary>
        /// Channel join queue thread loop.
        /// </summary>
        protected void __RunChannelJoinQueue()
        {
            try
            {
                while (KeepThreadsAlive)
                {
                    try
                    {
                        if (Status == ClientStatus.Started && channelJoinQueue.Count > 0)
                        {
                            lock (channelJoinQueue)
                            {
                                string nextToJoin = channelJoinQueue.Dequeue();

                                if (nextToJoin != null)
                                {
                                    if (!channelList.Contains(nextToJoin))
                                    {
                                        TmiLog.Error($"Skipping channel join, no longer on list: {nextToJoin}");
                                    }
                                    else
                                    {
                                        TmiLog.Error($"Joining channel: #{nextToJoin}");
                                        client.Channels.Join($"#{nextToJoin}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TmiLog.Error($"Exception in channel join queue: {ex}");
                    }
                    finally
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (ThreadInterruptedException) { }

            TmiLog.Log("Channel join queue thread exited.");
        }
        #endregion
    }
}