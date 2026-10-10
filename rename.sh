#!/usr/bin/env bash
# Renames this template to your project.
#
#   ./rename.sh Acme.Orders
#
# The name must be a valid C# namespace segment sequence (letters, digits, dots;
# each part starting with a letter, e.g. Contoso.Billing). It replaces:
#   ProjectName  -> Acme.Orders   (namespaces, project/solution/folder/file names)
#   projectname  -> acme-orders   (Docker image/user names, database, JWT issuer)
#   PROJECTNAME  -> ACME_ORDERS   (environment variable prefixes)
# Then deletes itself. Run it once, before the first commit of your project.
set -euo pipefail

if [ $# -ne 1 ] || ! [[ "$1" =~ ^[A-Za-z][A-Za-z0-9]*(\.[A-Za-z][A-Za-z0-9]*)*$ ]]; then
  echo "usage: $0 <Pascal.Case.Name>   (letters/digits, dot-separated)" >&2
  exit 1
fi

pascal="$1"
lower="$(echo "$pascal" | sed -E 's/([a-z0-9])([A-Z])/\1-\2/g; s/\./-/g' | tr '[:upper:]' '[:lower:]')"
upper="$(echo "$lower" | tr '[:lower:]-' '[:upper:]_')"
cd "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# 1. Contents.
grep -rIl --exclude-dir=.git --exclude-dir=bin --exclude-dir=obj --exclude=rename.sh \
  -e 'ProjectName' -e 'projectname' -e 'PROJECTNAME' . |
  while IFS= read -r file; do
    sed -i -e "s/ProjectName/${pascal}/g" -e "s/projectname/${lower}/g" -e "s/PROJECTNAME/${upper}/g" "$file"
  done

# 2. File and directory names, deepest first.
find . -depth -name '*ProjectName*' -not -path './.git/*' -not -path '*/bin/*' -not -path '*/obj/*' |
  while IFS= read -r path; do
    dir="$(dirname "$path")"
    base="$(basename "$path")"
    mv "$path" "${dir}/${base//ProjectName/${pascal}}"
  done

echo "Renamed to '${pascal}' (docker/db/issuer: '${lower}', env prefix: '${upper}_')."
echo "Next: dotnet build && dotnet test tests/${pascal}.UnitTests"
rm -- "$0"
