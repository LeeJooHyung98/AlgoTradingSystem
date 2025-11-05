# Entity Framework Core 설정 가이드
## Stock Trading Algorithm DDD System

이 문서는 PostgreSQL 17 + TimescaleDB + Redis와 .NET 8 Entity Framework Core를 연동하는 방법을 설명합니다.

## 목차
1. [NuGet 패키지 설치](#1-nuget-패키지-설치)
2. [연결 문자열 설정](#2-연결-문자열-설정)
3. [DbContext 구성](#3-dbcontext-구성)
4. [엔티티 설정](#4-엔티티-설정)
5. [Redis 설정](#5-redis-설정)
6. [의존성 주입 설정](#6-의존성-주입-설정)
7. [마이그레이션 가이드](#7-마이그레이션-가이드)

---

## 1. NuGet 패키지 설치

### 프로젝트 구조
```
src/
├── AlgoTrading.Infrastructure/
│   └── AlgoTrading.Infrastructure.csproj
```

### 필수 패키지

```xml
<!-- AlgoTrading.Infrastructure.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    
    <!-- PostgreSQL Provider -->
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    
    <!-- TimescaleDB (PostgreSQL 확장, 추가 패키지 불필요) -->
    
    <!-- Redis -->
    <PackageReference Include="StackExchange.Redis" Version="2.7.10" />
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
    
    <!-- 추가 유틸리티 -->
    <PackageReference Include="Dapper" Version="2.1.24" /> <!-- 복잡한 쿼리용 -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AlgoTrading.Domain\AlgoTrading.Domain.csproj" />
    <ProjectReference Include="..\AlgoTrading.Application\AlgoTrading.Application.csproj" />
  </ItemGroup>
</Project>
```

### 패키지 설치 명령
```bash
# Infrastructure 프로젝트로 이동
cd src/AlgoTrading.Infrastructure

# 필수 패키지 설치
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package Dapper
```

---

## 2. 연결 문자열 설정

### appsettings.json
```json
{
  "ConnectionStrings": {
    "AlgoTradingDb": "Host=localhost;Port=5432;Database=algotrading;Username=algotrading_app;Password=your_secure_password;Include Error Detail=true;Timeout=30;CommandTimeout=30",
    "Redis": "localhost:6379,abortConnect=false,connectTimeout=5000,syncTimeout=5000"
  },
  
  "Database": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30"
  },
  
  "Redis": {
    "DefaultDatabase": 0,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "AsyncTimeout": 5000,
    "ConnectRetry": 3,
    "KeepAlive": 60,
    "AbortOnConnectFail": false
  },
  
  "TimescaleDb": {
    "ChunkTimeInterval": "7 days",
    "CompressionAfter": "30 days",
    "RetentionPeriod": "1 year"
  }
}
```

### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "AlgoTradingDb": "Host=localhost;Port=5432;Database=algotrading_dev;Username=postgres;Password=dev_password;Include Error Detail=true",
    "Redis": "localhost:6379"
  },
  
  "Database": {
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  },
  
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

---

## 3. DbContext 구성

### 3.1 Base DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using AlgoTrading.Domain.Common;

namespace AlgoTrading.Infrastructure.Persistence
{
    /// <summary>
    /// 기본 DbContext - 공통 기능 제공
    /// </summary>
    public abstract class BaseDbContext : DbContext
    {
        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 자동으로 UpdatedAt 설정
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### 3.2 Account Context DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using AlgoTrading.Domain.AccountContext;

namespace AlgoTrading.Infrastructure.Persistence.Contexts
{
    public class AccountDbContext : BaseDbContext
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options) 
            : base(options)
        {
        }

        // DbSet 정의
        public DbSet<TradingAccount> TradingAccounts { get; set; } = null!;
        public DbSet<AccountBalance> AccountBalances { get; set; } = null!;
        public DbSet<PerformanceRecord> PerformanceRecords { get; set; } = null!;
        public DbSet<AccountRestriction> AccountRestrictions { get; set; } = null!;
        public DbSet<AccountWithdrawal> AccountWithdrawals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Schema 설정
            modelBuilder.HasDefaultSchema("account");

            // Configuration 적용
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
        }
    }
}
```

### 3.3 Trading Context DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using AlgoTrading.Domain.TradingContext;

namespace AlgoTrading.Infrastructure.Persistence.Contexts
{
    public class TradingDbContext : BaseDbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderExecution> OrderExecutions { get; set; } = null!;
        public DbSet<OrderModification> OrderModifications { get; set; } = null!;
        public DbSet<Position> Positions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("trading");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TradingDbContext).Assembly);
        }
    }
}
```

### 3.4 Strategy Context DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using AlgoTrading.Domain.StrategyContext;

namespace AlgoTrading.Infrastructure.Persistence.Contexts
{
    public class StrategyDbContext : BaseDbContext
    {
        public StrategyDbContext(DbContextOptions<StrategyDbContext> options) 
            : base(options)
        {
        }

        public DbSet<TradingStrategy> TradingStrategies { get; set; } = null!;
        public DbSet<StrategyRule> StrategyRules { get; set; } = null!;
        public DbSet<StrategySignal> StrategySignals { get; set; } = null!;
        public DbSet<BacktestResult> BacktestResults { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("strategy");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StrategyDbContext).Assembly);
        }
    }
}
```

### 3.5 Market Data Context DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using AlgoTrading.Domain.MarketDataContext;

namespace AlgoTrading.Infrastructure.Persistence.Contexts
{
    public class MarketDataDbContext : BaseDbContext
    {
        public MarketDataDbContext(DbContextOptions<MarketDataDbContext> options) 
            : base(options)
        {
        }

        public DbSet<StockInfo> StockInfos { get; set; } = null!;
        public DbSet<MarketData> MarketData { get; set; } = null!;
        public DbSet<CandleData> CandleData { get; set; } = null!;
        public DbSet<OrderBook> OrderBooks { get; set; } = null!;
        public DbSet<TickData> TickData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("market");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketDataDbContext).Assembly);
        }
    }
}
```

---

## 4. 엔티티 설정

### 4.1 TradingAccount Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AlgoTrading.Domain.AccountContext;

namespace AlgoTrading.Infrastructure.Persistence.Configurations.Account
{
    public class TradingAccountConfiguration : IEntityTypeConfiguration<TradingAccount>
    {
        public void Configure(EntityTypeBuilder<TradingAccount> builder)
        {
            builder.ToTable("trading_accounts", "account");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasConversion(
                    id => id.Value,
                    value => new AccountId(value))
                .HasColumnName("id");

            builder.Property(a => a.AccountNumber)
                .HasConversion(
                    an => an.Value,
                    value => new AccountNumber(value))
                .HasColumnName("account_number")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(a => a.AccountName)
                .HasConversion(
                    an => an.Value,
                    value => new AccountName(value))
                .HasColumnName("account_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.AccountType)
                .HasConversion<string>()
                .HasColumnName("account_type")
                .IsRequired();

            builder.Property(a => a.AccountStatus)
                .HasConversion<string>()
                .HasColumnName("account_status")
                .IsRequired();

            builder.Property(a => a.TradingPermission)
                .HasConversion<string>()
                .HasColumnName("trading_permission")
                .IsRequired();

            builder.Property(a => a.TotalAssets)
                .HasConversion(
                    money => money.Amount,
                    value => new Money(value, Currency.KRW))
                .HasColumnName("total_assets")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(a => a.CashBalance)
                .HasConversion(
                    money => money.Amount,
                    value => new Money(value, Currency.KRW))
                .HasColumnName("cash_balance")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(a => a.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // 인덱스
            builder.HasIndex(a => a.AccountNumber)
                .IsUnique()
                .HasDatabaseName("idx_trading_accounts_number");

            builder.HasIndex(a => a.AccountStatus)
                .HasDatabaseName("idx_trading_accounts_status");
        }
    }
}
```

### 4.2 Order Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AlgoTrading.Domain.TradingContext;

namespace AlgoTrading.Infrastructure.Persistence.Configurations.Trading
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", "trading");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    value => new OrderId(value))
                .HasColumnName("id");

            builder.Property(o => o.StockCode)
                .HasConversion(
                    sc => sc.Value,
                    value => new StockCode(value))
                .HasColumnName("stock_code")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.OrderType)
                .HasConversion<string>()
                .HasColumnName("order_type")
                .IsRequired();

            builder.Property(o => o.OrderSide)
                .HasConversion<string>()
                .HasColumnName("order_side")
                .IsRequired();

            builder.Property(o => o.OrderStatus)
                .HasConversion<string>()
                .HasColumnName("order_status")
                .IsRequired();

            builder.Property(o => o.OrderPrice)
                .HasConversion(
                    price => price.Value,
                    value => new OrderPrice(value))
                .HasColumnName("order_price")
                .HasPrecision(18, 2);

            builder.Property(o => o.OrderQuantity)
                .HasConversion(
                    qty => qty.Value,
                    value => new OrderQuantity(value))
                .HasColumnName("order_quantity")
                .IsRequired();

            // 관계 설정
            builder.HasMany(o => o.Executions)
                .WithOne()
                .HasForeignKey("order_id")
                .OnDelete(DeleteBehavior.Cascade);

            // 인덱스
            builder.HasIndex(o => new { o.AccountId, o.OrderStatus })
                .HasDatabaseName("idx_orders_account_status");

            builder.HasIndex(o => o.StockCode)
                .HasDatabaseName("idx_orders_stock");
        }
    }
}
```

### 4.3 Value Objects 변환

```csharp
// Money Value Object 변환
public class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter() 
        : base(
            money => money.Amount,
            value => new Money(value, Currency.KRW))
    {
    }
}

// StockCode Value Object 변환
public class StockCodeConverter : ValueConverter<StockCode, string>
{
    public StockCodeConverter() 
        : base(
            code => code.Value,
            value => new StockCode(value))
    {
    }
}

// 사용 예시
builder.Property(p => p.CurrentPrice)
    .HasConversion(new MoneyConverter())
    .HasColumnName("current_price")
    .HasPrecision(18, 2);
```

---

## 5. Redis 설정

### 5.1 Redis Service

```csharp
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AlgoTrading.Infrastructure.Caching
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        ISubscriber GetSubscriber();
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ISubscriber _subscriber;

        public RedisCacheService(IConnectionMultiplexer redis, int database = 0)
        {
            _redis = redis;
            _database = _redis.GetDatabase(database);
            _subscriber = _redis.GetSubscriber();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public ISubscriber GetSubscriber() => _subscriber;
    }
}
```

### 5.2 Distributed Cache 구현

```csharp
using Microsoft.Extensions.Caching.Distributed;

namespace AlgoTrading.Infrastructure.Caching
{
    public class CachedRepository<T> where T : class
    {
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(5);

        public CachedRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetOrAddAsync(
            string key, 
            Func<Task<T>> factory, 
            TimeSpan? expiry = null)
        {
            var cachedValue = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<T>(cachedValue);
            }

            var value = await factory();

            if (value != null)
            {
                var json = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry
                };
                await _cache.SetStringAsync(key, json, options);
            }

            return value;
        }

        public async Task InvalidateAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
```

---

## 6. 의존성 주입 설정

### 6.1 Startup.cs / Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using AlgoTrading.Infrastructure.Persistence.Contexts;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 1. PostgreSQL + EF Core 설정
builder.Services.AddDbContext<AccountDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("AlgoTradingDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "account");
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddDbContext<TradingDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("AlgoTradingDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "trading");
        });
});

builder.Services.AddDbContext<StrategyDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("AlgoTradingDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "strategy");
        });
});

