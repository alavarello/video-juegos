﻿using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Net;
using System.Net.Sockets;

public class Connection
{
	private readonly Queue<Datagram> _queue;

	private readonly UdpClient _udpServer;

	private IPEndPoint _ep;

	public Connection(string ip, int port, bool isListener) {
		// save the IP and port

		// Start the new client
		_udpServer = new UdpClient(port);

		// Initialize the queue to save the messages
		_queue = new Queue<Datagram>();
		
		// start the listener
		if (isListener)
		{
			_ep = new IPEndPoint(IPAddress.Any, port);
			var thread = new Thread(Listener);
			thread.Start();
		}
		else
		{
			// Connect to the IP and port to send data
			_udpServer.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
		}
	}

	private void Listener()
	{
  		try
  		{
  			Debug.Log(">> LISTENER > Starting infinite loop");
            while (true)
	        {
		        // Listen for the data
		        var data = _udpServer.Receive(ref _ep);
		        Datagram datagram = new Datagram(data, _ep);
		        // Get the lock
		        lock (_queue)
		        {
			        // Save data into the queue
			        _queue.Enqueue(datagram);
		        }
	        }
        }
  		catch (Exception ex)
  		{
    		Debug.Log(ex);	// log errors
  		}
	}

	public Datagram GetData() {
		// Get the lock
		lock (_queue)
		{
			// Check if the queue is not empty
			return _queue.Count != 0 ? _queue.Dequeue() : null;
		}
	}

	public void SendData(byte[] data, IPEndPoint ipEndPoint) {
		// Send the data
		_udpServer.Send(data, data.Length, ipEndPoint);
	}

}
