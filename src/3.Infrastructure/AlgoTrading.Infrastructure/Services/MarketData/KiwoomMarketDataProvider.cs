using AlgoTrading.Application.Services.MarketData;
using AlgoTrading.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AlgoTrading.Infrastructure.Services.MarketData;

/// <summary>
/// description(설명) : Kiwoom Market Data Provider (키움증권 시장 데이터 제공자)
/// Details(상세설명) : Provides real-time and historical market data from Kiwoom OpenAPI
/// Applied technology patterns(적용기술패턴) : Provider Pattern, Adapter Pattern, COM Interop
///
/// IMPORTANT: This requires Kiwoom OpenAPI to be installed and registered
/// Install from: https://www.kiwoom.com/h/customer/download/VOpenApiInfoView
/// </summary>
public class KiwoomMarketDataProvider : IMarketDataProvider, IDisposable
{
    private readonly ILogger<KiwoomMarketDataProvider> _logger;
    private bool _isConnected = false;
    private bool _disposed = false;

    // COM object for Kiwoom OpenAPI
    // Type: KHOPENAPI.KHOpenAPICtrl.1
    private dynamic? _kiwoomApi;

    // Event synchronization
    private readonly AutoResetEvent _loginEvent = new AutoResetEvent(false);
    private readonly ConcurrentDictionary<string, AutoResetEvent> _trEvents = new();
    private readonly ConcurrentDictionary<string, List<HistoricalPriceData>> _trData = new();

    // Screen number management (Kiwoom requires unique screen numbers)
    private int _screenNumberCounter = 1000;

    public KiwoomMarketDataProvider(ILogger<KiwoomMarketDataProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Connect to Kiwoom OpenAPI using COM Interop
    /// </summary>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to connect to Kiwoom OpenAPI...");

            // Check if running on Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogError("Kiwoom OpenAPI only works on Windows platform");
                return false;
            }

            try
            {
                // Initialize COM object
                // IMPORTANT: This requires Kiwoom OpenAPI to be installed
                Type? kiwoomType = Type.GetTypeFromProgID("KHOPENAPI.KHOpenAPICtrl.1");

                if (kiwoomType == null)
                {
                    _logger.LogError("Kiwoom OpenAPI COM object not found. Please install Kiwoom OpenAPI.");
                    _logger.LogWarning("Download from: https://www.kiwoom.com/h/customer/download/VOpenApiInfoView");
                    return false;
                }

                _kiwoomApi = Activator.CreateInstance(kiwoomType);

                // Setup event handlers for real-time data
                SetupEventHandlers();

                // Request login
                int loginResult = _kiwoomApi.CommConnect();

                if (loginResult == 0)
                {
                    // Wait for login event (timeout 30 seconds)
                    bool loginSuccess = await Task.Run(() =>
                        _loginEvent.WaitOne(TimeSpan.FromSeconds(30)), cancellationToken);

                    if (loginSuccess)
                    {
                        _isConnected = true;

                        // Get account information
                        string accountCount = _kiwoomApi.GetLoginInfo("ACCOUNT_CNT");
                        string accounts = _kiwoomApi.GetLoginInfo("ACCNO");
                        string userId = _kiwoomApi.GetLoginInfo("USER_ID");
                        string userName = _kiwoomApi.GetLoginInfo("USER_NAME");

                        _logger.LogInformation(
                            "Successfully connected to Kiwoom OpenAPI. User: {UserName} ({UserId}), Accounts: {Accounts}",
                            userName, userId, accounts);

                        return true;
                    }
                    else
                    {
                        _logger.LogError("Kiwoom login timeout");
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Kiwoom CommConnect failed with code: {Code}", loginResult);
                    return false;
                }
            }
            catch (COMException comEx)
            {
                _logger.LogError(comEx, "COM Exception while connecting to Kiwoom API");
                _logger.LogWarning("Make sure Kiwoom OpenAPI is properly installed and registered");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to Kiwoom OpenAPI");
            return false;
        }
    }

    /// <summary>
    /// Setup event handlers for Kiwoom API callbacks
    /// </summary>
    private void SetupEventHandlers()
    {
        if (_kiwoomApi == null) return;

        try
        {
            // OnEventConnect: Login result
            _kiwoomApi.OnEventConnect += new Action<int>((errCode) =>
            {
                if (errCode == 0)
                {
                    _logger.LogInformation("Kiwoom login successful");
                }
                else
                {
                    _logger.LogError("Kiwoom login failed with error code: {ErrorCode}", errCode);
                }
                _loginEvent.Set();
            });

            // OnReceiveTrData: Transaction data received
            _kiwoomApi.OnReceiveTrData += new Action<string, string, string, string, string, int, string, string, string>(
                OnReceiveTrData);

            // OnReceiveRealData: Real-time data received
            _kiwoomApi.OnReceiveRealData += new Action<string, string, string>(
                OnReceiveRealData);

            // OnReceiveMsg: Message received
            _kiwoomApi.OnReceiveMsg += new Action<string, string, string, string>(
                (scrNo, rqName, trCode, msg) =>
                {
                    _logger.LogDebug("Kiwoom Message - Screen: {Screen}, Request: {Request}, Code: {Code}, Message: {Message}",
                        scrNo, rqName, trCode, msg);
                });

            _logger.LogDebug("Kiwoom event handlers setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up Kiwoom event handlers");
        }
    }

