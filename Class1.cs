using System;

namespace Creating_A_Simple_Chat_Server
{
	class DChatServer 
	{
		// Declare a multicast (because return type is void) delegate type
		public delegate void OnMsgArrived(string message);

		// Declare a reference to an OnGetString delegate
		// Note: this field is private to prevent clients from removing or invoking delegates
		private static OnMsgArrived onMsgArrived;

		// Private to prevent instances of this type from being instantiated.
		private DChatServer() {}

		// The following method is used to provide clients a public method to add delegates to the onMsgArrived's invocation list
		public static void ClientConnect(OnMsgArrived onMsgArrived) 
		{
			// the following shorthand in C# is equivalent to: 
			//   DChatServer.onMsgArrived = (OnMsgArrived)Delegate.Combine(DChatServer.onMsgArrived, onMsgArrived);
			DChatServer.onMsgArrived += onMsgArrived;
		}

		public static void ClientDisconnect(OnMsgArrived onMsgArrived) 
		{
			//DChatServer.onMsgArrived = (OnMsgArrived)
			//Delegate.Remove(DChatServer.onMsgArrived, onMsgArrived);
			DChatServer.onMsgArrived -= onMsgArrived;
		}

		// optional SendMsg helper method, not required by the lab
		public static void SendMsg(string msg) 
		{
			// Send message to ALL clients
			SendMsg(msg, null);
		}

		public static void SendMsg(string msg, object excludeClient) 
		{
			// Send message to all clients except 'excludeClient'
			if (excludeClient == null) 
			{
				onMsgArrived(msg);
			} 
			else 
			{
				Delegate[] DelegateList = onMsgArrived.GetInvocationList();
				for (int i = 0; i < DelegateList.Length; i++) 
				{
					if (DelegateList[i].Target != excludeClient) 
					{
						((OnMsgArrived) DelegateList[i])(msg);
					}
				}            
			}        
		}    
	}


	///////////////////////////////////////////////////////////////////////////////


	class DChatClient 
	{
		private void onMsgArrived(string msg) 
		{
			Console.WriteLine("Msg arrived (Client {0}): {1}", clientName, msg);
		}

		private string clientName;

		public DChatClient(string clientName) 
		{
			this.clientName = clientName;
			// wire up client
			//DChatServer.onMsgArrived += new DChatServer.OnMsgArrived(onMsgArrived);
			DChatServer.ClientConnect(new DChatServer.OnMsgArrived(onMsgArrived));
		}

	}


	///////////////////////////////////////////////////////////////////////////////


	class EChatServer 
	{
		// Declare a multicast (because return type is void) delegate type
		public delegate void OnMsgArrived(string message);

		// Declaring an event causes the compiler to generate a PRIVATE field 
		// (onMsgArrived) that references the tail of an OnMsgArrived delegate 
		// linked-list. The compiler also generates two PUBLIC methods, 
		// add_onMsgArrived and remove_onMsgArrived which are called when the 
		// += and -= operators are applied to the event's delegate.
		public static event OnMsgArrived onMsgArrived;

		// Private to prevent instances of this type from being instantiated.
		private EChatServer() {}

		// optional SendMsg helper method, not required by the lab
		public static void SendMsg(string msg) 
		{
			// Send message to ALL clients
			SendMsg(msg, null);
		}

		public static void SendMsg(string msg, object excludeClient) 
		{
			// Send message to all clients except 'excludeClient'
			if (excludeClient == null) 
			{
				onMsgArrived(msg);
			} 
			else 
			{
				Delegate[] DelegateList = onMsgArrived.GetInvocationList();
				for (int i = 0; i < DelegateList.Length; i++) 
				{
					if (DelegateList[i].Target != excludeClient) 
					{
						((OnMsgArrived) DelegateList[i])(msg);
					}
				}            
			}        
		}    
	}


	///////////////////////////////////////////////////////////////////////////////


	class EChatClient 
	{
		private void onMsgArrived(string msg) 
		{
			Console.WriteLine("Msg arrived (Client {0}): {1}", clientName, msg);
		}

		private string clientName;

		public EChatClient(string clientName) 
		{
			this.clientName = clientName;
			// wire up client
			EChatServer.onMsgArrived += new EChatServer.OnMsgArrived(onMsgArrived);
		}

	}


	///////////////////////////////////////////////////////////////////////////////

	// Handling the MsgArrived Event according to the .NET Framework guidelines
	//  Note: we make the server an instance to illustrate the sender argument of the event's data object

	// Note:   The following class, MsgArrivedEventArgs, is not needed if an event data class already exists for the event your class wants to raise, or if data is not generated by your event. 

	// MsgArrived Event needs to provide the message text to the event handlers
	// MsgEventArgs - event argument class that stores the message string
	// Naming convention for an event argument: <EventName>EventArgs
	public class MsgArrivedEventArgs : EventArgs 
	{
		private readonly string message;
		//constructor
		public MsgArrivedEventArgs(string message) 
		{
			this.message = message;
		}
		//properties
		public string Message 
		{
			get { return message; }
		}     
	}

