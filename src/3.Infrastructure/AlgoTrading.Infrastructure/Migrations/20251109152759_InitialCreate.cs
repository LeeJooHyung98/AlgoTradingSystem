using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoTrading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "account");

            migrationBuilder.EnsureSchema(
                name: "monitoring");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "market");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "account_balance_history",
                schema: "account",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance_date = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    cash_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CashBalance_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    total_assets = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAssets_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    total_evaluation = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalEvaluation_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    total_pl = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalProfitLoss_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_balance_history", x => new { x.account_id, x.balance_date });
                });

            migrationBuilder.CreateTable(
                name: "account_restrictions",
                schema: "account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restriction_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    restriction_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    applied_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    applied_by = table.Column<Guid>(type: "uuid", nullable: false),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_restrictions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_withdrawals",
                schema: "account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    bank_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    bank_account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_holder_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    withdrawal_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    approved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_withdrawals", x => x.id);
                    table.CheckConstraint("chk_withdrawal_amount", "amount > 0");
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                schema: "monitoring",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alert_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    action_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => new { x.log_timestamp, x.id });
                });

            migrationBuilder.CreateTable(
                name: "market_data",
                schema: "market",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    open_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OpenPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    high_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HighPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    low_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LowPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    close_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ClosePrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    volume = table.Column<long>(type: "bigint", nullable: false),
                    trade_count = table.Column<int>(type: "integer", nullable: true),
                    change_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ChangeAmount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    change_percent = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    market_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_data", x => new { x.stock_code, x.data_timestamp });
                });

            migrationBuilder.CreateTable(
                name: "order_book",
                schema: "market",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    captured_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ask_price_1 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice1_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_1 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_2 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice2_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_2 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_3 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice3_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_3 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_4 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice4_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_4 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_5 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice5_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_5 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_6 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice6_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_6 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_7 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice7_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_7 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_8 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice8_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_8 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_9 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice9_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_9 = table.Column<long>(type: "bigint", nullable: true),
                    ask_price_10 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AskPrice10_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ask_volume_10 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_1 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice1_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_1 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_2 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice2_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_2 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_3 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice3_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_3 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_4 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice4_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_4 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_5 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice5_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_5 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_6 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice6_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_6 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_7 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice7_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_7 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_8 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice8_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_8 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_9 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice9_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_9 = table.Column<long>(type: "bigint", nullable: true),
                    bid_price_10 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BidPrice10_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    bid_volume_10 = table.Column<long>(type: "bigint", nullable: true),
                    total_ask_volume = table.Column<long>(type: "bigint", nullable: true),
                    total_bid_volume = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_book", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "performance_records",
                schema: "account",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    record_date = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    equity_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EquityValue_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    daily_return = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DailyReturn_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    daily_return_percent = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    cumulative_return = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CumulativeReturn_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    cumulative_return_percent = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    sharpe_ratio = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    max_drawdown = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    win_rate = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_records", x => new { x.account_id, x.record_date });
                });

            migrationBuilder.CreateTable(
                name: "stock_info",
                schema: "market",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    stock_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    market_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sector = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    industry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    listing_date = table.Column<DateTime>(type: "date", nullable: true),
                    listed_shares = table.Column<long>(type: "bigint", nullable: true),
                    is_tradable = table.Column<bool>(type: "boolean", nullable: false),
                    is_suspended = table.Column<bool>(type: "boolean", nullable: false),
                    upper_limit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UpperLimitPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    lower_limit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    LowerLimitPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_info", x => x.stock_code);
                });

            migrationBuilder.CreateTable(
                name: "system_metrics",
                schema: "monitoring",
                columns: table => new
                {
                    metric_timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    cpu_usage_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    memory_usage_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    memory_used_mb = table.Column<long>(type: "bigint", nullable: true),
                    memory_total_mb = table.Column<long>(type: "bigint", nullable: true),
                    network_in_mbps = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    network_out_mbps = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    db_connections = table.Column<int>(type: "integer", nullable: true),
                    db_query_time_ms = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    api_request_count = table.Column<int>(type: "integer", nullable: true),
                    api_error_count = table.Column<int>(type: "integer", nullable: true),
                    api_avg_latency_ms = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_metrics", x => x.metric_timestamp);
                });

            migrationBuilder.CreateTable(
                name: "tick_data",
                schema: "market",
                columns: table => new
                {
                    stock_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tick_timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    execution_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExecutionPrice_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    execution_volume = table.Column<long>(type: "bigint", nullable: false),
                    execution_side = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tick_data", x => new { x.stock_code, x.tick_timestamp });
                });

            migrationBuilder.CreateTable(
                name: "trading_metrics",
                schema: "monitoring",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_timestamp = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    orders_submitted = table.Column<int>(type: "integer", nullable: false),
                    orders_filled = table.Column<int>(type: "integer", nullable: false),
                    orders_cancelled = table.Column<int>(type: "integer", nullable: false),
                    orders_rejected = table.Column<int>(type: "integer", nullable: false),
                    total_execution_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalExecutionAmount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    avg_execution_time_ms = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    open_positions = table.Column<int>(type: "integer", nullable: false),
                    closed_positions = table.Column<int>(type: "integer", nullable: false),
                    realized_pl = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RealizedPl_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    unrealized_pl = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnrealizedPl_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trading_metrics", x => new { x.account_id, x.metric_timestamp });
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    user_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    last_login_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("chk_failed_login_attempts", "failed_login_attempts >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "idx_account_balance_account",
                schema: "account",
                table: "account_balance_history",
                columns: new[] { "account_id", "balance_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_account_restrictions_account",
                schema: "account",
                table: "account_restrictions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_account_restrictions_status",
                schema: "account",
                table: "account_restrictions",
                column: "restriction_status");

            migrationBuilder.CreateIndex(
                name: "idx_withdrawals_account",
                schema: "account",
                table: "account_withdrawals",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_withdrawals_status",
                schema: "account",
                table: "account_withdrawals",
                column: "withdrawal_status");

            migrationBuilder.CreateIndex(
                name: "idx_alerts_account",
                schema: "monitoring",
                table: "alerts",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_alerts_created",
                schema: "monitoring",
                table: "alerts",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_alerts_severity",
                schema: "monitoring",
                table: "alerts",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "idx_alerts_status",
                schema: "monitoring",
                table: "alerts",
                column: "alert_status");

            migrationBuilder.CreateIndex(
                name: "idx_audit_entity",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "idx_audit_user",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "user_id", "log_timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_market_data_stock",
                schema: "market",
                table: "market_data",
                columns: new[] { "stock_code", "data_timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_orderbook_stock_time",
                schema: "market",
                table: "order_book",
                columns: new[] { "stock_code", "captured_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_performance_account",
                schema: "account",
                table: "performance_records",
                columns: new[] { "account_id", "record_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_stock_info_market",
                schema: "market",
                table: "stock_info",
                column: "market_type");

            migrationBuilder.CreateIndex(
                name: "idx_stock_info_sector",
                schema: "market",
                table: "stock_info",
                column: "sector");

            migrationBuilder.CreateIndex(
                name: "idx_tick_stock_time",
                schema: "market",
                table: "tick_data",
                columns: new[] { "stock_code", "tick_timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_trading_metrics_account",
                schema: "monitoring",
                table: "trading_metrics",
                columns: new[] { "account_id", "metric_timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                schema: "public",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_status",
                schema: "public",
                table: "users",
                column: "user_status");

            migrationBuilder.CreateIndex(
                name: "idx_users_username",
                schema: "public",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_balance_history",
                schema: "account");

            migrationBuilder.DropTable(
                name: "account_restrictions",
                schema: "account");

            migrationBuilder.DropTable(
                name: "account_withdrawals",
                schema: "account");

            migrationBuilder.DropTable(
                name: "alerts",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "market_data",
                schema: "market");

            migrationBuilder.DropTable(
                name: "order_book",
                schema: "market");

            migrationBuilder.DropTable(
                name: "performance_records",
                schema: "account");

            migrationBuilder.DropTable(
                name: "stock_info",
                schema: "market");

            migrationBuilder.DropTable(
                name: "system_metrics",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "tick_data",
                schema: "market");

            migrationBuilder.DropTable(
                name: "trading_metrics",
                schema: "monitoring");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
