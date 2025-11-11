-- ================================================================================
-- Foreign Key Constraints
-- ================================================================================
-- This script adds foreign key constraints after all tables are created
-- to avoid circular dependency issues
-- ================================================================================

-- Drop existing foreign keys if they exist (for idempotency)
ALTER TABLE IF EXISTS trading.orders DROP CONSTRAINT IF EXISTS orders_strategy_id_fkey;
ALTER TABLE IF EXISTS trading.orders DROP CONSTRAINT IF EXISTS orders_stock_code_fkey;
ALTER TABLE IF EXISTS trading.positions DROP CONSTRAINT IF EXISTS positions_strategy_id_fkey;
ALTER TABLE IF EXISTS trading.positions DROP CONSTRAINT IF EXISTS positions_stock_code_fkey;
ALTER TABLE IF EXISTS strategy.strategy_signals DROP CONSTRAINT IF EXISTS strategy_signals_stock_code_fkey;

-- Add foreign keys for trading.orders
ALTER TABLE trading.orders
    ADD CONSTRAINT orders_strategy_id_fkey
    FOREIGN KEY (strategy_id)
    REFERENCES strategy.trading_strategies(id)
    ON DELETE SET NULL;

ALTER TABLE trading.orders
    ADD CONSTRAINT orders_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

-- Add foreign keys for trading.positions
ALTER TABLE trading.positions
    ADD CONSTRAINT positions_strategy_id_fkey
    FOREIGN KEY (strategy_id)
    REFERENCES strategy.trading_strategies(id)
    ON DELETE SET NULL;

ALTER TABLE trading.positions
    ADD CONSTRAINT positions_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

-- Add foreign keys for strategy.strategy_signals
ALTER TABLE strategy.strategy_signals
    ADD CONSTRAINT strategy_signals_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

-- Add foreign keys for market data tables
ALTER TABLE IF EXISTS market.market_data DROP CONSTRAINT IF EXISTS market_data_stock_code_fkey;
ALTER TABLE IF EXISTS market.candle_data DROP CONSTRAINT IF EXISTS candle_data_stock_code_fkey;
ALTER TABLE IF EXISTS market.order_book DROP CONSTRAINT IF EXISTS order_book_stock_code_fkey;
ALTER TABLE IF EXISTS market.tick_data DROP CONSTRAINT IF EXISTS tick_data_stock_code_fkey;

ALTER TABLE market.market_data
    ADD CONSTRAINT market_data_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

ALTER TABLE market.candle_data
    ADD CONSTRAINT candle_data_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

ALTER TABLE market.order_book
    ADD CONSTRAINT order_book_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

ALTER TABLE market.tick_data
    ADD CONSTRAINT tick_data_stock_code_fkey
    FOREIGN KEY (stock_code)
    REFERENCES market.stock_info(stock_code)
    ON DELETE RESTRICT;

RAISE NOTICE 'Foreign key constraints added successfully!';
