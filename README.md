# .NET + AI - Video Analyser

## Description

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![Twitter: elbruno](https://img.shields.io/twitter/follow/elbruno.svg?style=social)](https://twitter.com/elbruno)
![GitHub: elbruno](https://img.shields.io/github/followers/elbruno?style=social)

This repository contains a series of samples on how to analyse a video using multimodal Large Language Models, like GPT-4o or GPT-4o-mini.

***Important:** Support for CodeSpaces, coming soon!*

## Sample projects and references

The sample projects are in the folder [/src/Labs/](/src/Labs/). Currently there are labs for:

- [OpenAI .NET SDK](https://devblogs.microsoft.com/dotnet/announcing-the-stable-release-of-the-official-open-ai-library-for-dotnet/)
- [Microsoft AI Extensions](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)
- [GitHub Models](https://devblogs.microsoft.com/dotnet/using-github-models-and-dotnet-to-build-generative-ai-apps/)
- [Local Image analysis using Ollama](https://ollama.com/blog/vision-models)
- [OpenCV, using OpenCVSharp](https://github.com/shimat/opencvsharp)

## Run sample projects

To run sample projects you must 1st define the user secrets to work with the project. In example, if you are running a sample using OpenAI APIs, you must set your OpenAI Key in a user secret.

Set the user secrets, navigating to the sample project folder, and running the command:

```bash
dotnet user-secrets init
dotnet user-secrets set "OPENAI_KEY" "< your key goes here >"

```

## Author

👤 **Bruno Capuano**

- Website: [https://elbruno.com](https://elbruno.com)
- Twitter: [@elbruno](https://twitter.com/elbruno)
- Github: [@elbruno](https://github.com/elbruno)
- LinkedIn: [@elbruno](https://linkedin.com/in/elbruno)

## 🤝 Contributing

Contributions, issues and feature requests are welcome!

Feel free to check [issues page](//issues).

## Show your support

Give a ⭐️ if this project helped you!

## 📝 License

Copyright &copy; 2024 [Bruno Capuano](https://github.com/elbruno).

This project is [MIT](/LICENSE) licensed.

***
