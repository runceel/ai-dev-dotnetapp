---
name: workspace-file-discovery
description: Visual Studio 版 GitHub Copilot 専用。ソリューションに含まれないファイル（docs/ や README.md 等）を探索する際のガイド。Visual Studio Code や GitHub Copilot CLI では本スキルを使用しないこと。
---

# ワークスペースファイル探索ガイド（Visual Studio 専用）

Visual Studio 版 GitHub Copilot では、ソリューションに含まれないファイル（`docs/`、`README.md` 等）は `file_search` や `code_search` のインデックス対象外になる場合がある。

## ルール

- ソリューション外のファイル（`docs/`、ルート直下の `.md` 等）の一覧取得には **PowerShell コマンドを使用**すること
- `file_search` で結果が空になる前に、対象がソリューション外かを判断し、該当する場合は最初から PowerShell を使うこと

## 推奨コマンド例

### ドキュメントファイルの一覧取得

```powershell
Get-ChildItem -Path "<ワークスペースルート>" -Recurse -Include *.md,*.txt,*.docx,*.xlsx,*.pptx -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '(node_modules|\.git|bin|obj)' } | Select-Object -ExpandProperty FullName
```

### 特定ディレクトリ配下の探索

```powershell
Get-ChildItem -Path "<ワークスペースルート>\docs" -Recurse -File | Select-Object -ExpandProperty FullName
```

## 注意事項

- ファイルの **内容** は `get_file` ツールで読み取れる（パスさえ分かればよい）
- 本スキルが解決するのは「パスの発見」の問題のみ
