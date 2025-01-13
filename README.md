# Local AI Chat Client

This project is a simple console chat client that allows you to interact with local AI models using the Semantic Kernel framework and Ollama.

## Features

- Connects to local AI models through Ollama
- Provides a simple command-line interface for chatting
- Utilizes the Semantic Kernel framework for AI interactions
- Supports continuous conversation with chat history

## Prerequisites

- .NET 6.0 or later
- Ollama installed and running locally
- A compatible AI model (default: "codestral")

## Setup

1. Ensure Ollama is installed and running on your local machine.
2. Clone this repository.
3. Open the solution in your preferred C# IDE.

## Configuration

The main configuration is done in the `Program.cs` file:

- `modelId`: The ID of the AI model you want to use (default: "codestral")
- `endpoint`: The Ollama endpoint URL (default: "http://localhost:11434")

You can modify these values to use different models or endpoints.

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