builder.Services.AddDbContext<MarketDataDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("AlgoTradingDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "market");
        });
});

// 2. Redis 설정
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis")!);
    
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "AlgoTrading_";
});

// Redis Service 등록
builder.Services.AddScoped<IRedisCacheService>(sp =>
{
    var redis = sp.GetRequiredService<IConnectionMultiplexer>();
    return new RedisCacheService(redis, database: 0);
});

// 3. Repository 등록
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();

// 4. Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// 데이터베이스 마이그레이션 (선택사항)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var accountDb = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    await accountDb.Database.MigrateAsync();
}

app.Run();
```

---

## 7. 마이그레이션 가이드

### 7.1 초기 마이그레이션 생성

```bash
# Account Context
dotnet ef migrations add InitialCreate --context AccountDbContext --output-dir Persistence/Migrations/Account

# Trading Context
dotnet ef migrations add InitialCreate --context TradingDbContext --output-dir Persistence/Migrations/Trading

# Strategy Context
dotnet ef migrations add InitialCreate --context StrategyDbContext --output-dir Persistence/Migrations/Strategy

# Market Data Context
dotnet ef migrations add InitialCreate --context MarketDataDbContext --output-dir Persistence/Migrations/Market
```

### 7.2 데이터베이스 업데이트

```bash
# 모든 Context 업데이트
dotnet ef database update --context AccountDbContext
dotnet ef database update --context TradingDbContext
dotnet ef database update --context StrategyDbContext
dotnet ef database update --context MarketDataDbContext
```

### 7.3 스크립트 생성 (프로덕션 배포용)

```bash
# SQL 스크립트 생성
dotnet ef migrations script --context AccountDbContext --output account_migration.sql

