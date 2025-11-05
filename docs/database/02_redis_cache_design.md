# ================================================================================
# Stock Trading Algorithm DDD System - Redis Cache Design
# ================================================================================
# Author: Development Team
# Date: 2025-10-27
# Description: Redis cache structure and strategy for DDD-based stock trading system
# ================================================================================

## Redis 개요

Redis는 다음 목적으로 사용됩니다:
- **L1 캐시**: 빠른 데이터 조회
- **실시간 데이터**: 시세, 호가 등 실시간 정보
- **세션 관리**: 사용자 세션
- **분산 락**: 동시성 제어
- **Pub/Sub**: 실시간 알림

## Redis 데이터베이스 분리 전략

```
DB 0: 계좌 캐시 (Account Context)
DB 1: 거래 캐시 (Trading Context)  
DB 2: 전략 캐시 (Strategy Context)
DB 3: 시장 데이터 (Market Data Context)
DB 4: 세션 및 인증
DB 5: 분산 락
DB 6: Pub/Sub 메시지 큐
```

## 1. Account Context 캐시 (DB 0)

### 1.1 계좌 정보 캐시
```
Key Pattern: account:{account_id}
Type: Hash
TTL: 5 minutes
Fields:
  - id: UUID
  - account_number: String
  - account_name: String
  - account_type: String
  - account_status: String
  - total_assets: Decimal
  - cash_balance: Decimal
  - available_balance: Decimal
  - updated_at: Timestamp

Example:
HSET account:550e8400-e29b-41d4-a716-446655440000 
  id "550e8400-e29b-41d4-a716-446655440000"
  account_number "1234567890"
  account_name "My Trading Account"
  account_type "CASH"
  account_status "ACTIVE"
  total_assets "10000000.00"
  cash_balance "5000000.00"
  available_balance "4500000.00"
  updated_at "2025-10-27T10:30:00Z"

EXPIRE account:550e8400-e29b-41d4-a716-446655440000 300
```

### 1.2 계좌 잔고 요약 (빠른 조회)
```
Key Pattern: account_balance:{account_id}
Type: String (JSON)
TTL: 1 minute

Value:
{
  "cash_balance": 5000000.00,
  "total_assets": 10000000.00,
  "available_balance": 4500000.00,
  "total_pl": 500000.00,
  "total_pl_percent": 5.25,
  "last_synced": "2025-10-27T10:30:00Z"
}
```

### 1.3 계좌 성과 지표 (실시간)
```
Key Pattern: account_performance:{account_id}:{date}
Type: Hash
TTL: 1 day

Fields:
  - daily_return: Decimal
  - daily_return_percent: Decimal
  - equity_value: Decimal
  - sharpe_ratio: Decimal
  - win_rate: Decimal
```

## 2. Trading Context 캐시 (DB 1)

### 2.1 주문 상태 캐시
```
Key Pattern: order:{order_id}
Type: Hash
TTL: 10 minutes

Fields:
  - id: UUID
  - account_id: UUID
  - stock_code: String
  - order_type: String
  - order_side: String
  - order_status: String
  - order_price: Decimal
  - order_quantity: Integer
  - executed_quantity: Integer
  - remaining_quantity: Integer
  - created_at: Timestamp
  - updated_at: Timestamp

Example:
HMSET order:abc123-def456-ghi789
  id "abc123-def456-ghi789"
  account_id "550e8400-e29b-41d4-a716-446655440000"
  stock_code "005930"
  order_type "LIMIT"
  order_side "BUY"
  order_status "PARTIALLY_FILLED"
  order_price "70000"
  order_quantity "100"
  executed_quantity "50"
  remaining_quantity "50"
```

### 2.2 활성 주문 목록 (계좌별)
```
Key Pattern: account_active_orders:{account_id}
Type: Set
TTL: 5 minutes

Members: Order IDs

Commands:
SADD account_active_orders:550e8400 order_id_1 order_id_2 order_id_3
SMEMBERS account_active_orders:550e8400
```

