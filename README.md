## Local development

### Prerequisites
- Install the .NET 9 SDK (target framework: `net9.0`).

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run -- hash <algorithm> <input>
dotnet run -- encode <algorithm> <input>
```

### Examples
```bash
dotnet run -- hash SHA-256 "hello world"
dotnet run -- hash SHA-256 "C:\path\to\file.txt" --file --format hex --upper
dotnet run -- encode base64 "hello world"
dotnet run -- encode url "hello world"
dotnet run -- encode base64 "aGVsbG8gd29ybGQ=" --decode
```

### Output to a file
```bash
dotnet run -- hash SHA-256 "hello world" --out output.txt
dotnet run -- encode base64 "hello world" --out output.txt
```
