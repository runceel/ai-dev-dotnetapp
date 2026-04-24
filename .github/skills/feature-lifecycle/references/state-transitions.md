# 状態遷移定義

feature-lifecycle の各フェーズにおける状態遷移を定義する。

---

## エントリモード分岐（Phase 1 開始時）

Orchestrator 起動時に、状態台帳の `entry_mode` を決定する:

- **`draft_issue` パラメータが指定されている** → `entry_mode: "draft"`、Step 1.1 で Product Manager が **指定 Issue を `update-specification` で SPEC 化**
- **未指定** → `entry_mode: "new"`、Step 1.1 で Product Manager が **新規 SPEC Issue を `create-specification` で作成**

両モードとも Step 1.1 完了後は Step 1.2（仕様レビュー）へ合流する。

---

## Phase 1: 仕様策定

- **Step 1.1** → Step 1.2: Product Manager が仕様書 Issue を作成（`new`）または既存 Issue を SPEC 化（`draft`）完了
- **Step 1.2** → Step 1.3: Reviewer が REQUEST CHANGES を返した場合
- **Step 1.2** → Step 1.4: Reviewer が APPROVE を返した場合
- **Step 1.3** → Step 1.2: Product Manager が指摘を反映（ループ上限: 3 回）
- **Step 1.4** → Step 1.5: 仕様確定、人間の承認待ち
- **Step 1.5** → Phase 2: 人間が承認
- **Step 1.5** → Step 1.1: 人間が差し戻し

エスカレーション: 仕様レビューが 3 回を超えた場合 → 人間に判断を求める

---

## Phase 2: 設計・計画

- **Step 2.1** → Step 2.2: Architect が設計方針を策定完了
- **Step 2.2** → Step 2.2.5: Developer が実装計画を仕様書 Issue にコメントとして投稿完了
- **Step 2.2.5** → Step 2.3: Documentation が設計ドキュメント骨子を作成/更新完了
- **Step 2.3** → Step 2.4: Reviewer が REQUEST CHANGES を返した場合
- **Step 2.3** → Phase 3: Reviewer が APPROVE を返した場合
- **Step 2.4** → Step 2.3: Architect / Developer / Documentation が指摘を反映（ループ上限: 2 回）

エスカレーション: 設計レビューが 2 回を超えた場合 → 人間に判断を求める

---

## Phase 3: 実装（自律ループ）

- **Step 3.1** → Step 3.2: Developer が feature ブランチ・Draft PR を作成完了
- **Step 3.2** → Step 3.3: Developer が実装・ユニットテスト完了
- **Step 3.3** → Step 3.4: ビルド成功
- **Step 3.3** → Step 3.2: ビルド失敗 → 修正（ループ上限: 5 回）
- **Step 3.4** → Step 3.4.5: Orchestrator が実装完了を検知
- **Step 3.4.5** → Step 3.4.9: Documentation が docs 更新完了（DONE / UP_TO_DATE）
- **Step 3.4.9** → Step 3.5: Orchestrator が PR を Ready for Review に変更
- **Step 3.5** → Step 3.6: Reviewer が REQUEST CHANGES を返した場合
- **Step 3.5** → Step 3.7: Reviewer が APPROVE を返した場合
- **Step 3.6** → Step 3.5: Developer / Documentation が指摘を反映（ループ上限: 3 回）
- **Step 3.7** → Step 3.8: Tester が FAIL を返した場合
- **Step 3.7** → Step 3.9: Tester が PASS を返した場合
- **Step 3.8** → Step 3.7: Developer がバグ修正
- **Step 3.9** → Step 3.10: Architect が INCOMPLETE を返した場合（docs 不足も INCOMPLETE の対象）
- **Step 3.9** → Step 3.11: Architect が COMPLETE を返した場合
- **Step 3.10** → Step 3.5: Developer / Documentation が改善（ループ上限: 3 回）
- **Step 3.11** → Step 3.12: 人間が承認
- **Step 3.11** → Step 3.2: 人間が差し戻し
- **Step 3.12**: Squash Merge → ブランチ削除 → 完了

エスカレーション:
- ビルド失敗が 5 回を超えた場合 → 人間に判断を求める
- コードレビューが 3 回を超えた場合 → 人間に判断を求める
- 完成判断サイクルが 3 回を超えた場合 → 人間に判断を求める
- ドキュメント更新が 3 回を超えた場合 → 人間に判断を求める

---

## 例外状態

通常のフロー以外で発生する例外状態と、その遷移を定義する。

| 例外状態 | 発生条件 | 遷移先 |
|---------|---------|--------|
| **ENV_FAILED** | テスト環境（AppHost 等）の起動失敗・サービス未応答 | Orchestrator が環境復旧を試行 → 2 回失敗で人間にエスカレーション |
| **AUTH_FAILED** | `gh` CLI 認証エラー、GitHub API 権限不足 | 即時人間にエスカレーション |
| **FLAKY_TEST** | 同一テストシナリオが成功/失敗を繰り返す | Developer に再実行指示（最大 2 回） → 解消しない場合は人間に報告 |
| **BLOCKED** | 外部依存（API、パッケージ、権限）で進行不可 | 原因をコメントに記録し、人間にエスカレーション |
| **WAITING_HUMAN** | 人間の判断待ち（Step 1.5, 3.11 等） | 待機状態。自動リマインドは行わない |

例外状態は `decision` フィールドに記録し、Orchestrator が適切なエスカレーションパスを選択する。
