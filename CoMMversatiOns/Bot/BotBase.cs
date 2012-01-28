/**
 *  CoMMversatiOns Project: API Tool for increase client functionalities of
 *  realXtend technologies.
 *  Copyright (C) 2010 Information Engineering Research Unit
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *  
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using System.Xml.Linq;
using CoMMversatiOns.Events;
using OpenMetaverse.Packets;

namespace CoMMversatiOns.Bot
{

    /// <summary>
    /// Base class for creating a MMOL bot.
    /// </summary>
    public class BotBase
    {
#region User data
        /// <summary>
        /// First name of the agent
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the agent
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Returns the full name of the agent
        /// </summary>
        public string FullName { get { return string.Format("{1} {2}", FirstName, LastName); } }

        /// <summary>
        /// Password of the agent
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// User position in the virtual world
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// User angle in the virtual world
        /// </summary>
        public float Angle { get; set; }

#endregion

#region Server data
        /// <summary>
        /// Virtual world server URL
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Virtual world region to connect
        /// </summary>
        public string Region { get; set; }
#endregion

#region OpenMetaverse objects
        /// <summary>
        /// OpenMetaverse client
        /// </summary>
        public GridClient Client { get; set; }
#endregion

        /// <summary>
        /// Dictionary that contains Handlers to respond to chat events
        /// </summary>
        private Dictionary<string, EventHandler<ChatActionEventArgs>> Handlers { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BotBase()
        {
            Handlers = new Dictionary<string, EventHandler<ChatActionEventArgs>>();
        }

        /// <summary>
        /// Loads the bot configuration from an XML
        /// </summary>
        /// <param name="xmlPath">Path to the configuration XML file</param>
        public void LoadConfig(string xmlPath)
        {
            XDocument configDoc = XDocument.Load(xmlPath);

            XElement rootElem = configDoc.Root;

            ServerUrl = rootElem.Element(XName.Get("serverUrl")).Value;
            Region = rootElem.Element(XName.Get("region")).Value;

            FirstName = rootElem.Element(XName.Get("firstName")).Value;
            LastName = rootElem.Element(XName.Get("lastName")).Value;
            Password = rootElem.Element(XName.Get("password")).Value;

            XElement position = rootElem.Element(XName.Get("initialPosition"));
            try
            {
                float x, y, z;
                float.TryParse(position.Attribute(XName.Get("x")).Value, out x);
                float.TryParse(position.Attribute(XName.Get("y")).Value, out y);
                float.TryParse(position.Attribute(XName.Get("z")).Value, out z);

                Position = new Vector3(x, y, z);
            }
            catch (Exception e)
            {
            }

            XElement angle = rootElem.Element(XName.Get("initialAngle"));
            try
            {
                float yaw;
                float.TryParse(angle.Attribute(XName.Get("yaw")).Value, out yaw);

                Angle = yaw;
            }
            catch (Exception e)
            {
            }

        }

        /// <summary>
        /// Initializes the bot.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Connects the bot to the virtual world.
        /// </summary>
        public void Connect()
        {
            Client = new GridClient();
            Client.Appearance.AppearanceSet += new EventHandler<AppearanceSetEventArgs>(Appearance_AppearanceSet);

            string startLocation = NetworkManager.StartLocation(Region, (int)Position.X, (int)Position.Y, (int)Position.Z);
            Client.Network.SimConnected += new EventHandler<SimConnectedEventArgs>(Network_OnConnected);
            Client.Settings.LOGIN_SERVER = ServerUrl;
            string[] pointAtt = new string[8];
            
            if (Client.Network.Login(FirstName, LastName, Password, "", startLocation, ""))
            {
                Client.Network.SimConnected += new EventHandler<SimConnectedEventArgs>(Network_OnConnected);
                Console.WriteLine("Bot Login Message: " + Client.Network.LoginMessage);
                //Client.Appearance.SetPreviousAppearance(true);
                
                //Client.Appearance.RequestSetAppearance(true);//.SetPreviousAppearance(false);
                
                Client.Self.ChatFromSimulator += new EventHandler<ChatEventArgs>(Self_ChatFromSimulator);
            }
        }

        void Network_EventQueueRunning(object sender, EventQueueRunningEventArgs e)
        {
            Console.WriteLine("Network_EventQueueRunning");
            if (e.Simulator == Client.Network.CurrentSim)
            {
                Client.Appearance.RequestSetAppearance(true);
            }
        }

        void Network_SimChanged(object sender, SimChangedEventArgs e)
        {
            Console.WriteLine("Network_SimChanged");
            if (e.PreviousSimulator != null)
            {
                Client.Appearance.RequestSetAppearance(false);
               // AppearanceManager.
                //LoadOutfit("Clothing/Ayudante English");
            }
        }

        void Appearance_AppearanceSet(object sender, AppearanceSetEventArgs e)
        {
            Console.WriteLine("AppearanceSet!! Success: " +e.Success);

            if (!e.Success)
            {
                LoadOutfit("Clothing/" + FirstName + " " + LastName);
            }
        }

        private void LoadOutfit(string target)
        {
            UUID folder = Client.Inventory.FindObjectByPath(Client.Inventory.Store.RootFolder.UUID, Client.Self.AgentID, target, 20 * 1000);

            if (folder == UUID.Zero)
            {
                Console.WriteLine("ERROR: Outfit path " + target + " not found");
                return;
            }

            List<InventoryBase> contents = Client.Inventory.FolderContents(folder, Client.Self.AgentID, true, true, InventorySortOrder.ByName, 20 * 1000);
            List<InventoryItem> items = new List<InventoryItem>();

            if (contents == null)
            {
                Console.WriteLine("ERROR: Failed to get contents of " + target);
                return;
            }

            foreach (InventoryBase item in contents)
            {
                if (item is InventoryItem)
                    items.Add((InventoryItem)item);
            }

            Client.Appearance.ReplaceOutfit(items);

            Console.WriteLine("Starting to change outfit to " + target);
        }

        /// <summary>
        /// Registers a chat action and its handler
        /// </summary>
        /// <param name="action">Action to register (a verb like "translate")</param>
        /// <param name="handler">Handler of this action</param>
        public void RegisterChatAction(string action, EventHandler<ChatActionEventArgs> handler)
        {
            Handlers.Add(action, handler);
        }

        /// <summary>
        /// Unregister the handler of an action
        /// </summary>
        /// <param name="action">Action whose handler is going to be removed</param>
        public void UnregisterChatAction(string action)
        {
            Handlers.Remove(action);
        }

        /// <summary>
        /// Handles a chat message received from the server
        /// </summary>
        /// <param name="sender">Client that received the chat</param>
        /// <param name="e">Parameters of the chat message</param>
        private void Self_ChatFromSimulator(object sender, ChatEventArgs e)
        {
            if (e.SourceType == ChatSourceType.Agent && e.Type == ChatType.Normal && e.SourceID != Client.Self.AgentID)
            {
                //Iterate through each action and call its handler in case the received message tries to invoke that action
                //The Message that is sent with the event is the original message without the verb
                foreach (string action in Handlers.Keys)
                {
                    if (e.Message.ToLower().StartsWith(action))
                    {
                        ChatActionEventArgs args = new ChatActionEventArgs();
                        args.Message = e.Message.Replace(action + " ", "");
                        args.FromName = e.FromName;
                        Handlers[action].Invoke(this, args);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handler when the bot is connected to the server
        /// </summary>
        /// <param name="sender">Client that was connected</param>
        /// <param name="e">Parameters of the result of the connection</param>
        private void Network_OnConnected(object sender, SimConnectedEventArgs e)
        {
            Console.WriteLine("The bot is connected");
            //Client.CurrentDirectory = Inventory.Store.RootFolder;
            Client.Self.Movement.AlwaysRun = false;
            Client.Network.SimChanged += new EventHandler<SimChangedEventArgs>(Network_SimChanged);
            Client.Network.EventQueueRunning += new EventHandler<EventQueueRunningEventArgs>(Network_EventQueueRunning);
            //LoadOutfit("Clothing/" + FirstName + " " + LastName);
            System.Threading.Thread.Sleep(3000);

            float rad = (float)DegreeToRadian(Angle);

            Console.WriteLine("angulo: " + Angle + " - rad: " + rad);
            Client.Self.Movement.BodyRotation = Quaternion.CreateFromEulers(0, 0, (float)DegreeToRadian(Angle));
            //client.Objects.AvatarUpdate += new EventHandler<AvatarUpdateEventArgs>(Objects_AvatarUpdate);
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