### 2.3 포지션 캐시
```
Key Pattern: position:{account_id}:{stock_code}
Type: Hash
TTL: 5 minutes

Fields:
  - id: UUID
  - account_id: UUID
  - stock_code: String
  - quantity: Integer
  - average_entry_price: Decimal
  - current_price: Decimal
  - unrealized_pl: Decimal
  - unrealized_pl_percent: Decimal
  - stop_loss_price: Decimal
  - take_profit_price: Decimal
```

### 2.4 계좌 전체 포지션 목록
```
Key Pattern: account_positions:{account_id}
Type: Sorted Set (Score = 진입시간)
TTL: 5 minutes

Commands:
ZADD account_positions:550e8400 1698403200 "005930"
ZADD account_positions:550e8400 1698406800 "035720"
ZRANGE account_positions:550e8400 0 -1
```

### 2.5 주문 실행 대기 큐
```
Key Pattern: order_queue:{priority}
Type: List
TTL: None (persistent)

Commands:
LPUSH order_queue:high order_id_1
LPUSH order_queue:normal order_id_2
RPOP order_queue:high
```

## 3. Strategy Context 캐시 (DB 2)

### 3.1 전략 정보 캐시
```
Key Pattern: strategy:{strategy_id}
Type: Hash
TTL: 5 minutes

Fields:
  - id: UUID
  - strategy_name: String
  - strategy_type: String
  - strategy_status: String
  - max_positions: Integer
  - position_size_percent: Decimal
  - total_trades: Integer
  - win_rate: Decimal
```

### 3.2 활성 전략 목록
```
Key Pattern: active_strategies:{user_id}
Type: Set
TTL: 10 minutes

Members: Strategy IDs
```

### 3.3 전략 신호 캐시 (최근 생성)
```
Key Pattern: strategy_signals:{strategy_id}
Type: List (최대 100개 유지)
TTL: 1 hour

Value: JSON String
{
  "signal_id": "uuid",
  "stock_code": "005930",
  "signal_type": "BUY",
  "signal_strength": 0.85,
  "entry_price": 70000,
  "generated_at": "2025-10-27T10:30:00Z"
}

Commands:
LPUSH strategy_signals:strategy_id '{"signal_id":"...","stock_code":"005930",...}'
LTRIM strategy_signals:strategy_id 0 99  # 최대 100개만 유지
LRANGE strategy_signals:strategy_id 0 9   # 최근 10개 조회
```

### 3.4 전략 성과 (실시간)
```
Key Pattern: strategy_performance:{strategy_id}:{date}
Type: Hash
TTL: 1 day

Fields:
  - total_return: Decimal
  - total_trades: Integer
  - winning_trades: Integer
  - win_rate: Decimal
  - sharpe_ratio: Decimal
```

## 4. Market Data Context 캐시 (DB 3)

### 4.1 실시간 시세 (가장 중요!)
```
Key Pattern: market_price:{stock_code}
Type: Hash
TTL: 10 seconds

Fields:
  - stock_code: String
  - current_price: Decimal
  - open_price: Decimal
  - high_price: Decimal
  - low_price: Decimal
  - volume: Integer
  - change_amount: Decimal
  - change_percent: Decimal
  - timestamp: Timestamp

Example:
HMSET market_price:005930
  stock_code "005930"
  current_price "70500"
  open_price "69000"
  high_price "71000"
  low_price "68500"
  volume "15234567"
  change_amount "1500"
  change_percent "2.17"
  timestamp "2025-10-27T10:30:15Z"
```

### 4.2 실시간 호가 (10호가)
```
Key Pattern: order_book:{stock_code}
Type: Hash
TTL: 5 seconds

Fields:
  - ask_price_1 ~ ask_price_10: Decimal
  - ask_volume_1 ~ ask_volume_10: Integer
  - bid_price_1 ~ bid_price_10: Decimal
  - bid_volume_1 ~ bid_volume_10: Integer
  - total_ask_volume: Integer
  - total_bid_volume: Integer
  - timestamp: Timestamp
```

### 4.3 관심 종목 리스트
```
Key Pattern: watchlist:{user_id}
Type: Sorted Set (Score = 추가 시간)
TTL: None

Commands:
ZADD watchlist:user123 1698403200 "005930"
ZADD watchlist:user123 1698406800 "035720"
ZRANGE watchlist:user123 0 -1
ZREVRANGE watchlist:user123 0 9  # 최근 10개
```

