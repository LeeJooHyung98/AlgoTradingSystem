-- ================================================================================
-- Stock Trading Algorithm DDD System - Initial Data & Seed Data
-- PostgreSQL 17 + TimescaleDB
-- ================================================================================
-- Author: Development Team
-- Date: 2025-10-27
-- Description: Seed data for testing and initial system setup
-- ================================================================================

-- ================================================================================
-- 1. 종목 정보 삽입 (Market Data Context)
-- ================================================================================

-- 대표 KOSPI 종목
INSERT INTO market.stock_info (stock_code, stock_name, market_type, sector, industry, listing_date, listed_shares, is_tradable, is_suspended)
VALUES
    -- 시가총액 Top 10
    ('005930', '삼성전자', 'KOSPI', 'IT', '반도체', '1975-06-11', 5969782550, TRUE, FALSE),
    ('000660', 'SK하이닉스', 'KOSPI', 'IT', '반도체', '1996-12-26', 728002365, TRUE, FALSE),
    ('035420', 'NAVER', 'KOSPI', 'IT', '인터넷', '2002-10-29', 164263395, TRUE, FALSE),
    ('051910', 'LG화학', 'KOSPI', '화학', '화학', '2001-04-25', 70592343, TRUE, FALSE),
    ('006400', '삼성SDI', 'KOSPI', 'IT', '전기전자', '1979-05-02', 68760259, TRUE, FALSE),
    ('035720', '카카오', 'KOSPI', 'IT', '인터넷', '2017-07-10', 443942894, TRUE, FALSE),
    ('068270', '셀트리온', 'KOSPI', '제약', '바이오', '2008-07-21', 137803219, TRUE, FALSE),
    ('207940', '삼성바이오로직스', 'KOSPI', '제약', '바이오', '2016-11-10', 80000000, TRUE, FALSE),
    ('005380', '현대차', 'KOSPI', '자동차', '자동차', '1974-09-05', 213668187, TRUE, FALSE),
    ('012330', '현대모비스', 'KOSPI', '자동차', '자동차부품', '2000-12-27', 42129293, TRUE, FALSE),
    
    -- 금융
    ('055550', '신한지주', 'KOSPI', '금융', '은행', '2001-09-10', 1830085397, TRUE, FALSE),
    ('105560', 'KB금융', 'KOSPI', '금융', '은행', '2008-09-29', 416115333, TRUE, FALSE),
    ('086790', '하나금융지주', 'KOSPI', '금융', '은행', '2005-12-15', 383524967, TRUE, FALSE),
    
    -- 에너지
    ('096770', 'SK이노베이션', 'KOSPI', '화학', '석유화학', '2007-06-21', 118245593, TRUE, FALSE),
    ('034730', 'SK', 'KOSPI', '기타', '지주회사', '1999-08-16', 71361345, TRUE, FALSE),
    
    -- 유통
    ('011170', '롯데케미칼', 'KOSPI', '화학', '석유화학', '2001-07-27', 95175417, TRUE, FALSE),
    ('032830', '삼성생명', 'KOSPI', '금융', '보험', '2010-05-12', 240000000, TRUE, FALSE)
ON CONFLICT (stock_code) DO NOTHING;

