---
name: dotnet-best-practices
description: 'Ensure .NET/C# code meets best practices for the solution/project.'
---

# .NET/C# Best Practices

Your task is to ensure .NET/C# code in ${selection} meets the best practices specific to this solution/project. This includes:

## Documentation & Structure

- **全ての public 型（class / interface / record / enum）および public メンバー（プロパティ・メソッド・フィールド）に `/// <summary>` を付与すること**
- **XML ドキュメントコメントは日本語で記述する**（`<summary>`, `<param>`, `<returns>`, `<remarks>` 等すべて）
- インターフェースを実装するメンバーには `/// <inheritdoc />` を使用し、定義元のドキュメントを継承する
- Follow the established namespace structure: {Core|Console|App|Service}.{Feature}

### XML ドキュメントコメントの記述例

```csharp
// 型レベル: 1行目に役割、2行目以降に補足
/// <summary>
/// 現在のリクエストユーザーを表すインターフェース。
/// Web では ClaimsPrincipal から、Azure Functions では認証トークンから生成する。
/// </summary>
public interface ICurrentUser
{
    /// <summary>ユーザーの一意識別子。</summary>
    string UserId { get; }
}

// インターフェース実装: inheritdoc で継承
public class CurrentUser : ICurrentUser
{
    /// <inheritdoc />
    public string UserId => _userId;
}

// static メソッド
/// <summary>成功結果を生成する。</summary>
public static Result Success() => new() { IsSuccess = true };
```

## Design Patterns & Architecture

- Use primary constructor syntax for dependency injection (e.g., `public class MyClass(IDependency dependency)`)
- Implement the Command Handler pattern with generic base classes (e.g., `CommandHandler<TOptions>`)
- Use interface segregation with clear naming conventions (prefix interfaces with 'I')
- Follow the Factory pattern for complex object creation.

## Dependency Injection & Services

- Use constructor dependency injection with null checks via ArgumentNullException
- Register services with appropriate lifetimes (Singleton, Scoped, Transient)
- Use Microsoft.Extensions.DependencyInjection patterns
- Implement service interfaces for testability

## Resource Management & Localization

- Use ResourceManager for localized messages and error strings
- Separate LogMessages and ErrorMessages resource files
- Access resources via `_resourceManager.GetString("MessageKey")`

## Async/Await Patterns

- Use async/await for all I/O operations and long-running tasks
- Return Task or Task<T> from async methods
- Use ConfigureAwait(false) where appropriate
- Handle async exceptions properly

## Testing Standards

- Use MSTest framework with FluentAssertions for assertions
- Follow AAA pattern (Arrange, Act, Assert)
- Use Moq for mocking dependencies
- Test both success and failure scenarios
- Include null parameter validation tests

## Configuration & Settings

- Use strongly-typed configuration classes with data annotations
- Implement validation attributes (Required, NotEmptyOrWhitespace)
- Use IConfiguration binding for settings
- Support appsettings.json configuration files

## Semantic Kernel & AI Integration

- Use Microsoft.SemanticKernel for AI operations
- Implement proper kernel configuration and service registration
- Handle AI model settings (ChatCompletion, Embedding, etc.)
- Use structured output patterns for reliable AI responses

## Error Handling & Logging

- Use structured logging with Microsoft.Extensions.Logging
- Include scoped logging with meaningful context
- Throw specific exceptions with descriptive messages
- Use try-catch blocks for expected failure scenarios

## Performance & Security

- Use C# 12+ features and .NET 8 optimizations where applicable
- Implement proper input validation and sanitization
- Use parameterized queries for database operations
- Follow secure coding practices for AI/ML operations

## Code Quality

- Ensure SOLID principles compliance
- Avoid code duplication through base classes and utilities
- Use meaningful names that reflect domain concepts
- Keep methods focused and cohesive
- Implement proper disposal patterns for resources