### 4.4 최근 체결가 (Tick 데이터)
```
Key Pattern: tick_data:{stock_code}
Type: List (최대 1000개)
TTL: 1 minute

Value: JSON String
{
  "price": 70500,
  "volume": 1500,
  "side": "BUY",
  "timestamp": "2025-10-27T10:30:15.123Z"
}

Commands:
LPUSH tick_data:005930 '{"price":70500,"volume":1500,...}'
LTRIM tick_data:005930 0 999
```

### 4.5 캔들 데이터 캐시 (최근 200개)
```
Key Pattern: candle:{stock_code}:{interval}
Type: List
TTL: 5 minutes

interval: 1m, 5m, 15m, 1h, 1d

Value: JSON String
{
  "timestamp": "2025-10-27T10:30:00Z",
  "open": 70000,
  "high": 71000,
  "low": 69500,
  "close": 70500,
  "volume": 1234567
}
```

### 4.6 시장 상태
```
Key Pattern: market_status:{market_type}
Type: String
TTL: 1 minute

Value: "TRADING" | "PRE_OPEN" | "CLOSED" | "HALTED"

Example:
SET market_status:KOSPI "TRADING"
EXPIRE market_status:KOSPI 60
```

## 5. 세션 및 인증 (DB 4)

### 5.1 사용자 세션
```
Key Pattern: session:{session_id}
Type: Hash
TTL: 30 minutes (활동시 갱신)

Fields:
  - user_id: UUID
  - username: String
  - email: String
  - login_at: Timestamp
  - last_activity: Timestamp
  - ip_address: String
```

### 5.2 API 토큰
```
Key Pattern: api_token:{token}
Type: String (User ID)
TTL: 1 hour

Commands:
SET api_token:abc123xyz user_id_here
EXPIRE api_token:abc123xyz 3600
```

### 5.3 로그인 시도 제한 (Rate Limiting)
```
Key Pattern: login_attempts:{ip_address}
Type: String (Counter)
TTL: 15 minutes

Commands:
INCR login_attempts:192.168.1.100
EXPIRE login_attempts:192.168.1.100 900
GET login_attempts:192.168.1.100
```

## 6. 분산 락 (DB 5)

### 6.1 주문 처리 락
```
Key Pattern: lock:order:{order_id}
Type: String
TTL: 30 seconds

Commands:
SET lock:order:abc123 process_id NX EX 30
# NX = 없을 때만 SET, EX = 만료시간(초)
```

### 6.2 포지션 업데이트 락
```
Key Pattern: lock:position:{account_id}:{stock_code}
Type: String
TTL: 10 seconds
```

### 6.3 계좌 잔고 락
```
Key Pattern: lock:account_balance:{account_id}
Type: String
TTL: 5 seconds
```

## 7. Pub/Sub 채널 (DB 6)

### 7.1 실시간 시세 발행
```
Channel: market_price_updates
Message Format (JSON):
{
  "stock_code": "005930",
  "price": 70500,
  "volume": 1234567,
  "timestamp": "2025-10-27T10:30:15Z"
}

Commands:
PUBLISH market_price_updates '{"stock_code":"005930","price":70500,...}'
SUBSCRIBE market_price_updates
```

### 7.2 주문 상태 변경 알림
```
Channel: order_status_updates:{account_id}
Message Format (JSON):
{
  "order_id": "abc123",
  "old_status": "SUBMITTED",
  "new_status": "FILLED",
  "timestamp": "2025-10-27T10:30:15Z"
}
```

### 7.3 전략 신호 발행
```
Channel: strategy_signals:{strategy_id}
Message Format (JSON):
{
  "signal_id": "xyz789",
  "signal_type": "BUY",
  "stock_code": "005930",
  "strength": 0.85,
  "timestamp": "2025-10-27T10:30:15Z"
}
```

### 7.4 시스템 알림
```
Channel: system_alerts
Message Format (JSON):
{
  "alert_type": "RISK",
  "severity": "HIGH",
  "message": "Daily loss limit exceeded",
  "account_id": "550e8400",
  "timestamp": "2025-10-27T10:30:15Z"
}
```

