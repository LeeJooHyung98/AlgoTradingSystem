-- ================================================================================
-- Stock Trading Algorithm DDD System - Performance Optimization
-- PostgreSQL 17 + TimescaleDB Performance Tuning
-- ================================================================================
-- Author: Development Team
-- Date: 2025-10-27
-- Description: Index optimization, query tuning, and performance configurations
-- ================================================================================

-- ================================================================================
-- 1. 추가 인덱스 생성 (성능 최적화)
-- ================================================================================

-- Account Context 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_trading_accounts_status_owner 
    ON account.trading_accounts(account_status, owner_id);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_trading_accounts_broker 
    ON account.trading_accounts(broker_name);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_account_balance_history_date_range 
    ON account.account_balance_history(account_id, balance_date DESC);

-- 부분 인덱스: 활성 계좌만
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_active_accounts 
    ON account.trading_accounts(owner_id) 
    WHERE account_status = 'ACTIVE';

-- Trading Context 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_account_status 
    ON trading.orders(account_id, order_status);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_stock_status 
    ON trading.orders(stock_code, order_status);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_broker_id 
    ON trading.orders(broker_order_id) 
    WHERE broker_order_id IS NOT NULL;

-- 복합 인덱스: 주문 조회 최적화
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_composite 
    ON trading.orders(account_id, order_status, created_at DESC);

-- 부분 인덱스: 활성 주문만
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_active_orders 
    ON trading.orders(account_id, stock_code) 
    WHERE order_status IN ('PENDING', 'SUBMITTED', 'ACCEPTED', 'PARTIALLY_FILLED');

-- 포지션 조회 최적화
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_positions_account_open 
    ON trading.positions(account_id) 
    WHERE closed_at IS NULL;

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_positions_stock_open 
    ON trading.positions(stock_code) 
    WHERE closed_at IS NULL;

-- Strategy Context 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_strategies_owner_status 
    ON strategy.trading_strategies(owner_id, strategy_status);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_strategies_type_status 
    ON strategy.trading_strategies(strategy_type, strategy_status);

-- 부분 인덱스: 활성 전략만
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_active_strategies 
    ON strategy.trading_strategies(owner_id) 
    WHERE strategy_status = 'ACTIVE';

-- 신호 조회 최적화
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_signals_stock_generated 
    ON strategy.strategy_signals(stock_code, generated_at DESC) 
    WHERE executed = FALSE;

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_signals_strategy_recent 
    ON strategy.strategy_signals(strategy_id, generated_at DESC) 
    INCLUDE (signal_type, signal_strength);

