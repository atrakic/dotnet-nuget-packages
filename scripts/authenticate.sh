dotnet nuget add source \
  --username "$USERNAME" --password "${GITHUB_TOKEN}" \
  --store-password-in-clear-text \
  --name github "https://nuget.pkg.github.com/$NAMESPACE/index.json"
