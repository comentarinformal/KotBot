﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
namespace KotBot
{
    public static class IRCBot
    {
        private static Collection<IrcClient> allClients = new Collection<IrcClient>();
        [Scripting.RegisterLuaFunction("IRC.Connect")]
        public static IrcClient Connect(string IP)
        {
            IrcDotNet.IrcUserRegistrationInfo info = new IrcUserRegistrationInfo();
            info.NickName = "KotBot";
            info.RealName = "KotBot";
            info.UserName = "KotBot";
            var client = new IrcClient();
            client.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            client.Connected += client_Connected;
            client.Disconnected += client_Disconnected;
            client.Registered += client_Registered;
            
            client.Connect(System.Net.IPAddress.Parse(IP), false, info);

            // Add new client to collection.
            allClients.Add(client);
            return client;
        }
        public static void JoinChannel(IrcClient client, string channel)
        {
            client.Channels.Join(channel);
        }
        private static void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.MessageReceived += Channel_MessageReceived;
        }

        static void Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            IrcChannel channel = (IrcChannel)sender;
            IrcLocalUser user = channel.Client.LocalUser;
            IrcUser messageSender = (IrcUser)e.Source;
            Scripting.LuaHook.Call("MessageRecieved", new Message(new IRC.IRCClient(user, channel), new IRC.IRCUser(user, messageSender), e.Text));
        }
        static void client_Registered(object sender, EventArgs e)
        {
            ((IrcClient)sender).LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            Scripting.LuaHook.Call("IRC.Registered", (IrcClient)sender);
        }

        static void client_Disconnected(object sender, EventArgs e)
        {
            
        }

        static void client_Connected(object sender, EventArgs e)
        {
            Scripting.LuaHook.Call("IRC.Connected", (IrcClient)sender);
        }
    }
}