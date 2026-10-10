#!/usr/bin/env bash
# Creates any missing files of the docs/project-memory pack. Idempotent: it
# never overwrites an existing file. Run from anywhere; paths are relative to
# this script.
set -euo pipefail

dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/docs/project-memory"
mkdir -p "$dir"

create() {
  local file="$dir/$1" title="$2" hint="$3"
  if [ -e "$file" ]; then
    echo "exists:  $1"
    return
  fi
  printf '# %s\n\n%s\n' "$title" "$hint" > "$file"
  echo "created: $1"
}

create 01-brief.md          "Brief"            "What this project is, who it is for, and what it deliberately is not."
create 02-requirements.md   "Requirements"     "Functional and non-functional requirements. One ID per requirement."
create 03-architecture.md   "Architecture"     "Layers, modules, data flow and the reasoning behind the shape."
create 04-security.md       "Security"         "Threat model, auth design, secrets handling, accepted risks."
create 05-testing.md        "Testing"          "Test strategy, what each suite covers, what only CI can verify."
create 06-ops.md            "Operations"       "How to run, deploy, configure and observe the service."
create 07-decisions.md      "Decisions"        "Architecture decision records. Append only; one entry per decision."
create 08-risk.md           "Risks"            "Open risks with owner and review date."
create 09-backlog.md        "Backlog"          "Planned work, in priority order."
create 10-release-notes.md  "Release notes"    "User-visible changes per release."
create 11-retirement-plan.md "Retirement plan" "How data and dependencies are wound down if the project is retired."
create 12-session-handoff.md "Session handoff" "Current state for the next contributor or agent session. Append dated sections; do not rewrite history."
