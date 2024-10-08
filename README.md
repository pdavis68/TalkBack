# TalkBack

TalkBack is a library that abstracts out chat with LLMs.

## Overview

Each LLM has its own API, its own model options and so forth. If you want an application that uses multiple models, you then have to 
implement code for interacting with each API. 

TalkBack is designed to be lightweight and easy-to-use. It provides access to the custom options of each LLM while providing an 
otherwise single interface for interacting with the APIs, whether executing blocking or streaming completions.

TalkBack utilizes a "IConversationContext" that maintains the history of a conversation for maintaining context between calls.

## Supporter LLMs

- Ollama
- OpenAI
- Claude
- Groq

## Installation

```
dotnet add package TalkBack-LLM
```

## Startup

	Call the RegisterTalkBack() extension method on IServiceCollection, to add all the services to the DI container.

```csharp
using TalkBack;

...
	services.RegisterTalkBack()
```


## Example

The non-streaming version of the code is very simple:

```csharp
_conversationContext.SystemMessage = "You are an expert C# programmer!";
var result = await _llm.CompleteAsync("Please write a command-line C# program to retrieve the current weather for Paris, France, from OpenWeather.", _conversationContext);
string responseText = result.Response;
```

The streaming version isn't much more complicated:

```csharp
public class MyClass : ICompletionReceiver
{

...
    public SomeMethod()
    {
        await _llm.StreamCompletionAsync(this, prompt, _conversationContext);
    }
...

    public async Task ReceiveCompletionPartAsync(IModelResponse response, bool final)
    {
        if (!final) // final contains a copy of the entire response that was streamed.
        {
            Console.Write(response.Response);
        }
        return;
    }
```


This is a command-line chat app. When you run it, it will give you a prompt: "> ", and you type your prompt. It will then respond. The conversation continues until you enter "q" on a blank line and hit enter.
Simply change the provider setup to change LLMs.

```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using TalkBack;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Claude;
using TalkBack.LLMProviders.Ollama;
using TalkBack.LLMProviders.OpenAI;

bool streaming = true; // <-- Change to false for blocking version.

// Add services for DI and add TalkBack
var services = new ServiceCollection();
services.RegisterTalkBack();

var serviceProvider = services.BuildServiceProvider();

var providerActivator = serviceProvider.GetService<IProviderActivator>();
var llm = serviceProvider.GetService<ILLM>();

/*
    Examples of the 3 current providers:

var provider = providerActivator!.CreateProvider<OpenAIProvider>();
provider!.InitProvider(new OpenAIOptions()
{
    ApiKey = "<your key here>",
    Model = "gpt-3.5-turbo"
});

var provider = providerActivator!.CreateProvider<OllamaProvider>();
provider!.InitProvider(new OllamaOptions()
{
    ServerUrl = "http://localhost:11434/api",
    Model = "llama2"
});

var provider = providerActivator!.CreateProvider<ClaudeProvider>();
provider!.InitProvider(new ClaudeOptions()
{
    ApiKey = "<your key here>",
    Model = "claude-2.1"
});
*/

var provider = providerActivator!.CreateProvider<OllamaProvider>();
provider!.InitProvider(new OllamaOptions()
{
    ServerUrl = "http://localhost:11434/api",
    Model = "llama2"
});

llm!.SetProvider(provider);

var streamReceiver = new StreamReceiver();

var conversationContext = llm.CreateNewContext();

string prompt = string.Empty;
Console.WriteLine("'q' + enter to quit");
while (prompt.ToLower() != "q")
{
    Console.Write("> ");
    prompt = Console.ReadLine() ?? string.Empty;
    if (streaming)
    {
        streamReceiver.Done = false;
        Console.Write(Environment.NewLine + "Response: ");
        await llm.StreamCompletionAsync(streamReceiver, prompt, conversationContext);

        // Wait for it to finish streaming.
        while(!streamReceiver.Done)
        {
            Thread.Sleep(1000);
        }
        Console.WriteLine(Environment.NewLine);
    }
    else
    {
        var result = await llm.CompleteAsync(prompt, conversationContext);
        Console.WriteLine(Environment.NewLine + "Response: " + result.Response + Environment.NewLine);
    }

}


public class StreamReceiver : ICompletionReceiver
{
    public bool Done { get; set; } = true;
    public Task ReceiveCompletionPartAsync(IModelResponse response, bool final)
    {
        Done = final;
        if (!final)
        {
            Console.Write(response.Response);
        }
        return Task.CompletedTask;
    }
}
```

## More

See the Wiki for some more realistic examples with normal dependency injection.

And there's a sample bare-bones chat server and client in the Samples directory.