# 특정 마이그레이션 범위
dotnet ef migrations script 20240101000000_InitialCreate 20240102000000_AddIndices --context AccountDbContext
```

---

## 8. 사용 예시

### 8.1 Repository 구현

```csharp
using AlgoTrading.Domain.AccountContext;
using AlgoTrading.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AlgoTrading.Infrastructure.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDbContext _context;
        private readonly IRedisCacheService _cache;

        public AccountRepository(
            AccountDbContext context,
            IRedisCacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<TradingAccount?> GetByIdAsync(AccountId id)
        {
            var cacheKey = $"account:{id}";

            // 캐시 조회
            var cached = await _cache.GetAsync<TradingAccount>(cacheKey);
            if (cached != null)
                return cached;

            // DB 조회
            var account = await _context.TradingAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            // 캐시 저장
            if (account != null)
            {
                await _cache.SetAsync(cacheKey, account, TimeSpan.FromMinutes(5));
            }

            return account;
        }

        public async Task AddAsync(TradingAccount account)
        {
            _context.TradingAccounts.Add(account);
            await _context.SaveChangesAsync();

            // 캐시 무효화
            await _cache.RemoveAsync($"account:{account.Id}");
        }

        public async Task UpdateAsync(TradingAccount account)
        {
            _context.TradingAccounts.Update(account);
            await _context.SaveChangesAsync();

            // 캐시 무효화
            await _cache.RemoveAsync($"account:{account.Id}");
        }
    }
}
```

### 8.2 Dapper를 이용한 복잡한 쿼리

```csharp
using Dapper;
using Npgsql;

