# Encode CLI

Command-line utility for hashing or encoding text and files. The tool supports hashing to Base64 or hexadecimal (upper- or lowercase) and encoding/decoding using Base64 or URL encoding.

## Usage

```bash
dotnet run -- hash <algorithm> <input> [options]
dotnet run -- encode <algorithm> <input> [options]
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

### Output to a file
Use `--out` with either command to write the result to disk instead of stdout.
```bash
dotnet run -- hash SHA-256 "hello world" --out output.txt
dotnet run -- encode base64 "hello world" --out output.txt
```

## Local development

### Prerequisites
- Install the .NET 9 SDK (target framework: `net9.0`).

### Build
```bash
dotnet build
```
