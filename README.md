# GHLearning-EasyFusionCache

[![.NET](https://github.com/gordon-hung/GHLearning-EasyFusionCache/actions/workflows/dotnet.yml/badge.svg)](https://github.com/gordon-hung/GHLearning-EasyFusionCache/actions/workflows/dotnet.yml)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/gordon-hung/GHLearning-EasyFusionCache)

[![codecov](https://codecov.io/github/gordon-hung/GHLearning-EasyFusionCache/graph/badge.svg?token=D12HRpI8Yu)](https://codecov.io/github/gordon-hung/GHLearning-EasyFusionCache)

EasyFusionCache 是一個示範專案，展示如何在 **.NET 8.0** 中整合 [FusionCache](https://github.com/ZiggyCreatures/FusionCache)，用來提升資料存取效能並強化系統穩定性。

## 特色

* 簡化快取操作
* 支援 Fail-Safe 機制
* 可設定到期策略（絕對/滑動）

## 範例程式碼

```csharp
var cache = new FusionCache(new FusionCacheOptions());

// 設定快取
await cache.SetAsync("user:1", new User { Id = 1, Name = "Gordon" }, TimeSpan.FromMinutes(5));

// 快取不存在時自動從資料來源取得
var user = await cache.GetOrSetAsync(
    "user:1",
    async _ => await userService.GetUserAsync(1),
    TimeSpan.FromMinutes(5)
);
```

## 適用場景

* API 回應加速
* 頻繁查詢的資料快取
* 高流量系統容錯設計