public class PerformanceQueryService
{
    private readonly string _connectionString;

    public PerformanceQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AlgoTradingDb")!;
    }

    public async Task<IEnumerable<AccountPerformance>> GetTopPerformingAccountsAsync(int count)
    {
        const string sql = @"
            SELECT 
                a.id,
                a.account_name,
                a.total_pl_percent,
                COUNT(p.id) as open_positions_count
            FROM account.trading_accounts a
            LEFT JOIN trading.positions p ON a.id = p.account_id AND p.closed_at IS NULL
            WHERE a.account_status = 'ACTIVE'
            GROUP BY a.id, a.account_name, a.total_pl_percent
            ORDER BY a.total_pl_percent DESC
            LIMIT @Count";

        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<AccountPerformance>(sql, new { Count = count });
    }
}
```

---

## 9. 주의사항

### TimescaleDB Hypertable 처리
- EF Core는 Hypertable을 일반 테이블로 취급합니다
- Hypertable 생성은 SQL 스크립트로 별도 실행해야 합니다
- Migration 후 별도로 `SELECT create_hypertable(...)` 실행

### Enum 변환
- PostgreSQL의 ENUM 타입은 EF Core에서 string 변환 사용 권장
- `.HasConversion<string>()` 사용

### JSONB 필드
```csharp
builder.Property(e => e.Metadata)
    .HasColumnType("jsonb")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null!)!
    );
```

---

## 10. 성능 최적화 팁

1. **AsNoTracking 사용**: 읽기 전용 쿼리에는 항상 `AsNoTracking()` 사용
2. **Include 최적화**: 필요한 관계만 Include
3. **Compiled Queries**: 자주 사용되는 쿼리는 컴파일
4. **Connection Pooling**: 자동으로 활성화되지만 설정 조정 가능
5. **Batch Operations**: EF Core Plus 사용 고려

---

이 가이드를 따라 Entity Framework Core를 설정하면 PostgreSQL 17 + TimescaleDB + Redis와 원활하게 연동할 수 있습니다.