    /// <summary>
    /// TR Data received event handler
    /// </summary>
    private void OnReceiveTrData(
        string scrNo, string rqName, string trCode,
        string recordName, string prevNext,
        int dataLength, string errorCode,
        string message, string splmMsg)
    {
        try
        {
            _logger.LogDebug("TR Data received - Request: {Request}, Code: {Code}, Error: {Error}",
                rqName, trCode, errorCode);

            // Check for errors
            if (!string.IsNullOrEmpty(errorCode) && errorCode != "0")
            {
                _logger.LogWarning("TR request error - Code: {ErrorCode}, Message: {Message}",
                    errorCode, message);
            }

            if (trCode == "opt10081") // Daily chart data
            {
                var priceDataList = ParseDailyChartData(scrNo, rqName);
                _trData[rqName] = priceDataList;
            }
            else if (trCode == "opt10001") // Current price data
            {
                // Data will be retrieved directly in GetCurrentPriceAsync
                // Just signal completion
            }

            // Signal completion
            if (_trEvents.TryGetValue(rqName, out var eventHandle))
            {
                eventHandle.Set();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TR data for {Request}", rqName);

            // Signal completion even on error to prevent hanging
            if (_trEvents.TryGetValue(rqName, out var eventHandle))
            {
                eventHandle.Set();
            }
        }
    }

    /// <summary>
    /// Real-time data received event handler
    /// </summary>
    private void OnReceiveRealData(string stockCode, string realType, string realData)
    {
        try
        {
            _logger.LogTrace("Real-time data received - Stock: {Stock}, Type: {Type}", stockCode, realType);

            if (_kiwoomApi == null) return;

            // Parse different realType values
            switch (realType)
            {
                case "주식체결": // Stock execution/trade
                    ParseStockExecution(stockCode);
                    break;

                case "주식호가": // Stock order book/bid-ask
                    ParseStockOrderBook(stockCode);
                    break;

                case "주식시세": // Stock quote/market data
                    ParseStockQuote(stockCode);
                    break;

                default:
                    _logger.LogDebug("Unhandled real-time type: {Type} for {Stock}", realType, stockCode);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing real-time data for {Stock}", stockCode);
        }
    }

    /// <summary>
    /// Parse stock execution (체결) real-time data
    /// FID: 10(현재가), 11(전일대비), 12(등락률), 27(거래량), 28(거래대금)
    /// </summary>
    private void ParseStockExecution(string stockCode)
    {
        if (_kiwoomApi == null) return;

        try
        {
            string currentPriceStr = _kiwoomApi.GetCommRealData(stockCode, 10); // 현재가
            string volumeStr = _kiwoomApi.GetCommRealData(stockCode, 27); // 거래량

            if (decimal.TryParse(currentPriceStr, out decimal currentPrice) &&
                long.TryParse(volumeStr, out long volume))
            {
                _logger.LogTrace("Stock execution - Code: {Code}, Price: {Price}, Volume: {Volume}",
                    stockCode, Math.Abs(currentPrice), volume);

                // TODO: Trigger real-time data callback for subscribers
                // RaiseRealtimePriceUpdated(stockCode, Math.Abs(currentPrice), volume);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing stock execution for {Stock}", stockCode);
        }
    }

    /// <summary>
    /// Parse stock order book (호가) real-time data
    /// FID: 41-50(매도호가), 51-60(매수호가), 61-70(매도잔량), 71-80(매수잔량)
    /// </summary>
    private void ParseStockOrderBook(string stockCode)
    {
        if (_kiwoomApi == null) return;

        try
        {
            // Best bid/ask
            string bestAskStr = _kiwoomApi.GetCommRealData(stockCode, 41); // 매도호가1
            string bestBidStr = _kiwoomApi.GetCommRealData(stockCode, 51); // 매수호가1

            if (decimal.TryParse(bestAskStr, out decimal bestAsk) &&
                decimal.TryParse(bestBidStr, out decimal bestBid))
            {
                _logger.LogTrace("Stock order book - Code: {Code}, BestBid: {Bid}, BestAsk: {Ask}",
                    stockCode, Math.Abs(bestBid), Math.Abs(bestAsk));

                // TODO: Trigger order book callback for subscribers
                // RaiseOrderBookUpdated(stockCode, bestBid, bestAsk);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing stock order book for {Stock}", stockCode);
        }
    }

    /// <summary>
    /// Parse stock quote (시세) real-time data
    /// </summary>
    private void ParseStockQuote(string stockCode)
    {
        if (_kiwoomApi == null) return;

        try
        {
            string currentPriceStr = _kiwoomApi.GetCommRealData(stockCode, 10); // 현재가
            string openStr = _kiwoomApi.GetCommRealData(stockCode, 16); // 시가
            string highStr = _kiwoomApi.GetCommRealData(stockCode, 17); // 고가
            string lowStr = _kiwoomApi.GetCommRealData(stockCode, 18); // 저가

            if (decimal.TryParse(currentPriceStr, out decimal currentPrice))
            {
                _logger.LogTrace("Stock quote - Code: {Code}, Price: {Price}",
                    stockCode, Math.Abs(currentPrice));

                // TODO: Trigger quote callback for subscribers
                // RaiseQuoteUpdated(stockCode, currentPrice, open, high, low);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing stock quote for {Stock}", stockCode);
        }
    }

    /// <summary>
    /// Parse daily chart data from Kiwoom response
    /// </summary>
    private List<HistoricalPriceData> ParseDailyChartData(string scrNo, string rqName)
    {
        var result = new List<HistoricalPriceData>();

        if (_kiwoomApi == null) return result;

        try
        {
            int dataCount = _kiwoomApi.GetRepeatCnt(scrNo, rqName);

            for (int i = 0; i < dataCount; i++)
            {
                string dateStr = _kiwoomApi.GetCommData(scrNo, rqName, i, "일자").Trim();
                string openStr = _kiwoomApi.GetCommData(scrNo, rqName, i, "시가").Trim();
                string highStr = _kiwoomApi.GetCommData(scrNo, rqName, i, "고가").Trim();
                string lowStr = _kiwoomApi.GetCommData(scrNo, rqName, i, "저가").Trim();
                string closeStr = _kiwoomApi.GetCommData(scrNo, rqName, i, "현재가").Trim();
                string volumeStr = _kiwoomApi.GetCommData(scrNo, rqName, i, "거래량").Trim();

                if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out DateTime date) &&
                    decimal.TryParse(openStr, out decimal open) &&
                    decimal.TryParse(highStr, out decimal high) &&
                    decimal.TryParse(lowStr, out decimal low) &&
                    decimal.TryParse(closeStr, out decimal close) &&
                    long.TryParse(volumeStr, out long volume))
                {
                    result.Add(new HistoricalPriceData
                    {
                        Date = date,
                        Open = Math.Abs(open),
                        High = Math.Abs(high),
                        Low = Math.Abs(low),
                        Close = Math.Abs(close),
                        Volume = volume,
                        AdjustedClose = Math.Abs(close)
                    });
                }
            }

            _logger.LogInformation("Parsed {Count} daily chart data points", result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing daily chart data");
        }

        return result;
    }

    /// <summary>
    /// Get historical price data for a stock from Kiwoom API
    /// TR: opt10081 (주식일봉차트조회)
    /// </summary>
    public async Task<IEnumerable<HistoricalPriceData>> GetHistoricalPricesAsync(
        string stockCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_isConnected || _kiwoomApi == null)
            {
                _logger.LogWarning("Not connected to Kiwoom API. Attempting to connect...");
                bool connected = await ConnectAsync(cancellationToken);
                if (!connected)
                {
                    return Array.Empty<HistoricalPriceData>();
                }
            }

            _logger.LogInformation(
                "Fetching historical prices for {StockCode} from {StartDate} to {EndDate}",
                stockCode, startDate, endDate);

            string screenNo = GetNextScreenNumber();
            string requestName = $"DailyChart_{stockCode}_{DateTime.Now:HHmmssfff}";

            // Setup event for this request
            var requestEvent = new AutoResetEvent(false);
            _trEvents[requestName] = requestEvent;

            try
            {
                // Set input values for TR
                _kiwoomApi.SetInputValue("종목코드", stockCode);
                _kiwoomApi.SetInputValue("기준일자", endDate.ToString("yyyyMMdd"));
                _kiwoomApi.SetInputValue("수정주가구분", "1"); // Adjusted price

                // Request data
                int requestResult = _kiwoomApi.CommRqData(requestName, "opt10081", 0, screenNo);

                if (requestResult == 0)
                {
                    // Wait for response (timeout 30 seconds)
                    bool received = await Task.Run(() =>
                        requestEvent.WaitOne(TimeSpan.FromSeconds(30)), cancellationToken);

                    if (received && _trData.TryGetValue(requestName, out var data))
                    {
                        // Filter by date range
                        var filteredData = data
                            .Where(d => d.Date >= startDate && d.Date <= endDate)
                            .OrderBy(d => d.Date)
                            .ToList();

                        // Set stock code
                        foreach (var item in filteredData)
                        {
                            item.StockCode = stockCode;
                        }

                        _logger.LogInformation(
                            "Retrieved {Count} historical price data points for {StockCode}",
                            filteredData.Count, stockCode);

                        return filteredData;
                    }
                    else
                    {
                        _logger.LogWarning("Timeout waiting for historical data for {StockCode}", stockCode);
                        return Array.Empty<HistoricalPriceData>();
                    }
                }
                else
                {
                    _logger.LogError("Failed to request historical data for {StockCode}, result: {Result}",
                        stockCode, requestResult);
                    return Array.Empty<HistoricalPriceData>();
                }
            }
            finally
            {
                // Cleanup
                _trEvents.TryRemove(requestName, out _);
                _trData.TryRemove(requestName, out _);
                requestEvent.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical prices for {StockCode}", stockCode);
            throw;
        }
    }

    /// <summary>
    /// Get historical prices for multiple stocks
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<HistoricalPriceData>>> GetHistoricalPricesAsync(
        IEnumerable<string> stockCodes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, IEnumerable<HistoricalPriceData>>();
        var stockCodesList = stockCodes.ToList();

        _logger.LogInformation("Fetching historical prices for {Count} stocks", stockCodesList.Count);

        foreach (var stockCode in stockCodesList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var data = await GetHistoricalPricesAsync(stockCode, startDate, endDate, cancellationToken);
            result[stockCode] = data;

            // Kiwoom API rate limiting: 200ms between requests (5 req/sec)
            await Task.Delay(200, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Get current real-time price for a stock
    /// TR: opt10001 (주식기본정보)
    /// </summary>
    public async Task<decimal?> GetCurrentPriceAsync(
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_isConnected || _kiwoomApi == null)
            {
                _logger.LogWarning("Not connected to Kiwoom API");
                return null;
            }

            string screenNo = GetNextScreenNumber();
            string requestName = $"CurrentPrice_{stockCode}_{DateTime.Now:HHmmssfff}";

            var requestEvent = new AutoResetEvent(false);
            _trEvents[requestName] = requestEvent;

            try
            {
                _kiwoomApi.SetInputValue("종목코드", stockCode);
                int requestResult = _kiwoomApi.CommRqData(requestName, "opt10001", 0, screenNo);

                if (requestResult == 0)
                {
                    bool received = await Task.Run(() =>
                        requestEvent.WaitOne(TimeSpan.FromSeconds(10)), cancellationToken);

                    if (received)
                    {
                        string priceStr = _kiwoomApi.GetCommData(screenNo, requestName, 0, "현재가").Trim();
                        if (decimal.TryParse(priceStr, out decimal price))
                        {
                            return Math.Abs(price);
                        }
                    }
                }

                return null;
            }
            finally
            {
                _trEvents.TryRemove(requestName, out _);
                requestEvent.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current price for {StockCode}", stockCode);
            return null;
        }
    }

    /// <summary>
    /// Get current prices for multiple stocks
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetCurrentPricesAsync(
        IEnumerable<string> stockCodes,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, decimal>();
        var stockCodesList = stockCodes.ToList();

        foreach (var stockCode in stockCodesList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var price = await GetCurrentPriceAsync(stockCode, cancellationToken);
            if (price.HasValue)
            {
                result[stockCode] = price.Value;
            }

            await Task.Delay(200, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Get next available screen number (Kiwoom requires unique screen numbers)
    /// </summary>
    private string GetNextScreenNumber()
    {
        int screenNo = Interlocked.Increment(ref _screenNumberCounter);
        if (screenNo > 9999)
        {
            Interlocked.Exchange(ref _screenNumberCounter, 1000);
            screenNo = 1000;
        }
        return screenNo.ToString("D4");
    }

    /// <summary>
    /// Disconnect from Kiwoom OpenAPI
    /// </summary>
    public void Disconnect()
    {
        try
        {
            if (_isConnected && _kiwoomApi != null)
            {
                _logger.LogInformation("Disconnecting from Kiwoom OpenAPI...");
                _kiwoomApi.CommTerminate();
                _isConnected = false;
                _logger.LogInformation("Disconnected from Kiwoom OpenAPI");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from Kiwoom OpenAPI");
        }
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Disconnect();

            if (_kiwoomApi != null)
            {
                try
                {
                    Marshal.ReleaseComObject(_kiwoomApi);
                }
                catch { }
                _kiwoomApi = null;
            }

            _loginEvent?.Dispose();

            foreach (var evt in _trEvents.Values)
            {
                evt?.Dispose();
            }
            _trEvents.Clear();

            _disposed = true;
        }
    }
}
