# Forest Events #

The forest event system enables instant topic-besed communication. It allows 
you to benefit from the *event bus* pattern with minimal development effort.


## Publishig Events ##

### 1. One-way Publishing ###

Each `IView` implementation exposes a `Publish` method that can be used to 
pass data to other views.

>     bool Publish<TMessage>(TMessage message, params string[] topics);

The publishing mechanism will determine the potential subscribers by examining 
the .NET type of the message object (`TMesssage`) and the provided topic names.

Here is an example of passing the name of a person to be greeted via Forest's 
built-in event mechanism:

    public void Introduce()
    {
        string myName = "George";
        string topicName = "GreetingsTopic";

        Publish(myName, topicName);
    }

The above code will send the `string` "George" to all receivers who expect a 
message of type `string` and have subscribed to the "GreetingsTopic".

### 2. Request-Reply Behavior ###

> NB: Future development

The `Publish` method has another overload which allows for handling replies 
from the subscriber:

>     void Publish<TMessage,TReply>(
>         TMessage message, 
>         Action<object> callback
>         params string[] topics);

In this case alongside the message, a reference to a callback method will also 
reach the subscribers. The method is represented by a generic `Action<object>`
so it allows the subscribers to return any object as a reply.

An example using the above publishing method can be seen below:  

    public void Introduce()
    {
        string myName = "George";
        string topicName = "GreetingsTopic";

        Publish(myName, GetReply, topicName);
    } 

    public void GetReply(object reply) { ... }

## Subscribing for Events ##

In order to receive a message trough the event system, a view must first 
define a method that will handle the message being sent and place the 
`[Subscription]` attribute on top of the method.

### 1. Handling One-way Messages ##

Here is an example of handling a one-way message.

    [Subscription(Topic = "GreetingsTopic")]
    internal void GreetingReceived(string name)
    {
        ViewModel.ReplyText = string.Format(
            "Hello, {0}. Nice to meet you!", name);
    }

In order for a method to qualify as a valid subscriber, it must accept
as a first parameter the same object type as is the message. In the previous 
example we have passed the `string` "George", so the type of the message is 
automatically inferred as `System.String`.  

Notice that the `[Subscription]` attribute received a parameter - `Topic`, 
which has the same value as the `topicName` we used to sent the message 
in the above example. This is necessary in order for the `EventBus` to know 
how to efficiently route the message only to certain subscribers. Message 
routing is being discussed in more details in the **Message Routing** section
of the document. 

### 2. Handling Request-Reply Messages ###

> NB: Future development

Here is an example of handling a message in Request-Reply manner.

    [Subscription(Topic = "GreetingsTopic")]
    internal void GreetingReceived(string name, Action<object> callback)
    {
        string reply = string.Format("Hello, {0}. Nice to meet you!", name);
        callback(reply);
    }

The difference with the one-way message handling here is that our subscriber 
now receives a second argument - an `Action<object>` instance. This is the 
same callback instance we have passed when publishing the message.

### **Important Notes** ###

- When using request-reply messages, one could send the message like so:

        Publish(myName, null, topicName);

  This will mean that the callback object sent is `null`. However, the 
  subscriber does not have to worry about this, as the `EventBus` will 
  supply a dead-letter callback as default. Therefore, in our subscruber 
  logic we do not need to explicitly check the `callback` parameter for
  being `null`.

- When using request-reply messages, one could send the message like so:

        Publish(myName, topicName);

    Yet, if the only declared subscriber is a method that receives a callback 
  as a second argument. Then the behavior is like in the above example, the 
  `EventBus` will automatically suply a no-op callback. 

- When we specify a callback method in our publish call, but there is no
  matching subscriber that receives a reply callback, the messsage will 
  **not** be sent at all. As a general rule of a thumb, prefer to use a 
  signature for the subscriber methods that has a callback. 


 ## Message Routing ##

 TODO