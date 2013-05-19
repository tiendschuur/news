using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;

namespace news
{

	// http://www.developerfusion.com/article/4472/how-to-nntp-in-c/
	public class NntpClient : System.Net.Sockets.TcpClient
	{
		public NntpClient ()
		{
		}

		~NntpClient()
		{
			disconnect();
		}

		/**
		 * Connects and (optionaly) logs in to the server with the provided credentials
		 */ 
		protected void connect(string server, string username = "", string password = "")
		{
			string response;

			this.Connect(server,119);
			response = this.getResponse();

			if(response.Substring(0,3) != "200")
			{
				throw new System.ApplicationException(response);
			}
			if(username.Length > 0 && password.Length > 0)
			{
				System.Console.WriteLine("Sending Authinfo");
				string message = "AUTHINFO USER " + username + "\r\n";
				write(message);
				getResponse();// should deliver #381
				message = "AUTHINFO PASS " + password + "\r\n"; 
				write(message);
			}
		}

		/**
		 * Graceful logout
		 */ 
		public void disconnect()
		{
			string message;
			string response;

			message = "QUIT\r\n";
			this.write(message);
			response = this.getResponse();
			if(response.Substring(0,3) != "205")
			{
				throw new System.ApplicationException(response);
			}
		}



		protected void write(string message)
		{
			System.Text.UTF8Encoding en = new System.Text.UTF8Encoding();

			byte[] writeBuffer = new byte[1024];
			writeBuffer = en.GetBytes(message);

			NetworkStream stream = this.GetStream();

			stream.Write(writeBuffer,0,writeBuffer.Length);

			Debug.WriteLine("WRITE: " + message);
		}

		protected string getResponse()
		{
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			byte [] serverbuff = new byte[1024];
			NetworkStream stream = this.GetStream();
			int count = 0;
			while(true)
			{
				byte [] buff = new byte[2];
				int bytes = stream.Read(buff, 0,1);
				if(bytes == 1)
				{
					serverbuff[count] = buff[0];
					count++;
					if(buff[0] == '\n')
					{
						break;
					}
				}
				else
				{
					break;
				}
			};

			string retval = enc.GetString(serverbuff, 0, count);
			Debug.WriteLine("READ:" + retval);

			return retval;
		}

		/**
		 * Requests the capabilities of the server
		 */
		public ArrayList getCapabilities()
		{
			string message, response;
			ArrayList retval = new ArrayList();

			message = "CAPABILITIES\r\n";
			write (message);
			while(true)
			{
				response = this.getResponse();
				if(response == ".\r\n" || response == ".\n")
				{
					//TODO would be nice to store these so we can fallback on the data provided
					return retval;
				}
				else
				{
					char[] seps = {' '};
					string[] values = response.Split (seps);
					retval.Add (values[0]);
					continue;
				}
			}
		}


		/**
		 * Returns a list of all newsgroups
		 */
		public ArrayList getNewsGroups()
		{
			string message;
			string response;

			ArrayList retval = new ArrayList();
			message = "LIST\r\n";
			write (message);
			response = getResponse();
			if(response.Substring (0,3) != "281")
			{
				throw new System.ApplicationException(response);
			}
			else if(response.Substring (0,3) == "480")
			{
				// authentication required
				throw new System.ApplicationException(response);
			}
			while(true)
			{
				response = getResponse();
				if(response == ".\r\n" || response == ".\n")
				{
					//FIXME store these for later usage
					return retval;
				}
				else
				{
					char[] seps = {' '};
					string[] values = response.Split (seps);
					retval.Add (values[0]);
					continue;
				}
			}
		}		
	}
}

