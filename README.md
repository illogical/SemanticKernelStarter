# Local AI Chat Client with Multi-Server Support

This project is a console chat client that allows you to interact with local AI models using the Semantic Kernel framework and Ollama. It now supports multiple Ollama servers for load balancing and parallel querying.

## Features

- Connects to multiple local AI models through Ollama servers
- Provides load balancing across multiple Ollama servers
- Supports parallel querying of multiple AI models
- Offers a simple command-line interface for chatting
- Utilizes the Semantic Kernel framework for AI interactions
- Supports continuous conversation with chat history

## Prerequisites

- .NET 9.0 or later
- Ollama installed and running on one or more machines
- Compatible AI models (configurable for each server)

## Setup

1. Ensure Ollama is installed and running on your local machine or remote servers.
2. Clone this repository.
3. Open the solution in your preferred C# IDE.

## Configuration

The main configuration is now done in the `appsettings.json` file:

- Configure multiple Ollama servers with their respective endpoints, model IDs, and temperatures.
- Each server can be configured with a unique ID, endpoint URL, model ID, and temperature setting.

Example `appsettings.json`:

```json
{
  "OllamaServers": [
    {
      "ServerId": "local1",
      "Endpoint": "http://localhost:11434",
      "ModelId": "codestral",
      "Temperature": 0.7
    },
    {
      "ServerId": "local2",
      "Endpoint": "http://localhost:11435",
      "ModelId": "llama2",
      "Temperature": 0.5
    }
  ]
}

## Usage

1. Build and run the project.
2. The chat client will start and prompt you for input.
3. Type your messages and press Enter to send them to the AI model.
4. The AI's responses will be displayed in the console.
5. To exit the chat, type 'exit' and press Enter.

## How it Works

The chat client uses the Semantic Kernel framework to create a kernel with Ollama chat completion capabilities. It sets up a chat history to maintain context throughout the conversation. The client then enters a loop where it alternates between user input and AI responses, updating the chat history with each interaction.

## Disclaimer

This project is for educational and experimental purposes. The AI models and their responses should be used responsibly and with appropriate oversight.