-- KOSDAQ 종목
INSERT INTO market.stock_info (stock_code, stock_name, market_type, sector, industry, listing_date, listed_shares, is_tradable, is_suspended)
VALUES
    ('247540', '에코프로비엠', 'KOSDAQ', 'IT', '전기전자', '2016-07-25', 46026120, TRUE, FALSE),
    ('086520', '에코프로', 'KOSDAQ', '화학', '화학', '2007-08-24', 95251036, TRUE, FALSE),
    ('357780', '솔브레인', 'KOSDAQ', '화학', '화학', '2017-06-28', 21481076, TRUE, FALSE),
    ('091990', '셀트리온헬스케어', 'KOSDAQ', '제약', '바이오', '2014-02-17', 175673027, TRUE, FALSE),
    ('263750', '펄어비스', 'KOSDAQ', 'IT', '게임', '2017-09-13', 25000000, TRUE, FALSE),
    ('293490', '카카오게임즈', 'KOSDAQ', 'IT', '게임', '2020-09-10', 88000000, TRUE, FALSE),
    ('112040', '위메이드', 'KOSDAQ', 'IT', '게임', '2010-04-23', 30742373, TRUE, FALSE),
    ('067160', '아프리카TV', 'KOSDAQ', 'IT', '인터넷', '2014-07-04', 22320600, TRUE, FALSE),
    ('036930', '주성엔지니어링', 'KOSDAQ', 'IT', '반도체장비', '2000-08-08', 62590952, TRUE, FALSE),
    ('058470', '리노공업', 'KOSDAQ', 'IT', '반도체부품', '2002-08-22', 8883417, TRUE, FALSE)
ON CONFLICT (stock_code) DO NOTHING;

-- 제한가 설정 (전일 종가 기준 ±30%)
UPDATE market.stock_info 
SET upper_limit_price = 100000 * 1.30,
    lower_limit_price = 100000 * 0.70
WHERE stock_code = '005930';  -- 삼성전자 예시

-- ================================================================================
-- 2. 테스트 사용자 및 계좌 생성 (Account Context)
-- ================================================================================

-- 테스트 사용자 UUID 생성
DO $$
DECLARE
    v_user_id_1 UUID := '550e8400-e29b-41d4-a716-446655440000';
    v_user_id_2 UUID := '7c9e6679-7425-40de-944b-e07fc1f90ae7';
    v_account_id_1 UUID;
    v_account_id_2 UUID;
    v_account_id_3 UUID;
BEGIN
    -- 계좌 1: 현금 계좌 (사용자 1)
    v_account_id_1 := uuid_generate_v4();
    INSERT INTO account.trading_accounts (
        id, account_number, account_name, account_type, account_status,
        trading_permission, owner_id, broker_name, broker_api_version,
        total_assets, cash_balance, available_balance, withdrawable_amount,
        currency, created_at
    ) VALUES (
        v_account_id_1, '1234567890', 'Main Trading Account', 'CASH', 'ACTIVE',
        'FULL', v_user_id_1, 'Kiwoom', '1.0.0',
        10000000, 10000000, 9500000, 9500000,
        'KRW', CURRENT_TIMESTAMP
    );
    
    -- 계좌 2: 신용 계좌 (사용자 1)
    v_account_id_2 := uuid_generate_v4();
    INSERT INTO account.trading_accounts (
        id, account_number, account_name, account_type, account_status,
        trading_permission, owner_id, broker_name, broker_api_version,
        total_assets, cash_balance, available_balance, withdrawable_amount,
        margin_total, margin_available, margin_used, maintenance_margin, margin_multiplier,
        currency, created_at
    ) VALUES (
        v_account_id_2, '1234567891', 'Margin Trading Account', 'MARGIN', 'ACTIVE',
        'FULL', v_user_id_1, 'Kiwoom', '1.0.0',
        20000000, 10000000, 15000000, 8000000,
        20000000, 10000000, 10000000, 5000000, 2.0,
        'KRW', CURRENT_TIMESTAMP
    );
    
    -- 계좌 3: 현금 계좌 (사용자 2)
    v_account_id_3 := uuid_generate_v4();
    INSERT INTO account.trading_accounts (
        id, account_number, account_name, account_type, account_status,
        trading_permission, owner_id, broker_name, broker_api_version,
        total_assets, cash_balance, available_balance, withdrawable_amount,
        currency, created_at
    ) VALUES (
        v_account_id_3, '9876543210', 'Test Account', 'CASH', 'ACTIVE',
        'FULL', v_user_id_2, 'Kiwoom', '1.0.0',
        5000000, 5000000, 5000000, 5000000,
        'KRW', CURRENT_TIMESTAMP
    );
    
    RAISE NOTICE 'Created test accounts: %, %, %', v_account_id_1, v_account_id_2, v_account_id_3;
END $$;