-- Risk Management 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_violations_account_detected 
    ON risk.risk_violations(account_id, detected_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_violations_unresolved 
    ON risk.risk_violations(account_id, severity) 
    WHERE resolved_at IS NULL;

-- Market Data 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_stock_info_sector_industry 
    ON market.stock_info(sector, industry) 
    WHERE is_tradable = TRUE;

-- BRIN 인덱스: 시계열 데이터용 (공간 효율적)
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_market_data_timestamp_brin 
    ON market.market_data USING BRIN (data_timestamp);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_candle_timestamp_brin 
    ON market.candle_data USING BRIN (candle_timestamp);

-- Monitoring 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_alerts_account_status 
    ON monitoring.alerts(account_id, alert_status);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_alerts_severity_created 
    ON monitoring.alerts(severity, created_at DESC) 
    WHERE alert_status = 'ACTIVE';

-- Audit 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_audit_entity_time 
    ON audit.audit_logs(entity_type, entity_id, log_timestamp DESC);

-- ================================================================================
-- 2. 함수 기반 인덱스 (Functional Index)
-- ================================================================================

-- 날짜 기반 조회 최적화
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_created_date 
    ON trading.orders(DATE(created_at));

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_positions_opened_date 
    ON trading.positions(DATE(opened_at));

-- 대소문자 무관 검색
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_stock_info_name_lower 
    ON market.stock_info(LOWER(stock_name));

-- ================================================================================
-- 3. GIN 인덱스 (JSONB 및 배열 검색용)
-- ================================================================================

-- JSONB 필드 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_strategy_rules_condition 
    ON strategy.strategy_rules USING GIN (condition_json);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_backtest_parameters 
    ON strategy.backtest_results USING GIN (parameters);

CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_alerts_metadata 
    ON monitoring.alerts USING GIN (metadata);

-- 배열 필드 인덱스
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_strategies_target_stocks 
    ON strategy.trading_strategies USING GIN (target_stocks);

-- ================================================================================
-- 4. 통계 정보 업데이트
-- ================================================================================

-- 모든 테이블의 통계 정보 업데이트
ANALYZE account.trading_accounts;
ANALYZE account.account_balance_history;
ANALYZE account.performance_records;
ANALYZE trading.orders;
ANALYZE trading.order_executions;
ANALYZE trading.positions;
ANALYZE strategy.trading_strategies;
ANALYZE strategy.strategy_signals;
ANALYZE risk.risk_profiles;
ANALYZE risk.risk_violations;
ANALYZE market.market_data;
ANALYZE market.candle_data;
ANALYZE monitoring.system_metrics;
ANALYZE monitoring.alerts;
ANALYZE audit.audit_logs;

-- ================================================================================
-- 5. TimescaleDB 최적화
-- ================================================================================

-- 연속 집계 (Continuous Aggregates) - 일별 통계
CREATE MATERIALIZED VIEW IF NOT EXISTS market.daily_market_summary
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 day', data_timestamp) AS day,
    stock_code,
    FIRST(open_price, data_timestamp) AS open_price,
    MAX(high_price) AS high_price,
    MIN(low_price) AS low_price,
    LAST(close_price, data_timestamp) AS close_price,
    SUM(volume) AS total_volume,
    COUNT(*) AS tick_count
FROM market.market_data
GROUP BY day, stock_code;

-- 연속 집계 새로고침 정책
SELECT add_continuous_aggregate_policy('market.daily_market_summary',
    start_offset => INTERVAL '3 days',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour');

-- 계좌 일별 성과 집계
CREATE MATERIALIZED VIEW IF NOT EXISTS account.daily_account_summary
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 day', balance_date) AS day,
    account_id,
    LAST(total_assets, balance_date) AS end_total_assets,
    LAST(cash_balance, balance_date) AS end_cash_balance,
    LAST(total_pl, balance_date) AS end_total_pl
FROM account.account_balance_history
GROUP BY day, account_id;

SELECT add_continuous_aggregate_policy('account.daily_account_summary',
    start_offset => INTERVAL '7 days',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour');

-- ================================================================================
-- 6. 파티셔닝 전략 (대용량 테이블용)
-- ================================================================================

-- 주문 테이블 월별 파티셔닝 (미래 확장용)
-- 현재는 TimescaleDB Hypertable을 사용하지만, 필요시 수동 파티셔닝 가능

-- ================================================================================
-- 7. 뷰 생성 (자주 사용되는 쿼리)
-- ================================================================================

-- 활성 계좌 뷰
CREATE OR REPLACE VIEW account.v_active_accounts AS
SELECT
    id,
    account_number,
    account_name,
    account_type,
    total_assets,
    cash_balance,
    available_balance,
    total_pl_percent,
    owner_id,
    created_at,
    updated_at
FROM account.trading_accounts
WHERE account_status = 'ACTIVE';

-- 오늘의 주문 뷰
CREATE OR REPLACE VIEW trading.v_today_orders AS
SELECT
    o.id,
    o.account_id,
    o.stock_code,
    o.order_type,
    o.order_side,
    o.order_status,
    o.order_price,
    o.order_quantity,
    o.executed_quantity,
    o.created_at,
    s.stock_name
FROM trading.orders o
JOIN market.stock_info s ON o.stock_code = s.stock_code
WHERE DATE(o.created_at) = CURRENT_DATE;

-- 현재 포지션 뷰
CREATE OR REPLACE VIEW trading.v_current_positions AS
SELECT
    p.id,
    p.account_id,
    p.stock_code,
    p.quantity,
    p.average_entry_price,
    p.current_price,
    p.unrealized_pl,
    p.unrealized_pl_percent,
    p.opened_at,
    s.stock_name,
    s.market_type
FROM trading.positions p
JOIN market.stock_info s ON p.stock_code = s.stock_code
WHERE p.closed_at IS NULL;

-- 활성 전략 뷰
CREATE OR REPLACE VIEW strategy.v_active_strategies AS
SELECT
    id,
    owner_id,
    strategy_name,
    strategy_type,
    max_positions,
    total_trades,
    winning_trades,
    win_rate,
    total_profit,
    created_at,
    last_executed_at
FROM strategy.trading_strategies
WHERE strategy_status = 'ACTIVE';

-- 미해결 알림 뷰
CREATE OR REPLACE VIEW monitoring.v_unresolved_alerts AS
SELECT
    id,
    alert_type,
    severity,
    title,
    message,
    account_id,
    created_at
FROM monitoring.alerts
WHERE alert_status IN ('ACTIVE', 'ACKNOWLEDGED')
ORDER BY severity DESC, created_at DESC;

-- ================================================================================
-- 8. 성능 관련 함수
-- ================================================================================

-- 계좌 요약 정보 조회 함수 (캐시 활용)
CREATE OR REPLACE FUNCTION account.get_account_summary(p_account_id UUID)
RETURNS TABLE (
    account_id UUID,
    account_name VARCHAR,
    total_assets DECIMAL,
    cash_balance DECIMAL,
    available_balance DECIMAL,
    open_positions_count INTEGER,
    today_pl DECIMAL,
    today_pl_percent DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        a.id AS account_id,
        a.account_name,
        a.total_assets,
        a.cash_balance,
        a.available_balance,
        (SELECT COUNT(*) FROM trading.positions p 
         WHERE p.account_id = a.id AND p.closed_at IS NULL)::INTEGER AS open_positions_count,
        COALESCE(
            (SELECT daily_pl FROM risk.daily_risk_metrics 
             WHERE account_id = a.id 
             AND DATE(metric_date) = CURRENT_DATE
             ORDER BY metric_date DESC LIMIT 1), 0
        ) AS today_pl,
        COALESCE(
            (SELECT daily_pl_percent FROM risk.daily_risk_metrics 
             WHERE account_id = a.id 
             AND DATE(metric_date) = CURRENT_DATE
             ORDER BY metric_date DESC LIMIT 1), 0
        ) AS today_pl_percent
    FROM account.trading_accounts a
    WHERE a.id = p_account_id;
END;
$$ LANGUAGE plpgsql STABLE;

-- 포지션 손익 계산 함수
CREATE OR REPLACE FUNCTION trading.calculate_position_pl(
    p_position_id UUID,
    p_current_price DECIMAL
)
RETURNS TABLE (
    unrealized_pl DECIMAL,
    unrealized_pl_percent DECIMAL,
    total_value DECIMAL
) AS $$
DECLARE
    v_quantity INTEGER;
    v_avg_entry_price DECIMAL;
    v_position_side trading.position_side;
BEGIN
    SELECT quantity, average_entry_price, position_side
    INTO v_quantity, v_avg_entry_price, v_position_side
    FROM trading.positions
    WHERE id = p_position_id;
    
    IF v_position_side = 'LONG' THEN
        RETURN QUERY
        SELECT
            (p_current_price - v_avg_entry_price) * v_quantity AS unrealized_pl,
            ((p_current_price - v_avg_entry_price) / v_avg_entry_price * 100) AS unrealized_pl_percent,
            p_current_price * v_quantity AS total_value;
    ELSE
        RETURN QUERY
        SELECT
            (v_avg_entry_price - p_current_price) * v_quantity AS unrealized_pl,
            ((v_avg_entry_price - p_current_price) / v_avg_entry_price * 100) AS unrealized_pl_percent,
            v_avg_entry_price * v_quantity AS total_value;
    END IF;
END;
$$ LANGUAGE plpgsql STABLE;

-- ================================================================================
-- 9. 데이터베이스 설정 튜닝
-- ================================================================================

-- 연결 풀 설정
ALTER SYSTEM SET max_connections = 200;
ALTER SYSTEM SET shared_buffers = '4GB';
ALTER SYSTEM SET effective_cache_size = '12GB';
ALTER SYSTEM SET maintenance_work_mem = '1GB';
ALTER SYSTEM SET work_mem = '50MB';

-- 쿼리 플래너 설정
ALTER SYSTEM SET random_page_cost = 1.1;  -- SSD 사용시
ALTER SYSTEM SET effective_io_concurrency = 200;  -- SSD 사용시

-- 쿼리 병렬 처리
ALTER SYSTEM SET max_parallel_workers_per_gather = 4;
ALTER SYSTEM SET max_parallel_workers = 8;
ALTER SYSTEM SET max_worker_processes = 8;

-- WAL 설정
ALTER SYSTEM SET wal_buffers = '16MB';
ALTER SYSTEM SET checkpoint_completion_target = 0.9;
ALTER SYSTEM SET max_wal_size = '2GB';

-- 로깅 설정
ALTER SYSTEM SET log_min_duration_statement = 1000;  -- 1초 이상 쿼리 로깅
ALTER SYSTEM SET log_line_prefix = '%t [%p]: [%l-1] user=%u,db=%d,app=%a,client=%h ';
ALTER SYSTEM SET log_checkpoints = on;
ALTER SYSTEM SET log_connections = on;
ALTER SYSTEM SET log_disconnections = on;
ALTER SYSTEM SET log_lock_waits = on;

-- 통계 수집
ALTER SYSTEM SET track_activities = on;
ALTER SYSTEM SET track_counts = on;
ALTER SYSTEM SET track_io_timing = on;
ALTER SYSTEM SET track_functions = 'all';

-- 자동 Vacuum 설정
ALTER SYSTEM SET autovacuum = on;
ALTER SYSTEM SET autovacuum_max_workers = 4;
ALTER SYSTEM SET autovacuum_naptime = '10s';

-- ================================================================================
-- 10. 테이블별 스토리지 파라미터
-- ================================================================================

-- 주문 테이블 (빈번한 업데이트)
ALTER TABLE trading.orders SET (
    fillfactor = 90,
    autovacuum_vacuum_scale_factor = 0.05,
    autovacuum_analyze_scale_factor = 0.02
);

-- 포지션 테이블
ALTER TABLE trading.positions SET (
    fillfactor = 90,
    autovacuum_vacuum_scale_factor = 0.05
);

-- 시장 데이터 (읽기 전용)
ALTER TABLE market.stock_info SET (
    fillfactor = 100
);

-- ================================================================================
-- 11. 느린 쿼리 모니터링
-- ================================================================================

-- pg_stat_statements 확장 설치
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- 느린 쿼리 조회 뷰
CREATE OR REPLACE VIEW monitoring.v_slow_queries AS
SELECT
    query,
    calls,
    total_exec_time,
    mean_exec_time,
    max_exec_time,
    stddev_exec_time,
    rows
FROM pg_stat_statements
WHERE mean_exec_time > 100  -- 100ms 이상
ORDER BY mean_exec_time DESC
LIMIT 50;

-- ================================================================================
-- 12. 데이터 압축 및 정리
-- ================================================================================

-- TimescaleDB 압축 설정 (이미 설정된 것 외 추가)
ALTER TABLE market.market_data SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'stock_code',
    timescaledb.compress_orderby = 'data_timestamp DESC'
);

ALTER TABLE account.account_balance_history SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'account_id',
    timescaledb.compress_orderby = 'balance_date DESC'
);

-- ================================================================================
-- 13. 백업 및 복구 스크립트
-- ================================================================================

-- 전체 백업 (bash 명령)
-- pg_dump -h localhost -U postgres -d algotrading -F c -f backup_$(date +%Y%m%d).dump

-- 스키마별 백업
-- pg_dump -h localhost -U postgres -d algotrading -n account -F c -f backup_account.dump

-- 복구
-- pg_restore -h localhost -U postgres -d algotrading -c backup.dump

-- ================================================================================
-- 14. 성능 모니터링 쿼리
-- ================================================================================

-- 테이블 크기 조회
CREATE OR REPLACE VIEW monitoring.v_table_sizes AS
SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS total_size,
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) AS table_size,
    pg_size_pretty(pg_indexes_size(schemaname||'.'||tablename)) AS indexes_size
FROM pg_tables
WHERE schemaname IN ('account', 'trading', 'strategy', 'risk', 'market', 'monitoring', 'audit')
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- 인덱스 사용률 조회
CREATE OR REPLACE VIEW monitoring.v_index_usage AS
SELECT
    schemaname,
    tablename,
    indexname,
    idx_scan AS index_scans,
    idx_tup_read AS tuples_read,
    idx_tup_fetch AS tuples_fetched,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE schemaname IN ('account', 'trading', 'strategy', 'risk', 'market', 'monitoring', 'audit')
ORDER BY idx_scan ASC;

-- 캐시 히트율
CREATE OR REPLACE VIEW monitoring.v_cache_hit_ratio AS
SELECT
    'Cache Hit Ratio' AS metric,
    ROUND(
        100.0 * sum(heap_blks_hit) / NULLIF(sum(heap_blks_hit) + sum(heap_blks_read), 0),
        2
    ) AS percentage
FROM pg_statio_user_tables;

-- 활성 연결 수
CREATE OR REPLACE VIEW monitoring.v_active_connections AS
SELECT
    datname AS database,
    usename AS username,
    COUNT(*) AS connection_count,
    state
FROM pg_stat_activity
WHERE datname = current_database()
GROUP BY datname, usename, state
ORDER BY connection_count DESC;

-- ================================================================================
-- 15. 정리 작업 스케줄링
-- ================================================================================

-- 주기적 Vacuum 함수
CREATE OR REPLACE FUNCTION maintenance.run_vacuum_analyze()
RETURNS void AS $$
BEGIN
    VACUUM ANALYZE account.trading_accounts;
    VACUUM ANALYZE trading.orders;
    VACUUM ANALYZE trading.positions;
    VACUUM ANALYZE strategy.trading_strategies;
    VACUUM ANALYZE market.stock_info;
    
    RAISE NOTICE 'Vacuum analyze completed at %', NOW();
END;
$$ LANGUAGE plpgsql;

-- ================================================================================
-- 완료 메시지
-- ================================================================================

DO $$
BEGIN
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Performance optimization completed successfully!';
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Created indexes, views, and functions for optimal query performance';
    RAISE NOTICE 'TimescaleDB continuous aggregates configured';
    RAISE NOTICE 'Database parameters tuned for production workload';
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'IMPORTANT: Run "SELECT pg_reload_conf();" or restart PostgreSQL to apply system parameter changes';
    RAISE NOTICE '================================================================================';
END $$;

-- 설정 재로드 (일부 설정만 적용)
SELECT pg_reload_conf();
