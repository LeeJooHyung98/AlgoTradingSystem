using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlgoTrading.Desktop.Services;

/// <summary>
/// HTTP client for AlgoTrading API
/// </summary>
public class AlgoTradingApiClient : IAlgoTradingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AlgoTradingApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AlgoTradingApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AlgoTradingApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure base URL from appsettings
        var baseUrl = configuration["AlgoTradingApi:BaseUrl"] ?? "https://localhost:7001";
        _httpClient.BaseAddress = new Uri(baseUrl);

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _logger.LogInformation("AlgoTradingApiClient initialized with base URL: {BaseUrl}", baseUrl);
    }

    #region Strategy Endpoints

    public async Task<List<StrategyDto>> GetAvailableStrategiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET /api/strategy/available");

            var response = await _httpClient.GetAsync("/api/strategy/available", cancellationToken);
            response.EnsureSuccessStatusCode();

            var strategies = await response.Content.ReadFromJsonAsync<List<StrategyDto>>(_jsonOptions, cancellationToken);
            return strategies ?? new List<StrategyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available strategies");
            throw;
        }
    }

    public async Task<StrategyDto> GetStrategyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET /api/strategy/{Id}", id);

            var response = await _httpClient.GetAsync($"/api/strategy/{id}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var strategy = await response.Content.ReadFromJsonAsync<StrategyDto>(_jsonOptions, cancellationToken);
            return strategy ?? throw new InvalidOperationException("Strategy not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get strategy {Id}", id);
            throw;
        }
    }

    public async Task<StrategyDto> CreateStrategyAsync(CreateStrategyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/strategy");

            var response = await _httpClient.PostAsJsonAsync("/api/strategy", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var strategy = await response.Content.ReadFromJsonAsync<StrategyDto>(_jsonOptions, cancellationToken);
            return strategy ?? throw new InvalidOperationException("Failed to create strategy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create strategy");
            throw;
        }
    }

    public async Task<StrategyDto> ActivateStrategyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/strategy/{Id}/activate", id);

            var response = await _httpClient.PostAsync($"/api/strategy/{id}/activate", null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var strategy = await response.Content.ReadFromJsonAsync<StrategyDto>(_jsonOptions, cancellationToken);
            return strategy ?? throw new InvalidOperationException("Failed to activate strategy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate strategy {Id}", id);
            throw;
        }
    }

    public async Task<StrategyDto> DeactivateStrategyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/strategy/{Id}/deactivate", id);

            var response = await _httpClient.PostAsync($"/api/strategy/{id}/deactivate", null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var strategy = await response.Content.ReadFromJsonAsync<StrategyDto>(_jsonOptions, cancellationToken);
            return strategy ?? throw new InvalidOperationException("Failed to deactivate strategy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate strategy {Id}", id);
            throw;
        }
    }

    public async Task<SignalDto> GenerateSignalAsync(Guid strategyId, string stockCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/strategy/{StrategyId}/signal", strategyId);

            var request = new { StockCode = stockCode };
            var response = await _httpClient.PostAsJsonAsync($"/api/strategy/{strategyId}/signal", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var signal = await response.Content.ReadFromJsonAsync<SignalDto>(_jsonOptions, cancellationToken);
            return signal ?? throw new InvalidOperationException("Failed to generate signal");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate signal for strategy {StrategyId}, stock {StockCode}", strategyId, stockCode);
            throw;
        }
    }

    #endregion

    #region Account Endpoints

    public async Task<AccountSummaryDto> GetAccountSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET /api/account/summary");

            // TODO: Implement when account endpoint is available
            // For now, return mock data
            return await Task.FromResult(new AccountSummaryDto
            {
                TotalAssets = 10000000m,
                Cash = 5000000m,
                StockValue = 5000000m,
                TotalPnL = 500000m,
                TodayPnL = 50000m,
                DayPnLPercentage = 1.5m
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account summary");
            throw;
        }
    }

    #endregion

    #region Portfolio Endpoints

    public async Task<List<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET /api/portfolio/positions");

            // TODO: Implement when portfolio endpoint is available
            // For now, return mock data
            return await Task.FromResult(new List<PositionDto>
            {
                new PositionDto
                {
                    StockCode = "005930",
                    StockName = "삼성전자",
                    Quantity = 100,
                    AveragePrice = 70000m,
                    CurrentPrice = 72000m,
                    ProfitLoss = 200000m,
                    ProfitLossPercentage = 2.86m
                },
                new PositionDto
                {
                    StockCode = "000660",
                    StockName = "SK하이닉스",
                    Quantity = 50,
                    AveragePrice = 120000m,
                    CurrentPrice = 125000m,
                    ProfitLoss = 250000m,
                    ProfitLossPercentage = 4.17m
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get positions");
            throw;
        }
    }

    #endregion

    #region Order Endpoints

    public async Task<List<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET /api/order");

            // TODO: Implement when order endpoint is available
            // For now, return mock data
            return await Task.FromResult(new List<OrderDto>
            {
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    StockCode = "005930",
                    Type = "Limit",
                    Side = "Buy",
                    Quantity = 10,
                    FilledQuantity = 10,
                    RemainingQuantity = 0,
                    LimitPrice = 71000m,
                    AverageFillPrice = 70900m,
                    Status = "Filled",
                    AccountNumber = "1234567890",
                    Source = "Manual",
                    SubmittedAt = DateTime.Now.AddMinutes(-30)
                },
                new OrderDto
                {
                    Id = Guid.NewGuid(),
                    StockCode = "000660",
                    Type = "Market",
                    Side = "Sell",
                    Quantity = 5,
                    FilledQuantity = 0,
                    RemainingQuantity = 5,
                    Status = "Pending",
                    AccountNumber = "1234567890",
                    Source = "Manual",
                    SubmittedAt = DateTime.Now.AddMinutes(-5)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get orders");
            throw;
        }
    }

    public async Task<List<OrderDto>> GetOrdersByAccountAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("GET /api/order/account/{AccountNumber}", accountNumber);

            var response = await _httpClient.GetAsync($"/api/order/account/{accountNumber}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(_jsonOptions, cancellationToken);
            return orders ?? new List<OrderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get orders for account {AccountNumber}", accountNumber);
            throw;
        }
    }

    public async Task<OrderDto> CreateMarketOrderAsync(CreateMarketOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/order/market");

            var response = await _httpClient.PostAsJsonAsync("/api/order/market", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions, cancellationToken);
            return order ?? throw new InvalidOperationException("Failed to create market order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create market order");
            throw;
        }
    }

    public async Task<OrderDto> CreateLimitOrderAsync(CreateLimitOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/order/limit");

            var response = await _httpClient.PostAsJsonAsync("/api/order/limit", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions, cancellationToken);
            return order ?? throw new InvalidOperationException("Failed to create limit order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create limit order");
            throw;
        }
    }

    public async Task<OrderDto> CancelOrderAsync(Guid orderId, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("POST /api/order/{OrderId}/cancel", orderId);

            var request = new { Reason = reason };
            var response = await _httpClient.PostAsJsonAsync($"/api/order/{orderId}/cancel", request, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions, cancellationToken);
            return order ?? throw new InvalidOperationException("Failed to cancel order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order {OrderId}", orderId);
            throw;
        }
    }

    #endregion
}
