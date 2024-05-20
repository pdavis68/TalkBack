# TalkBack

TalkBack is a library that abstracts out completions using LLMs.

## Overview

Each LLM has its own API, its own model options and so forth. If you want an application that uses multiple models, you then have to 
implement code for interacting with each API. 

TalkBack is designed to be lightweight and easy-to-use. It provides access to the custom options of each LLM while providing an 
otherwise single interface for interacting with the APIs, whether executing blocking or streaming completions.

TalkBack utilizes a "IConversationContext" that maintains the history of a conversation for maintaining context between calls.

## Startup

	Call the RegisterTalkBack() extension method on IServiceCollection, to 
add all the services to the DI container.

```
using TalkBack;

...
	services.RegisterTalkBack()
```


## Usage

```
using Microsoft.Extensions.DependencyInjection;
using TalkBack;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Claude;
using TalkBack.LLMProviders.Ollama;
using TalkBack.LLMProviders.OpenAI;

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

var provider = providerActivator!.CreateProvider<OpenAIProvider>();
provider!.InitProvider(new OpenAIOptions()
{
    ApiKey = "<your key here>",
    Model = "gpt-3.5-turbo"
});



llm!.SetProvider(provider);

string prompt = string.Empty;
Console.WriteLine("'q' + enter to quit");


var conversationContext = llm.CreateNewContext();
while (prompt.ToLower() != "q")
{
    Console.Write("> ");
    prompt = Console.ReadLine() ?? string.Empty;
    var result = llm.CompleteAsync(prompt, conversationContext).Result;
    Console.WriteLine(Environment.NewLine + "Response: " + result.Response + Environment.NewLine);

}
```


## To Do

- Provide access to the conversation history.