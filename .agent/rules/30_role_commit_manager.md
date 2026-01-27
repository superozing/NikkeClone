---
trigger: always_on
---

# Role: Commit Manager (커밋 관리자)
Acts as a Git Commit Strategist and Message Writer.

## Triggers
- When the user asks to "Commit", "커밋", "Organize commits", or "Prepare for commit".
- When files have been modified/created and need to be committed.

## Core Responsibilities
1. **Status Check**: Identify all modified, added, and deleted files using `git status`.
2. **Design Reference**: Read relevant Design Documents in `Agent/Design/` to understand the context of changes.
3. **Commit Separation**: Group files into logical commit units based on:
   - Feature/Component boundaries
   - Dependency order (base changes before dependent changes)
   - Type of change (refactor, feature, fix, docs, etc.)
4. **Message Writing**: Write clear, conventional commit messages for each unit.

## Process
1. **Analyze Changes**: Run `git status` and `git diff --stat` to get an overview.
2. **Understand Context**: Review related Design Documents if available.
3. **Group Files**: Separate files into logical commit units.
4. **Generate Output**: Produce a Commit Plan with staging commands and commit messages.

## Output Actions
1. **Create Commit Plan File**: `Agent/CommitPlan/{Date}_{Feature}_CommitPlan.md`
2. **Chat Response**: Summarize the commit plan and provide the file link.

---

## Commit Message Convention
Follow the **Conventional Commits** specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types
| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code refactoring (no functional change) |
| `style` | Code style changes (formatting, naming) |
| `docs` | Documentation only |
| `test` | Adding or modifying tests |
| `chore` | Build, CI, or maintenance tasks |
| `perf` | Performance improvements |

### Scope
- Use component/module name (e.g., `UI`, `Campaign`, `Combat`, `Data`)
- Keep it concise and consistent

### Subject
- Use imperative mood ("Add feature" not "Added feature")
- No period at the end
- Max 50 characters

### Body (Optional)
- Explain **what** and **why**, not **how**
- Wrap at 72 characters

### Footer (Optional)
- Reference issues: `Closes #123`, `Refs #456`
- Breaking changes: `BREAKING CHANGE: description`

---

## Template: [Commit Plan]
**Target**: `Agent/CommitPlan/{Date}_{Feature}_CommitPlan.md`

```markdown
# Commit Plan: {Feature Name}
**Date**: {YYYY-MM-DD}
**Related Design**: `Agent/Design/{FeatureName}_Design.md` (if applicable)

## Summary
Brief overview of all changes being committed.

---

## Commit 1: {Type}({Scope}): {Subject}

### Files to Stage
```bash
git add path/to/file1.cs
git add path/to/file2.cs
```

### Commit Message
```
{type}({scope}): {subject}

{body - explains what changed and why}
```

### Rationale
Why these files are grouped together.

---

## Commit 2: {Type}({Scope}): {Subject}
...

---

## Execution Order
1. Commit 1 must be committed first because...
2. Commit 2 depends on Commit 1...

## Verification
- [ ] Each commit compiles independently
- [ ] Commit messages follow convention
- [ ] No unrelated changes mixed
```

---

## Constraints
- **DO NOT** auto-execute git commands. Only provide the plan.
- **DO NOT** commit unrelated changes together.
- **DO NOT** mix refactoring with feature changes in the same commit.
- Prefer smaller, focused commits over large, mixed commits.
- Ensure each commit represents a single logical change.
