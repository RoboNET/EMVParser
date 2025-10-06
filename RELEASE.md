# Automated NuGet Package Release Guide

This document describes the setup and usage of the automated release system for RoboNet.EMVParser.

## 🚀 How It Works

On every push to the `main` branch, the system automatically:
1. Analyzes commits to determine version type (patch/minor/major)
2. Updates the version in the `.csproj` file
3. Creates a Git tag (e.g., `v0.9.0`)
4. Builds the NuGet package
5. Publishes to [nuget.org](https://www.nuget.org)
6. Creates a GitHub Release with changelog and attached `.nupkg` file

## 📋 Initial Setup

### Step 1: Obtain NuGet API Key

1. Go to [nuget.org](https://www.nuget.org) and sign in to your account
2. Click on username → **API Keys**
3. Click **Create** to create a new key
4. Fill out the form:
   - **Key Name**: `GitHub Actions RoboNet.EMVParser`
   - **Select Scopes**: ✅ Push
   - **Select Packages**: select `RoboNet.EMVParser` (or use `*` for all packages)
   - **Glob Pattern**: `*`
5. Copy the created API key (it's shown only once!)

### Step 2: Add API Key to GitHub Secrets

1. Open the repository on GitHub
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter:
   - **Name**: `NUGET_API_KEY`
   - **Secret**: paste the copied API key
5. Click **Add secret**

### Step 3: Create Initial Tag

Since the repository doesn't have tags yet, you need to create the initial tag manually:

```bash
# Make sure you're on the main branch with the latest changes
git checkout main
git pull

# Create a tag with the current version (from .csproj)
git tag v0.8.0

# Push the tag to the repository
git push origin v0.8.0
```

### Step 4: Verify Setup

1. Navigate to **Actions** in your GitHub repository
2. Ensure that the `.NET` workflow runs successfully
3. The system is now ready for automated releases!

## 📝 Commit Naming Rules (Conventional Commits)

Use Conventional Commits for automatic version determination:

### Commit Types and Version Impact:

| Prefix | Description | Version Change | Example |
|---------|----------|------------------|--------|
| `fix:` | Bug fix | **patch** (0.8.0 → 0.8.1) | `fix: fixed memory leak in parser` |
| `feat:` | New functionality | **minor** (0.8.0 → 0.9.0) | `feat: added EMV tags 4.0 support` |
| `BREAKING CHANGE:` | Incompatible changes | **major** (0.8.0 → 1.0.0) | `feat!: changed ParseTags method API`<br>`BREAKING CHANGE: removed deprecated method` |
| `docs:` | Documentation | No version change | `docs: updated README` |
| `test:` | Tests | No version change | `test: added unit tests for DOL parser` |
| `chore:` | Technical work | No version change | `chore: dependency updates` |
| `refactor:` | Refactoring | No version change* | `refactor: TLV parser optimization` |

\* If a `refactor:` commit contains `BREAKING CHANGE:`, it will trigger a major version

### Examples of Correct Commits:

```bash
# Patch release (0.8.0 → 0.8.1)
git commit -m "fix: correct handling of empty DOL structures"

# Minor release (0.8.0 → 0.9.0)
git commit -m "feat: added support for long tags"

# Minor release with detailed description
git commit -m "feat: parsing nested constructed tags

Added recursive parsing for EMV tags with nested structure.
Supports up to 10 nesting levels."

# Major release (0.8.0 → 1.0.0)
git commit -m "feat!: new tag API

BREAKING CHANGE: GetTagValue method now returns TagPointer instead of byte[].
Use TagPointer.Value to get data."
```

### Multiple Changes in One Commit:

If one commit contains multiple types of changes, the most significant is used:

```bash
# Will be major (due to BREAKING CHANGE)
git commit -m "feat: new API and fixes

- feat: added ParseAsync method
- fix: fixed bug in DOL parser
- BREAKING CHANGE: removed deprecated Parse method"
```

## 🔄 Release Process

### Automatic Release (Recommended):

1. Make changes to the code
2. Create a commit with the correct prefix:
   ```bash
   git add .
   git commit -m "feat: added new feature"
   git push origin main
   ```
3. GitHub Actions will automatically:
   - Determine the new version (e.g., 0.8.0 → 0.9.0)
   - Update the version in `.csproj`
   - Create tag `v0.9.0`
   - Build and test the package
   - Publish to nuget.org
   - Create a GitHub Release

### Manual Release (If Control Is Needed):

If you need to create a release without triggering on main:

1. Create a version tag locally:
   ```bash
   git tag v0.9.0
   git push origin v0.9.0
   ```
2. The workflow will automatically start when the tag is created

## 📊 Check Release Status

### GitHub Actions:
1. Navigate to **Actions** in the repository
2. Find the **Release NuGet Package** workflow
3. Check the status of the last run

### NuGet.org:
1. Open https://www.nuget.org/packages/RoboNet.EMVParser
2. Verify that the new version is published (may take 5-10 minutes)

### GitHub Releases:
1. Navigate to **Releases** in the repository
2. Verify that a new release with the correct changelog was created

## ⚠️ Important Notes

### What Does NOT Trigger a Release:
- Changes to `.md` files (README, docs)
- Changes in `docs/` and `samples/` folders
- Changes to `LICENSE.md`
- Pull requests (only after merge to main)

### Version Rollback:
If you need to rollback a release:

1. Delete the tag locally and on GitHub:
   ```bash
   git tag -d v0.9.0
   git push origin :refs/tags/v0.9.0
   ```
2. Delete the package on nuget.org (within 72 hours of publication):
   - Go to the package page
   - Manage Package → Delete
3. Delete the GitHub Release on the Releases page

### Skip Version:
If you need a commit to NOT create a release, use `[skip ci]` in the message:

```bash
git commit -m "docs: documentation update [skip ci]"
```

## 🐛 Troubleshooting

### Error: "401 Unauthorized" when publishing to NuGet
- Verify that `NUGET_API_KEY` is added to GitHub Secrets
- Ensure that the API key hasn't expired (check on nuget.org)
- Check the key scope (should be "Push")

### Error: "Package already exists"
- NuGet doesn't allow overwriting versions
- Delete the version on nuget.org (if within 72 hours)
- Or manually increment the version and create a new tag

### Workflow doesn't start
- Verify that changes are not in ignored paths (`**.md`, `docs/**`, etc.)
- Ensure the push is to the `main` branch
- Check the logs in Actions

### Wrong version is determined
- Ensure you're using the correct commit prefixes
- Check the last tag: `git describe --tags --abbrev=0`
- The workflow analyzes commits since the last tag

## 📚 Additional Resources

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)

## 💡 Tips

1. **Use meaningful commits**: a good changelog is generated from good commits
2. **Group changes**: one feature = one commit with `feat:`
3. **Test locally**: before pushing to main, ensure tests pass
4. **Monitor versions**: verify that the version increments correctly
5. **Document breaking changes**: always specify `BREAKING CHANGE:` with description

## 🎯 Workflow Examples

### Bug Fix:
```bash
# 1. Fix the code
# 2. Commit
git add .
git commit -m "fix: correct handling of null values in TagPointer"
git push origin main
# 3. A patch version will be created automatically (0.8.0 → 0.8.1)
```

### New Feature:
```bash
# 1. Implement the functionality
# 2. Commit
git add .
git commit -m "feat: added EMV Contactless specification support"
git push origin main
# 3. A minor version will be created automatically (0.8.0 → 0.9.0)
```

### Breaking Change:
```bash
# 1. Make incompatible changes
# 2. Commit with detailed description
git add .
git commit -m "feat!: new asynchronous API

The old synchronous API is marked as Obsolete.

BREAKING CHANGE: TLVParser.Parse method replaced with TLVParser.ParseAsync.
Use await TLVParser.ParseAsync() instead of TLVParser.Parse()."
git push origin main
# 3. A major version will be created automatically (0.8.0 → 1.0.0)
```