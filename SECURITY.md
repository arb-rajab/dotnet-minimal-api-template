# Security Policy

## Supported versions

Only the latest commit on `main` receives security fixes until you publish
tagged releases. Update this section when you start tagging.

## Reporting a vulnerability

Please **do not** open a public GitHub issue for security vulnerabilities.

Instead, use GitHub's private vulnerability reporting (Security tab ->
"Report a vulnerability").

Please include:

- A description of the vulnerability and its potential impact
- Steps to reproduce
- The affected commit

## Disclosure process

1. Acknowledgement within 5 business days.
2. Assessment and severity rating.
3. Fix developed on a private branch where feasible.
4. Coordinated disclosure once a fix is merged, with credit to the reporter
   unless anonymity is requested.

## Scope

This policy covers the code, build configuration and GitHub Actions
workflows in this repository. It does not cover third-party dependencies
(report those upstream). Time-limited dependency exemptions live in the
scanner config files (`osv-scanner.toml`) and must carry an expiry date.
