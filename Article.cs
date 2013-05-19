using System;
using System.Collections;
using System.Diagnostics;

namespace news
{
	public class Article : NntpClient
	{

		public Article(string hostname, string username = null, string password = null)
		{
			connect(hostname,username,password);
		}

		public ArrayList getOverNews(string newsgroup, int begin = 0, int end = 0)
		{
			ArrayList retVal = new ArrayList();
			getGroupInformation(newsgroup);

			//TODO
			return retVal;

		}

		public object getGroupInformation(string newsgroup)
		{
			string message;
			string response;

			message = "GROUP " + newsgroup + "\r\n";
			write (message);
			response = getResponse();
			if(response.Substring(0,3) != "211" && response.Substring(0,3) != "281")
			{
				throw new System.ApplicationException(response);
			}
			response = getResponse();
			char[] seps =  {' '};
			string[] values = response.Split (seps);

			long start = Int32.Parse(values[2]); // reported low water mark
			long end = Int32.Parse(values[3]); // reported high water mark

			return new {startId = start, endId = end};
		}

		public ArrayList getHeadNews(string newsgroup, long startId = 0, long endId = 0)
		{
			//FIXME lijkt verdomt veel op getArticles
			string message;
			string response;
			ArrayList retval = new ArrayList();
			// http://tools.ietf.org/html/rfc3977#section-6.2.2
			dynamic groupStats = getGroupInformation(newsgroup);

			long start = groupStats.startId;
			long end = groupStats.endId;

			if(start + 100 < end && end > 100)
			{
				start = end -100; // Collect only 100 messages
			}


			for (long i = start; i < end; i++)
			{
				message = "HEAD " + i + "\r\n";
				write (message);
				response = this.getResponse();
				//Debug.WriteLine(response);
				if(response.Substring (0,3) == "430" || response.Substring(0,3) == "423") //  no article with that message-id / no article with that number
				{
					continue;
				}

				if(response.Substring(0,3) != "220")
				{
					throw new System.ApplicationException(response);
				}

				string article = "";

				while(true)
				{
					response = getResponse();
					if(response == ".\r\n")
					{
						break;
					}

					if(response == ".\n")
					{
						break;
					}

					if(article.Length < 1024)
					{
						article += response;
					}
				}

				retval.Add (article);
			}

			return retval;
		}

		public ArrayList getCompleteNews(string newsgroup)
		{
			// first groupstats so we can find out which articles to get
			dynamic groupStats = getGroupInformation(newsgroup);

			string message;
			string response;

			ArrayList retval = new ArrayList();

			long start = groupStats.startId;
			long end = groupStats.endId;

			if(start + 100 < end && end > 100)
			{
				start = end -100; // Collect only 100 messages
			}

			for (long i = start; i < end; i++)
			{
				message = "ARTICLE " + i + "\r\n";
				write (message);
				response = this.getResponse();
				//Debug.WriteLine(response);
				if(response.Substring (0,3) == "430" || response.Substring(0,3) == "423") //  no article with that message-id / no article with that number
				{
					continue;
				}

				if(response.Substring(0,3) != "220")
				{
					throw new System.ApplicationException(response);
				}

				string article = "";

				while(true)
				{
					response = getResponse();
					if(response == ".\r\n")
					{
						break;
					}

					if(response == ".\n")
					{
						break;
					}

					if(article.Length < 1024)
					{
						article += response;
					}
				}

				retval.Add (article);
			}

			return retval;
		}

		public void post(string subject, string from, string content, string newsgroup = "alt.test")
		{
			string message;
			string response;

			message = "POST\r\n";
			write (message);
			response = getResponse();
			if(response.Substring(0,3) != "340")
			{
				throw new System.ApplicationException(response);
			}

			message = "From: " + from + "\r\n"
					+ "Newsgroups: " + newsgroup + "\r\n"
					+ content + "\r\n.\r\n";

			write (message);
			response = getResponse();
			if(response.Substring(0,3) != "240")
			{
				throw new System.ApplicationException(response);
			}
		}
	}
}

