using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Uno_Server.Game;
using Uno_Server.Networking;

namespace Uno.Tests;

public static class TestUtils
{
    public static RequestHandler CreateHandler(out NetworkStream serverStream, out TcpClient serverClient)
    {
        TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        TcpClient client = new TcpClient();
        client.Connect(IPAddress.Loopback, port);
        serverClient = listener.AcceptTcpClient();
        listener.Stop();

        RequestHandler handler = new RequestHandler(client);
        var field = typeof(RequestHandler).GetField("clientStream", BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(handler, client.GetStream());
        serverStream = serverClient.GetStream();
        serverStream.ReadTimeout = 1000;
        return handler;
    }

    public static void SetPrivateField<T>(object obj, string name, T value)
    {
        var field = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        field!.SetValue(obj, value);
    }

    public static T GetPrivateField<T>(object obj, string name)
    {
        var field = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field!.GetValue(obj)!;
    }
}

public class ActionUseCardTests
{
    [Fact]
    public void WildWithoutColor_ReturnsErrorAndKeepsState()
    {
        var handler1 = TestUtils.CreateHandler(out var stream1, out var server1);
        var handler2 = TestUtils.CreateHandler(out var stream2, out var server2);
        var p1 = new Player(0, handler1);
        var p2 = new Player(1, handler2);
        var game = new Game(0, 2);
        game.Players = new List<Player> { p1, p2 };

        var draw = new Deck();
        draw.Add(new Card(Color.Red, Value.One));
        TestUtils.SetPrivateField(game, "drawPile", draw);

        var discard = new Deck();
        discard.Add(new Card(Color.Red, Value.Two));
        TestUtils.SetPrivateField(game, "discardPile", discard);
        TestUtils.SetPrivateField(game, "currentPlayerIndex", 0);

        p1.Hand.Add(new Card(Color.Wild, Value.Wild));

        game.ActionUseCard(0, new Card(Color.Wild, Value.Wild), null);

        Assert.Single(p1.Hand.Cards);
        Assert.Equal(Value.Wild, p1.Hand.Cards[0].Value);
        Assert.Single(discard.Cards);
        Assert.Equal(Value.Two, discard.Cards[0].Value);
        Assert.Equal(0, TestUtils.GetPrivateField<int>(game, "currentPlayerIndex"));

        var buffer = new byte[64];
        int len = stream1.Read(buffer, 0, buffer.Length);
        string msg = Encoding.ASCII.GetString(buffer, 0, len);
        Assert.StartsWith("UNOER", msg);

        server1.Close();
        server2.Close();
    }

    [Fact]
    public void DrawFourWithoutColor_ReturnsErrorAndKeepsState()
    {
        var handler1 = TestUtils.CreateHandler(out var stream1, out var server1);
        var handler2 = TestUtils.CreateHandler(out var stream2, out var server2);
        var p1 = new Player(0, handler1);
        var p2 = new Player(1, handler2);
        var game = new Game(0, 2);
        game.Players = new List<Player> { p1, p2 };

        var draw = new Deck();
        for (int i = 0; i < 5; i++) draw.Add(new Card(Color.Red, Value.One));
        TestUtils.SetPrivateField(game, "drawPile", draw);

        var discard = new Deck();
        discard.Add(new Card(Color.Red, Value.Two));
        TestUtils.SetPrivateField(game, "discardPile", discard);
        TestUtils.SetPrivateField(game, "currentPlayerIndex", 0);

        p1.Hand.Add(new Card(Color.Wild, Value.DrawFour));

        game.ActionUseCard(0, new Card(Color.Wild, Value.DrawFour), null);

        Assert.Single(p1.Hand.Cards);
        Assert.Equal(Value.DrawFour, p1.Hand.Cards[0].Value);
        Assert.Empty(p2.Hand.Cards);
        Assert.Single(discard.Cards);
        Assert.Equal(Value.Two, discard.Cards[0].Value);
        Assert.Equal(0, TestUtils.GetPrivateField<int>(game, "currentPlayerIndex"));

        var buffer = new byte[64];
        int len = stream1.Read(buffer, 0, buffer.Length);
        string msg = Encoding.ASCII.GetString(buffer, 0, len);
        Assert.StartsWith("UNOER", msg);

        server1.Close();
        server2.Close();
    }
}