-- ================================================================================
-- 3. 전략 템플릿 생성 (Strategy Context)
-- ================================================================================

DO $$
DECLARE
    v_user_id UUID := '550e8400-e29b-41d4-a716-446655440000';
    v_strategy_id_1 UUID;
    v_strategy_id_2 UUID;
    v_strategy_id_3 UUID;
BEGIN
    -- 전략 1: 이동평균 크로스오버
    v_strategy_id_1 := uuid_generate_v4();
    INSERT INTO strategy.trading_strategies (
        id, owner_id, strategy_name, strategy_type, strategy_status,
        description, target_stocks, time_frame,
        max_positions, position_size_percent,
        max_loss_per_trade, max_daily_loss, stop_loss_percent, take_profit_percent,
        created_at
    ) VALUES (
        v_strategy_id_1, v_user_id, 'MA Crossover Strategy', 'MOMENTUM', 'DRAFT',
        '5일 이동평균이 20일 이동평균을 상향 돌파시 매수, 하향 돌파시 매도',
        ARRAY['005930', '035420', '035720']::TEXT[],
        '1d',
        5, 10.00,
        2.00, 5.00, 3.00, 5.00,
        CURRENT_TIMESTAMP
    );
    
    -- 전략 규칙 추가
    INSERT INTO strategy.strategy_rules (strategy_id, rule_name, rule_type, rule_order, condition_json)
    VALUES
        (v_strategy_id_1, 'MA5 > MA20', 'ENTRY', 1, '{"indicator": "MA", "period1": 5, "period2": 20, "condition": "crossover"}'::jsonb),
        (v_strategy_id_1, 'MA5 < MA20', 'EXIT', 1, '{"indicator": "MA", "period1": 5, "period2": 20, "condition": "crossunder"}'::jsonb),
        (v_strategy_id_1, 'Stop Loss', 'EXIT', 2, '{"type": "stop_loss", "percent": 3.0}'::jsonb);
    
    -- 전략 2: RSI 역전 전략
    v_strategy_id_2 := uuid_generate_v4();
    INSERT INTO strategy.trading_strategies (
        id, owner_id, strategy_name, strategy_type, strategy_status,
        description, target_stocks, time_frame,
        max_positions, position_size_percent,
        max_loss_per_trade, max_daily_loss, stop_loss_percent, take_profit_percent,
        created_at
    ) VALUES (
        v_strategy_id_2, v_user_id, 'RSI Reversal Strategy', 'MEAN_REVERSION', 'DRAFT',
        'RSI가 30 이하일 때 매수, 70 이상일 때 매도',
        ARRAY['000660', '051910', '006400']::TEXT[],
        '1h',
        3, 15.00,
        3.00, 5.00, 5.00, 10.00,
        CURRENT_TIMESTAMP
    );
    
    INSERT INTO strategy.strategy_rules (strategy_id, rule_name, rule_type, rule_order, condition_json)
    VALUES
        (v_strategy_id_2, 'RSI < 30', 'ENTRY', 1, '{"indicator": "RSI", "period": 14, "threshold": 30, "condition": "below"}'::jsonb),
        (v_strategy_id_2, 'RSI > 70', 'EXIT', 1, '{"indicator": "RSI", "period": 14, "threshold": 70, "condition": "above"}'::jsonb);
    
    -- 전략 3: 볼린저밴드 돌파
    v_strategy_id_3 := uuid_generate_v4();
    INSERT INTO strategy.trading_strategies (
        id, owner_id, strategy_name, strategy_type, strategy_status,
        description, target_stocks, time_frame,
        max_positions, position_size_percent,
        max_loss_per_trade, max_daily_loss, stop_loss_percent, take_profit_percent,
        created_at
    ) VALUES (
        v_strategy_id_3, v_user_id, 'Bollinger Breakout Strategy', 'BREAKOUT', 'ACTIVE',
        '볼린저밴드 하단 돌파시 매수, 상단 도달시 매도',
        ARRAY['005930', '035420', '000660', '051910']::TEXT[],
        '1d',
        4, 12.50,
        2.50, 5.00, 4.00, 8.00,
        CURRENT_TIMESTAMP
    );
    
    INSERT INTO strategy.strategy_rules (strategy_id, rule_name, rule_type, rule_order, condition_json)
    VALUES
        (v_strategy_id_3, 'Price < Lower Band', 'ENTRY', 1, '{"indicator": "BB", "period": 20, "std": 2, "condition": "below_lower"}'::jsonb),
        (v_strategy_id_3, 'Price > Upper Band', 'EXIT', 1, '{"indicator": "BB", "period": 20, "std": 2, "condition": "above_upper"}'::jsonb);
    
    RAISE NOTICE 'Created test strategies: %, %, %', v_strategy_id_1, v_strategy_id_2, v_strategy_id_3;
