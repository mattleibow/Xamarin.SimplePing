# Xamarin.SimplePing

[![Build Status](https://www.bitrise.io/app/26b2b1828e248e3a/status.svg?token=LBY4eEQN88c2FrzF5RhfAg&branch=master)](https://www.bitrise.io/app/26b2b1828e248e3a)

`SimplePing` demonstrates ping (ICMP) send and receive.

A Xamarin.iOS and Xamarin.Mac binding for the [Apple sample](https://developer.apple.com/library/content/samplecode/SimplePing):

On most platforms ping requires privileges (it's implemented with a raw IP socket). 
Apple platforms include a special facility that allows you to ping without privileges.
Specifically, you can open a special, non-privileged ICMP socket that allows you to 
send and receive pings.

To use the `SimplePing` class:

1. create an instance of the `SimplePing` class and keep a reference to that instance.
2. set the `Delegate` property or the `Started` event.
3. call `Start()`.
4. if things go well, your delegate's `DidStartWithAddress()` method, or the `Started`
   event, will be called; to send a ping, call `SendPing()`.
5. when `SimplePing` receives an ICMP packet, it will call the 
   `DidReceivePingResponsePacket()` method, or the `ResponseRecieved` event.

`SimplePing` can be used from any thread but the use of any single instance must be 
confined to a specific thread. Moreover, that thread must run its run loop. In most 
cases it's best to use `SimplePing` from the main thread.

```csharp
var pinger = new SimplePing("www.apple.com");

pinger.Started += (sender, e) => {
    var endpoint = e.EndPoint;
    pinger.SendPing(null);
};

pinger.ResponseRecieved += (sender, e) => {
    var seq = e.SequenceNumber;
    var packet = e.Packet;
};

pinger.Start();
```