## 8. Redis 설정 권장사항

### 8.1 redis.conf 설정
```
# 최대 메모리 설정 (8GB 예시)
maxmemory 8gb

# 메모리 정책 (LRU - Least Recently Used)
maxmemory-policy allkeys-lru

# 영속성 설정 (RDB + AOF)
save 900 1      # 15분마다 1개 이상 키 변경시 저장
save 300 10     # 5분마다 10개 이상 키 변경시 저장
save 60 10000   # 1분마다 10000개 이상 키 변경시 저장

# AOF 활성화
appendonly yes
appendfsync everysec

# 최대 연결 수
maxclients 10000

# TCP Keepalive
tcp-keepalive 300
```

### 8.2 연결 풀 설정 (C#)
```csharp
var configurationOptions = new ConfigurationOptions
{
    EndPoints = { "localhost:6379" },
    DefaultDatabase = 0,
    ConnectTimeout = 5000,
    SyncTimeout = 5000,
    AsyncTimeout = 5000,
    ConnectRetry = 3,
    KeepAlive = 60,
    AbortOnConnectFail = false
};

var connection = ConnectionMultiplexer.Connect(configurationOptions);
```

## 9. 캐시 무효화 전략

### 9.1 계좌 정보 변경시
```csharp
// 1. DB 업데이트
await _accountRepository.UpdateAsync(account);

// 2. 캐시 삭제
await _cache.RemoveAsync($"account:{account.Id}");
await _cache.RemoveAsync($"account_balance:{account.Id}");

// 3. Pub/Sub로 알림 발행
await _pubsub.PublishAsync("account_updates", new {
    account_id = account.Id,
    action = "updated"
});
```

### 9.2 주문 상태 변경시
```csharp
// 1. DB 업데이트
await _orderRepository.UpdateAsync(order);

// 2. 캐시 업데이트 (삭제가 아닌 업데이트)
await _cache.SetAsync($"order:{order.Id}", order, TimeSpan.FromMinutes(10));

// 3. Pub/Sub로 알림 발행
await _pubsub.PublishAsync($"order_status_updates:{order.AccountId}", new {
    order_id = order.Id,
    new_status = order.Status
});
```

## 10. 모니터링 메트릭

### 10.1 Redis 성능 모니터링
```
Key Pattern: redis_metrics:{timestamp}
Type: Hash
TTL: 1 hour

Fields:
  - used_memory: Integer (bytes)
  - hit_rate: Decimal (%)
  - evicted_keys: Integer
  - connected_clients: Integer
  - ops_per_sec: Integer
```

### 10.2 캐시 히트율 추적
```csharp
public async Task<T> GetWithMetrics<T>(string key)
{
    var value = await _cache.GetAsync<T>(key);
    
    if (value != null)
    {
        // Cache Hit
        await _redis.IncrementAsync("cache_hits");
    }
    else
    {
        // Cache Miss
        await _redis.IncrementAsync("cache_misses");
    }
    
    return value;
}
```

## 11. 보안 고려사항

### 11.1 민감 데이터 암호화
```csharp
// 계좌번호 등 민감 정보는 암호화하여 저장
var encryptedAccountNumber = await _encryption.EncryptAsync(accountNumber);
await _cache.SetAsync($"account:{accountId}:sensitive", encryptedAccountNumber);
```

### 11.2 Redis AUTH 설정
```
# redis.conf
requirepass your_secure_password_here

# C# 연결
var config = new ConfigurationOptions
{
    Password = "your_secure_password_here"
};
```

## 12. 백업 전략

### 12.1 RDB 스냅샷
```bash
# 수동 백업
redis-cli BGSAVE

# 백업 파일 저장 경로
dir /var/lib/redis
dbfilename dump.rdb
```

### 12.2 AOF 백업
```bash
# AOF 재작성 (압축)
redis-cli BGREWRITEAOF
```

## 13. 성능 최적화 팁

### 13.1 Pipeline 사용
```csharp
var tasks = new List<Task>();
var batch = _redis.CreateBatch();

foreach (var order in orders)
{
    tasks.Add(batch.StringSetAsync($"order:{order.Id}", 
        JsonSerializer.Serialize(order), 
        TimeSpan.FromMinutes(10)));
}

batch.Execute();
await Task.WhenAll(tasks);
```