END $$;

-- ================================================================================
-- 4. 리스크 프로필 생성 (Risk Management Context)
-- ================================================================================

DO $$
DECLARE
    v_account_id UUID;
    v_profile_id UUID;
BEGIN
    -- 첫 번째 계좌 조회
    SELECT id INTO v_account_id FROM account.trading_accounts LIMIT 1;
    
    -- 보수적 리스크 프로필
    v_profile_id := uuid_generate_v4();
    INSERT INTO risk.risk_profiles (
        id, account_id, profile_name, risk_level,
        max_position_size, max_positions_count, max_position_percent,
        max_daily_loss, max_daily_loss_percent, max_drawdown,
        max_sector_concentration, max_stock_concentration,
        max_leverage, kill_switch_enabled
    ) VALUES (
        v_profile_id, v_account_id, 'Conservative Profile', 'CONSERVATIVE',
        1000000, 5, 10.00,
        200000, 2.00, 10.00,
        30.00, 20.00,
        1.00, TRUE
    );
    
    RAISE NOTICE 'Created risk profile: %', v_profile_id;
END $$;

-- ================================================================================
-- 5. 샘플 시장 데이터 생성 (Market Data Context)
-- ================================================================================

-- 삼성전자 최근 30일 일봉 데이터
DO $$
DECLARE
    v_date TIMESTAMP;
    v_base_price DECIMAL := 70000;
    v_price DECIMAL;
    v_volume BIGINT;
BEGIN
    FOR i IN 0..29 LOOP
        v_date := CURRENT_DATE - (i || ' days')::INTERVAL;
        
        -- 가격 랜덤 변동 (-2% ~ +2%)
        v_price := v_base_price * (1 + (random() * 0.04 - 0.02));
        v_volume := 10000000 + (random() * 5000000)::BIGINT;
        
        INSERT INTO market.market_data (
            stock_code, data_timestamp,
            open_price, high_price, low_price, close_price,
            volume, change_amount, change_percent,
            market_status
        ) VALUES (
            '005930', v_date,
            v_price * 0.99, v_price * 1.01, v_price * 0.98, v_price,
            v_volume, v_price - v_base_price, ((v_price - v_base_price) / v_base_price * 100),
            'CLOSED'
        );
        
        -- 일봉 캔들 데이터
        INSERT INTO market.candle_data (
            stock_code, interval_type, candle_timestamp,
            open_price, high_price, low_price, close_price, volume
        ) VALUES (
            '005930', '1d', v_date,
            v_price * 0.99, v_price * 1.01, v_price * 0.98, v_price, v_volume
        );
    END LOOP;
    
    RAISE NOTICE 'Created 30 days of sample market data for stock 005930';
END $$;

-- ================================================================================
-- 6. 샘플 주문 및 체결 데이터
-- ================================================================================

DO $$
DECLARE
    v_account_id UUID;
    v_order_id UUID;
    v_execution_id UUID;
