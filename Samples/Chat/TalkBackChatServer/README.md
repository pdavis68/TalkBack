# TalkbackChatServer

A sample chat server to demonstrate the use of Talkback.

## Overview

This is a bare-bones chat server that stores conversations in a SQLite database. It uses the Talkback library to generate responses to messages.

## Installation

You'll need to create a SQLite databaase:

`dotnet ef database update`

You'll need to configure at least one LLM provider in the appsettings.json file.

The TitleLLM and TitleModel are the LLM and model used to create titles for the conversations, so set to one you have a key for (or Ollama, if that's what you're using)

Then just run the project and open the client HTML in a browser.

## Sights to see

The program.cs has the `builder.Services.RegisterTalkBack();`

Pretty much everything else happens in ChatService.cs.

`GetProvider()` demonstrates using the ProviderActivator to create providers.
`GenerateTitleAsync()` demonstrates using `ILLM.CompleteAsync()`
`ChatAsync()` demonstrates creating a context with a conversation history and streaming completions