### 13.2 Lua Script 사용 (원자적 연산)
```lua
-- 계좌 잔고 업데이트 스크립트
local account_key = KEYS[1]
local amount = tonumber(ARGV[1])

local current_balance = tonumber(redis.call('HGET', account_key, 'cash_balance'))
local new_balance = current_balance + amount

if new_balance >= 0 then
    redis.call('HSET', account_key, 'cash_balance', new_balance)
    redis.call('HSET', account_key, 'updated_at', ARGV[2])
    return new_balance
else
    return -1  -- 잔고 부족
end
```

```csharp
var script = @"
    local account_key = KEYS[1]
    local amount = tonumber(ARGV[1])
    -- ... Lua script ...
";

var result = await _redis.ScriptEvaluateAsync(script, 
    keys: new[] { $"account:{accountId}" },
    values: new[] { amount.ToString(), DateTime.UtcNow.ToString() });
```

## 14. C# StackExchange.Redis 사용 예제

```csharp
using StackExchange.Redis;

public class RedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    
    public RedisCacheService(IConnectionMultiplexer redis, int database = 0)
    {
        _redis = redis;
        _database = _redis.GetDatabase(database);
    }
    
    // Hash 저장
    public async Task SetAccountAsync(TradingAccount account)
    {
        var key = $"account:{account.Id}";
        var entries = new[]
        {
            new HashEntry("id", account.Id.ToString()),
            new HashEntry("account_number", account.AccountNumber),
            new HashEntry("total_assets", account.TotalAssets.ToString()),
            new HashEntry("cash_balance", account.CashBalance.ToString())
        };
        
        await _database.HashSetAsync(key, entries);
        await _database.KeyExpireAsync(key, TimeSpan.FromMinutes(5));
    }
    
    // Hash 조회
    public async Task<TradingAccount> GetAccountAsync(Guid accountId)
    {
        var key = $"account:{accountId}";
        var entries = await _database.HashGetAllAsync(key);
        
        if (entries.Length == 0)
            return null;
        
        // Convert HashEntry[] to TradingAccount
        // ...
    }
    
    // 실시간 시세 발행
    public async Task PublishMarketPriceAsync(string stockCode, decimal price)
    {
        var subscriber = _redis.GetSubscriber();
        var message = JsonSerializer.Serialize(new
        {
            stock_code = stockCode,
            price = price,
            timestamp = DateTime.UtcNow
        });
        
        await subscriber.PublishAsync("market_price_updates", message);
    }
    
    // 실시간 시세 구독
    public void SubscribeMarketPrice(Action<string, decimal> callback)
    {
        var subscriber = _redis.GetSubscriber();
        
        subscriber.Subscribe("market_price_updates", (channel, message) =>
        {
            var data = JsonSerializer.Deserialize<MarketPriceUpdate>(message);
            callback(data.StockCode, data.Price);
        });
    }
}
```

## 15. 주요 키 네이밍 규칙 요약

```
account:{id}                              # 계좌 정보
account_balance:{id}                      # 계좌 잔고
account_positions:{id}                    # 계좌 포지션 목록
order:{id}                                # 주문 정보
position:{account_id}:{stock_code}        # 포지션 정보
strategy:{id}                             # 전략 정보
market_price:{stock_code}                 # 실시간 시세
order_book:{stock_code}                   # 호가 정보
tick_data:{stock_code}                    # 틱 데이터
candle:{stock_code}:{interval}            # 캔들 데이터
session:{session_id}                      # 세션
lock:{resource}:{id}                      # 분산 락
```

## 마무리

이 Redis 설계는 다음을 고려합니다:
- ✅ **성능**: 빠른 조회를 위한 적절한 TTL 설정
- ✅ **확장성**: DB 분리로 논리적 격리
- ✅ **실시간성**: Pub/Sub를 통한 이벤트 기반 아키텍처
- ✅ **안정성**: 영속성, 백업, 모니터링 전략
- ✅ **보안**: 암호화, 인증, 접근 제어