	class GChatServer 
	{
		// Declare a multicast (because return type is void) delegate type
		// Naming convention for an event delegate: <EventName>EventHandler
		public delegate void MsgArrivedEventHandler(object sender, MsgArrivedEventArgs me);
   
		// Declaring an event causes the compiler to generate a PRIVATE field 
		// (MsgArrivedHandler) that references the tail of an MsgArrivedEventHandler delegate 
		// linked-list. The compiler also generates two PUBLIC methods, 
		// add_MsgArrived and remove_MsgArrived which are called when the 
		// += and -= operators are applied to the event's delegate.
		public event MsgArrivedEventHandler MsgArrivedHandler;


		// optional SendMsg helper method, not required by the lab
		public void SendMsg(string msg) 
		{
			// raise MsgArrived event
			SendMsg(msg, null);
		}

		public void SendMsg(string msg, object excludeClient) 
		{
			MsgArrivedEventArgs me = new MsgArrivedEventArgs(msg);
			OnMsgArrived(me, excludeClient);
		}

		// The following pattern should be used for the method that raises the event:
		// The OnMsgArrived method raises the event by invoking the delegates. 
		// The sender is always "this", the current instance of the class.
		// Note: For classes designed to be subclassable, the following should be a protected virtual method. 
		//  Making it protected virtual provides a way for a subclass to handle the event using an override. 
 
		// Naming convention: On<EventName>
		protected virtual void OnMsgArrived(MsgArrivedEventArgs me, object excludeClient) 
		{
			if (MsgArrivedHandler != null)
			{
				// Send message to all clients except 'excludeClient'
				if (excludeClient == null) 
				{
					MsgArrivedHandler(this, me); // sender is NULL since this is a static method and there is no object
				} 
				else 
				{
					Delegate[] DelegateList = MsgArrivedHandler.GetInvocationList();
					for (int i = 0; i < DelegateList.Length; i++) 
					{
						if (DelegateList[i].Target != excludeClient) 
						{
							((MsgArrivedEventHandler) DelegateList[i])(this, me);
						}
					}            
				}        
			}
		}
	}


	///////////////////////////////////////////////////////////////////////////////


	class GChatClient 
	{
		private void onMsgArrived(object sender, MsgArrivedEventArgs me) 
		{
			Console.WriteLine("Msg arrived (Client {0}): {1} Server: {2}", clientName, me.Message, sender.ToString());
		}

		private string clientName;
		private GChatServer gcs;

		public GChatClient(string clientName, GChatServer gcs) 
		{
			this.clientName = clientName;
			this.gcs = gcs;
			gcs.MsgArrivedHandler += new GChatServer.MsgArrivedEventHandler(this.onMsgArrived);
		}

	}


	///////////////////////////////////////////////////////////////////////////////


	class Application 
	{
		private static void DelegateChatServerDemo() 
		{
			Console.WriteLine("Demo start: Delegate Chat Server.");

			DChatClient cc1 = new DChatClient("1");
			DChatClient cc2 = new DChatClient("2");
			DChatClient cc3 = new DChatClient("3");

			//DChatServer.SendMsg("Hi to all clients");
 			DChatServer.SendMsg("Hi to all clients", null); 
			DChatServer.SendMsg("Hi to all clients except client 2", cc2);

			Console.WriteLine("Demo stop: Delegate Chat Server.");
		}

		private static void EventChatServerDemo() 
		{
			Console.WriteLine("\n\nDemo start: Event Chat Server.");
			EChatClient cc1 = new EChatClient("1");
			EChatClient cc2 = new EChatClient("2");
			EChatClient cc3 = new EChatClient("3");

			//EChatServer.SendMsg("Hi to all clients");
			EChatServer.SendMsg("Hi to all clients", null);
			EChatServer.SendMsg("Hi to all clients except client 2", cc2);

			Console.WriteLine("Demo stop: Event Chat Server.");
		}
		private static void GuidelinesBasedEventChatServerDemo() 
		{
			GChatServer s1 = new GChatServer();
			Console.WriteLine("\n\nDemo start: Guidelines Based Event Chat Server. Server: {0}", s1.ToString());
			GChatClient cc1 = new GChatClient("1",s1);
			GChatClient cc2 = new GChatClient("2",s1);
			GChatClient cc3 = new GChatClient("3",s1);

			//s1.SendMsg("Hi to all clients");
			s1.SendMsg("Hi to all clients", null);
			s1.SendMsg("Hi to all clients except client 2", cc2);

			Console.WriteLine("Demo stop: Guidelines Based Event Chat Server.");
		}

		public static void Main() 
		{
			DelegateChatServerDemo();
			EventChatServerDemo();
			GuidelinesBasedEventChatServerDemo();
		}
	}
}
