# Encode CLI

Command-line utility for hashing, encoding, and encrypting text and files. The tool supports hashing to Base64 or hexadecimal (upper- or lowercase), encoding/decoding using Base64 or URL encoding, and AES-256-GCM encryption.

## Usage

```bash
dotnet run -- hash <algorithm> <input> [options]
dotnet run -- encode <algorithm> <input> [options]
dotnet run -- encrypt <algorithm> <input> [options]
dotnet run -- inspect <file>
dotnet run -- generateKey [options]
```

`<input>` can be raw text or a file path when `--file` is supplied.

### Hash command

Supported algorithms: `MD5`, `SHA-1`, `SHA-256`, `SHA-384`, `SHA-512` (common dash-less variants are also accepted).

Options:
- `--file, -f` Treat the input as a file path and hash the file contents.
- `--out <path>, -o <path>` Write output to the given file instead of stdout.
- `--format <base64|hex>` Output format (defaults to `base64`).
- `--upper` Uppercase hex output (only valid with `--format hex`).
- `--lower` Lowercase hex output (only valid with `--format hex`, default when using hex).

Examples:
```bash
# Hash text to base64 (default)
dotnet run -- hash SHA-256 "hello world"

# Hash a file to uppercase hex
dotnet run -- hash SHA-256 "C:\path\to\file.txt" --file --format hex --upper
```

### Encode command

Supported algorithms: `base64` and `url` (Base64 decode/encode and URL encode/decode).

Options:
- `--file, -f` Treat the input as a file path and use the file contents.
- `--out <path>, -o <path>` Write output to the given file instead of stdout.
- `--decode, -d` Decode instead of encode.

Examples:
```bash
# Encode text to base64
dotnet run -- encode base64 "hello world"

# Decode base64 input
dotnet run -- encode base64 "aGVsbG8gd29ybGQ=" --decode

# URL-encode text
dotnet run -- encode url "hello world"
```

### Encrypt command

Supported algorithms: `AES-256-GCM` (common dash-less variants are also accepted).

Options:
- `--file, -f` Treat the input as a file path and use the file contents.
- `--out <path>, -o <path>` Write output to the given file (defaults to `encrypt-output.txt` or `decrypt-output.txt` in the project root).
- `--decrypt, -d` Decrypt instead of encrypt.
- `--password <value>, -p <value>` Password to derive a key from (UTF-8 text).
- `--key <value>, -k <value>` Encryption key bytes (required unless `--password` is provided).
- `--key-format <base64|hex>` Key format (defaults to `base64`).
- `--nonce <value>, -n <value>` Nonce/IV for AES-GCM (optional, uses `--key-format`).
- `--aad <value>` Associated data (AAD) as UTF-8 text (optional).
- `--format <base64|hex>` Output payload format (defaults to `base64`).
- `--upper` Uppercase hex output (only valid with `--format hex`).
- `--lower` Lowercase hex output (only valid with `--format hex`, default when using hex).

Examples:
```bash
# Encrypt text to base64 payload
dotnet run -- encrypt AES-256-GCM "hello world" --key <base64-key>

# Decrypt a hex payload
dotnet run -- encrypt AES-256-GCM "<payload>" --decrypt --key <hex-key> --key-format hex --format hex
```

### Inspect command

Use `inspect` to view non-secret envelope metadata for an encrypted file.

Examples:
```bash
dotnet run -- inspect "C:\path\to\encrypt-output.txt"
```

### Envelope output

Encrypted files are stored as JSON envelopes with a version field:
- `version` `1` is the current envelope format.
- Envelopes without a version are treated as legacy `0` for inspection.

Example inspect output:
```json
{
  "version": {
    "value": "1",
    "legacy": false
  },
  "algorithm": {
    "id": "A256GCM",
    "name": "AES-256-GCM",
    "keySizeBytes": 32
  },
  "file": {
    "path": "C:\\path\\to\\encrypt-output.txt",
    "name": "encrypt-output.txt",
    "sizeBytes": 1234,
    "lastModifiedUtc": "2025-12-31T22:15:30.1234567Z"
  },
  "envelope": {
    "kdf": "PBKDF2-SHA256",
    "iterations": 310000,
    "salt": "base64-or-hex-salt",
    "format": "base64"
  }
}
```

### Generate key command

Options:
- `--bytes <count>, -b <count>` Number of bytes to generate (defaults to 32).
- `--format <base64|hex>` Output format (defaults to `base64`).
- `--upper` Uppercase hex output (only valid with `--format hex`).
- `--lower` Lowercase hex output (only valid with `--format hex`, default when using hex).

Examples:
```bash
# Generate a 32-byte base64 key (default)
dotnet run -- generateKey

# Generate a 32-byte hex key
dotnet run -- generateKey --format hex --upper
```

### Output to a file
Use `--out` with hash or encode to write the result to disk instead of stdout. Encrypt writes to a file by default (project root) unless you provide `--out`.
```bash
dotnet run -- hash SHA-256 "hello world" --out output.txt
dotnet run -- encode base64 "hello world" --out output.txt
dotnet run -- encrypt AES-256-GCM "hello world" --key <base64-key> --out output.txt
```

## Install as a .NET tool (download from GitHub)
Packaged releases are published as `.nupkg` files so you can install the CLI without building from source.

1. Download the latest `encode-cli.0.0.1.nupkg` artifact from the GitHub release assets or the workflow run artifacts.
2. Install the tool from the folder containing the downloaded package:
   ```bash
   dotnet tool install --global encode-cli --version 0.0.1 --add-source .
   ```
3. Verify the install:
   ```bash
   enc --help
   ```

## Local development

### Prerequisites
- Install the .NET 9 SDK (target framework: `net9.0`).

### Build
```bash
dotnet build
```
