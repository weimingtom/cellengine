package com.net.flash.server.test;

import java.io.File;
import java.io.IOException;

import com.cell.CIO;
import com.cell.CObject;
import com.cell.io.CFile;
import com.cell.j2se.CAppBridge;
import com.cell.j2se.CStorage;
import com.net.ExternalizableMessage;
import com.net.MessageHeader;
import com.net.NetDataInput;
import com.net.NetDataOutput;
import com.net.Protocol;
import com.net.flash.message.FlashMessageFactory;
import com.net.flash.server.test.Messages.*;
import com.net.minaimpl.server.ServerImpl;
import com.net.server.ClientSession;
import com.net.server.ClientSessionListener;
import com.net.server.ServerListener;

public class FlashTestEchoServer extends ServerImpl implements ServerListener
{
	public FlashTestEchoServer(FlashMessageFactory factory) {
		super(CIO.getAppBridge().getClassLoader(), factory, 10, 600, 600, 0);
	}
	
	@Override
	public ClientSessionListener connected(ClientSession session) {
		return new EchoClientSession();
	}
	
	public void open(int port) throws IOException {
		super.open(port, this);
	}
	
	class EchoClientSession implements ClientSessionListener
	{
		@Override
		public void disconnected(ClientSession session) {}
		@Override
		public void sentMessage(ClientSession session, Protocol protocol, MessageHeader message) {}
		@Override
		public void receivedMessage(ClientSession session, Protocol protocol, MessageHeader message) {
			if (message instanceof EchoRequest) {
				session.sendResponse(protocol, new EchoResponse(message.toString()));
			}
		}
	}
	
	public static void main(String[] args) throws IOException
	{
		try {
			CAppBridge.init();
			FlashMessageFactory factory = new FlashMessageFactory(new MutualMessageCodecJava(), Messages.class);
			FlashTestEchoServer server = new FlashTestEchoServer(factory);
			int port = 19820;
			if (args.length > 0) {
				try {
					port = Integer.parseInt(args[0]);
				} catch (Exception err) {
					System.err.println("use default port " + port);
				}
			}
			server.open(port);
			
		} catch (Exception err) {
			err.printStackTrace();
			System.exit(1);
		}
	}
}