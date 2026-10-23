# Local provider configuration

Provider credentials are read from the process environment. They must not be committed to Git, serialized into Unity assets, or added to Supabase migrations.

## PowerShell

Set replacement credentials for the current shell:

```powershell
$env:TFY_API_KEY = "<replacement-token>"
$env:OPENAI_API_KEY = "<replacement-openai-key>"
$env:KIMI_API_KEY = "<replacement-kimi-key>"
```

Persistent user-level variables can be set with `[Environment]::SetEnvironmentVariable`, then a new terminal or editor process must be opened so it inherits them.

Verify presence without printing values:

```powershell
@("TFY_API_KEY", "OPENAI_API_KEY", "KIMI_API_KEY") |
  ForEach-Object { "$_ set: " + [bool](Get-Item "Env:$_" -ErrorAction SilentlyContinue) }
```

The active NPC dialogue path currently uses `GEMINI_API_KEY` in the Supabase Edge Function. OpenAI, Kimi, and TrueFoundry variables are reserved for explicitly implemented server-side integrations; setting them alone does not send requests. Paid or production requests require a separate, approved test plan.