BEGIN
    -- 첫 번째 계좌 조회
    SELECT id INTO v_account_id FROM account.trading_accounts LIMIT 1;
    
    -- 주문 1: 완전 체결된 매수 주문
    v_order_id := uuid_generate_v4();
    INSERT INTO trading.orders (
        id, account_id, stock_code, market_type,
        order_type, order_side, order_status,
        order_price, order_quantity, executed_quantity, remaining_quantity,
        average_execution_price, total_commission, total_tax, total_cost,
        broker_order_id, created_at, submitted_at, accepted_at, filled_at
    ) VALUES (
        v_order_id, v_account_id, '005930', 'KOSPI',
        'LIMIT', 'BUY', 'FILLED',
        70000, 100, 100, 0,
        70000, 210, 0, 7000210,
        'KB-ORD-001', CURRENT_TIMESTAMP - INTERVAL '1 day',
        CURRENT_TIMESTAMP - INTERVAL '1 day', CURRENT_TIMESTAMP - INTERVAL '1 day',
        CURRENT_TIMESTAMP - INTERVAL '1 day'
    );
    
    -- 체결 내역
    v_execution_id := uuid_generate_v4();
    INSERT INTO trading.order_executions (
        id, order_id, executed_quantity, execution_price,
        commission, tax, total_amount, execution_time, broker_execution_id
    ) VALUES (
        v_execution_id, v_order_id, 100, 70000,
        210, 0, 7000210, CURRENT_TIMESTAMP - INTERVAL '1 day', 'KB-EXEC-001'
    );
    
    -- 포지션 생성
    INSERT INTO trading.positions (
        account_id, stock_code, market_type, position_side,
        quantity, average_entry_price, current_price,
        unrealized_pl, unrealized_pl_percent,
        total_commission, total_tax, opened_at
    ) VALUES (
        v_account_id, '005930', 'KOSPI', 'LONG',
        100, 70000, 71000,
        100000, 1.43,
        210, 0, CURRENT_TIMESTAMP - INTERVAL '1 day'
    );
    
    -- 주문 2: 부분 체결된 매수 주문
    v_order_id := uuid_generate_v4();
    INSERT INTO trading.orders (
        id, account_id, stock_code, market_type,
        order_type, order_side, order_status,
        order_price, order_quantity, executed_quantity, remaining_quantity,
        average_execution_price, total_commission, total_tax,
        broker_order_id, created_at, submitted_at, accepted_at
    ) VALUES (
        v_order_id, v_account_id, '035420', 'KOSPI',
        'LIMIT', 'BUY', 'PARTIALLY_FILLED',
        200000, 50, 30, 20,
        200000, 180, 0,
        'KB-ORD-002', CURRENT_TIMESTAMP - INTERVAL '2 hours',
        CURRENT_TIMESTAMP - INTERVAL '2 hours', CURRENT_TIMESTAMP - INTERVAL '2 hours'
    );
    
    -- 주문 3: 대기중인 매수 주문
    v_order_id := uuid_generate_v4();
    INSERT INTO trading.orders (
        id, account_id, stock_code, market_type,
        order_type, order_side, order_status,
        order_price, order_quantity, executed_quantity, remaining_quantity,
        broker_order_id, created_at, submitted_at
    ) VALUES (
        v_order_id, v_account_id, '000660', 'KOSPI',
        'LIMIT', 'BUY', 'SUBMITTED',
        130000, 80, 0, 80,
        'KB-ORD-003', CURRENT_TIMESTAMP - INTERVAL '30 minutes',
        CURRENT_TIMESTAMP - INTERVAL '30 minutes'
    );
    
    RAISE NOTICE 'Created sample orders and positions';
END $$;

-- ================================================================================
-- 7. 모니터링 샘플 데이터
-- ================================================================================

-- 시스템 메트릭 (최근 24시간)
DO $$
DECLARE
    v_timestamp TIMESTAMP;
