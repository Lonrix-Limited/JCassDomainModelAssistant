# tools/ — placeholder

**Filled by sessions S5 and S6.**

Will hold `jcass-dm`, the command-line tool at the centre of this project, shipped pre-compiled and
self-contained so that nothing here ever needs a .NET SDK or a NuGet restore.

| Session | Verbs |
|---|---|
| S5 | `dump`, `set-meta`, `add-treatment`, `add-parameter`, `add-input-header` — reading and writing `domain_model_setup.xlsx` |
| S6 | `scaffold`, `rename`, `check`, `package` |

The tool exists because `domain_model_setup.xlsx` is a binary file that an AI assistant can neither
edit nor diff, which makes it the single largest obstacle to agent-assisted work. `dump` is what
makes it diffable; the write verbs are what make bundle edits mechanical and verifiable instead of
"ask the user to open Excel".

Skills under [`../.claude/`](../.claude/) are thin wrappers over these verbs and hold no unique
knowledge of their own — the tool is the mechanism.
