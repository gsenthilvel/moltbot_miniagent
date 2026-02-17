# MiniAgent

Lightweight C# AI agent inspired by OpenClaw, with skills and tool calling. Builds and runs locally.

Continuous learn+share effort at https://ganesansenthilvel.blogspot.com/2026/02/moltbot.html

## Requirements

- .NET 8 SDK
- For local LLM: [Ollama](https://ollama.com) (e.g. `ollama run qwen2.5:7b`)
- For cloud: OpenAI API key (set `OPENAI_API_KEY` or in `miniagent.json`)

## Build and run

```bash
# From repo root
dotnet build
dotnet run --project src/MiniAgent

# Use OpenAI instead of Ollama
dotnet run --project src/MiniAgent -- --provider openai
```

## Configuration

- **miniagent.json** in the current directory or `~/.miniagent/miniagent.json`
- Env: `MINIAGENT_PROVIDER`, `OLLAMA_HOST`, `OLLAMA_MODEL`, `OPENAI_API_KEY`, `OPENAI_API_BASE`, `OPENAI_MODEL`
- CLI: `--provider ollama|openai`, `--ollama-model`, `--openai-model`

## Skills

Skills are loaded from (highest precedence first):

1. `./skills/` (workspace)
2. `~/.miniagent/skills/`
3. Extra dirs in config `skills.extraDirs`

Each skill is a folder containing **SKILL.md** with YAML frontmatter (`name`, `description`) and instruction body. Example: `skills/example-greeting/SKILL.md`, `skills/file-summarizer/SKILL.md`.

## Tools

Built-in tools: **file_read**, **file_write**, **shell_exec**. The agent can read/write files and run shell commands when the model requests them.

## Commands

- `/exit` — quit
- `/clear` — clear session history