BEGIN
    FOR i IN 0..23 LOOP
        v_timestamp := CURRENT_TIMESTAMP - (i || ' hours')::INTERVAL;
        
        INSERT INTO monitoring.system_metrics (
            metric_timestamp,
            cpu_usage_percent, memory_usage_percent, memory_used_mb, memory_total_mb,
            network_in_mbps, network_out_mbps,
            db_connections, db_query_time_ms,
            api_request_count, api_error_count, api_avg_latency_ms
        ) VALUES (
            v_timestamp,
            20 + random() * 30, 40 + random() * 20, 4000 + (random() * 2000)::BIGINT, 8192,
            10 + random() * 20, 5 + random() * 10,
            50 + (random() * 30)::INTEGER, 10 + random() * 20,
            1000 + (random() * 500)::INTEGER, (random() * 10)::INTEGER, 50 + random() * 50
        );
    END LOOP;
    
    RAISE NOTICE 'Created 24 hours of system metrics';
END $$;

-- 샘플 알림
DO $$
DECLARE
    v_account_id UUID;
BEGIN
    SELECT id INTO v_account_id FROM account.trading_accounts LIMIT 1;
    
    INSERT INTO monitoring.alerts (
        alert_type, alert_status, severity,
        title, message, account_id
    ) VALUES
        ('SYSTEM', 'ACTIVE', 'HIGH', 
         'High Memory Usage', 'System memory usage exceeded 80%', NULL),
        ('TRADING', 'RESOLVED', 'MEDIUM', 
         'Order Execution Delay', 'Order execution took longer than 1 second', v_account_id),
        ('RISK', 'ACTIVE', 'CRITICAL', 
         'Daily Loss Limit Approaching', 'Daily loss is at 90% of the limit', v_account_id);
    
    RAISE NOTICE 'Created sample alerts';
END $$;

-- ================================================================================
-- 8. 감사 로그 샘플
-- ================================================================================

DO $$
DECLARE
    v_user_id UUID := '550e8400-e29b-41d4-a716-446655440000';
    v_account_id UUID;
    v_order_id UUID;
BEGIN
    SELECT id INTO v_account_id FROM account.trading_accounts LIMIT 1;
    SELECT id INTO v_order_id FROM trading.orders LIMIT 1;
    
    INSERT INTO audit.audit_logs (
        log_timestamp, action_type, entity_type, entity_id,
        user_id, user_ip, description
    ) VALUES
        (CURRENT_TIMESTAMP - INTERVAL '1 hour', 'CREATE', 'Order', v_order_id,
         v_user_id, '192.168.1.100', 'Created new order for stock 005930'),
        (CURRENT_TIMESTAMP - INTERVAL '30 minutes', 'UPDATE', 'Order', v_order_id,
         v_user_id, '192.168.1.100', 'Order partially filled'),
        (CURRENT_TIMESTAMP - INTERVAL '10 minutes', 'EXECUTE', 'Strategy', uuid_generate_v4(),
         v_user_id, '192.168.1.100', 'Strategy generated buy signal');
    
    RAISE NOTICE 'Created sample audit logs';
END $$;

-- ================================================================================
-- 9. 통계 정보 업데이트
-- ================================================================================

ANALYZE account.trading_accounts;
ANALYZE trading.orders;
ANALYZE trading.positions;
ANALYZE strategy.trading_strategies;
ANALYZE market.market_data;
ANALYZE market.candle_data;

-- ================================================================================
-- 완료 메시지
-- ================================================================================

DO $$
DECLARE
    v_stock_count INTEGER;
    v_account_count INTEGER;
    v_strategy_count INTEGER;
    v_order_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_stock_count FROM market.stock_info;
    SELECT COUNT(*) INTO v_account_count FROM account.trading_accounts;
    SELECT COUNT(*) INTO v_strategy_count FROM strategy.trading_strategies;
    SELECT COUNT(*) INTO v_order_count FROM trading.orders;
    
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Initial data seeding completed successfully!';
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Stocks: %', v_stock_count;
    RAISE NOTICE 'Accounts: %', v_account_count;
    RAISE NOTICE 'Strategies: %', v_strategy_count;
    RAISE NOTICE 'Orders: %', v_order_count;
    RAISE NOTICE '================================================================================';
    RAISE NOTICE 'Sample market data, orders, and monitoring data created';
    RAISE NOTICE '================================================================================';
END $$;
