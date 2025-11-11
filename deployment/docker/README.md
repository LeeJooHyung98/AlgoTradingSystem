# Docker Deployment Guide

이 디렉토리는 AlgoTradingSystem의 인프라 서비스를 Docker로 실행하기 위한 설정 파일을 포함합니다.

## 서비스 구성

- **PostgreSQL 17 + TimescaleDB**: 시계열 데이터 최적화된 메인 데이터베이스
- **Redis 7**: 캐싱 및 세션 관리
- **RabbitMQ 3**: 메시지 큐 및 이벤트 버스

## 사전 요구사항

1. Docker Desktop 설치 및 실행
2. 포트 사용 가능 여부 확인:
   - PostgreSQL: 5432
   - Redis: 6379
   - RabbitMQ: 5672, 15672

## 시작하기

### 1. 모든 서비스 시작

```bash
cd deployment/docker
docker-compose up -d
```

### 2. 개별 서비스 시작

```bash
# PostgreSQL만 시작
docker-compose up -d postgres

# Redis만 시작
docker-compose up -d redis

# RabbitMQ만 시작
docker-compose up -d rabbitmq
```

### 3. 서비스 상태 확인

```bash
docker-compose ps
```

### 4. 로그 확인

```bash
# 모든 서비스 로그
docker-compose logs -f

# 특정 서비스 로그
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f rabbitmq
```

### 5. 서비스 중지

```bash
# 모든 서비스 중지 (데이터 유지)
docker-compose stop

# 모든 서비스 중지 및 제거 (데이터 유지)
docker-compose down

# 모든 서비스 및 볼륨 삭제 (데이터 삭제)
docker-compose down -v
```

## 데이터베이스 접속 정보

### PostgreSQL

- **Host**: localhost
- **Port**: 5432
- **Database**: AlgoTradingDb
- **Username**: postgres
- **Password**: postgres123

**Connection String:**
```
Server=localhost;Database=AlgoTradingDb;Port=5432;User Id=postgres;Password=postgres123;
```

### Redis

- **Host**: localhost
- **Port**: 6379
- **Password**: redis123

**Connection String:**
```
localhost:6379,password=redis123
```

### RabbitMQ

- **AMQP Port**: 5672
- **Management UI**: http://localhost:15672
- **Username**: rabbitmq
- **Password**: rabbitmq123

**Connection String:**
```
amqp://rabbitmq:rabbitmq123@localhost:5672/
```

## 데이터베이스 초기화

데이터베이스 스키마는 PostgreSQL 컨테이너 시작 시 자동으로 생성됩니다.

`init-scripts` 폴더에 있는 SQL 스크립트가 자동 실행됩니다:
- `01_create_database_schema.sql`: 전체 스키마 생성

### 수동으로 스키마 재생성

```bash
# PostgreSQL 컨테이너 접속
docker exec -it algotrading_postgres psql -U postgres -d AlgoTradingDb

# 또는 SQL 파일 직접 실행
docker exec -i algotrading_postgres psql -U postgres -d AlgoTradingDb < init-scripts/01_create_database_schema.sql
```

## 데이터 백업 및 복원

### 백업

```bash
# 전체 데이터베이스 백업
docker exec algotrading_postgres pg_dump -U postgres AlgoTradingDb > backup_$(date +%Y%m%d_%H%M%S).sql

# 스키마만 백업
docker exec algotrading_postgres pg_dump -U postgres --schema-only AlgoTradingDb > schema_backup.sql

# 데이터만 백업
docker exec algotrading_postgres pg_dump -U postgres --data-only AlgoTradingDb > data_backup.sql
```

### 복원

```bash
# 백업 파일에서 복원
docker exec -i algotrading_postgres psql -U postgres AlgoTradingDb < backup_20250101_120000.sql
```

## 트러블슈팅

### PostgreSQL 연결 실패

1. 컨테이너 상태 확인:
   ```bash
   docker-compose ps
   ```

2. PostgreSQL 로그 확인:
   ```bash
   docker-compose logs postgres
   ```

3. Health check 확인:
   ```bash
   docker inspect algotrading_postgres | grep -A 10 Health
   ```

### 포트 충돌

다른 서비스가 이미 해당 포트를 사용 중인 경우:

```bash
# Windows에서 포트 사용 확인
netstat -ano | findstr :5432
netstat -ano | findstr :6379
netstat -ano | findstr :5672

# docker-compose.yml에서 포트 변경
# 예: "15432:5432" (호스트:컨테이너)
```

### 데이터 초기화

모든 데이터를 삭제하고 처음부터 시작:

```bash
# 컨테이너 및 볼륨 삭제
docker-compose down -v

# 다시 시작
docker-compose up -d
```

## 보안 주의사항

⚠️ **프로덕션 환경에서는 반드시 비밀번호를 변경하세요!**

- `.env` 파일의 비밀번호 변경
- `docker-compose.yml`의 environment 변경
- User Secrets 또는 환경 변수로 관리

## 성능 튜닝

PostgreSQL 성능 최적화를 위해 `docker-compose.yml`에 다음 설정 추가 가능:

```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=256MB"
  - "-c"
  - "max_connections=200"
  - "-c"
  - "work_mem=10MB"
```

## 모니터링

### RabbitMQ Management UI

http://localhost:15672 접속
- Username: rabbitmq
- Password: rabbitmq123

### Redis 모니터링

```bash
# Redis CLI 접속
docker exec -it algotrading_redis redis-cli -a redis123

# 정보 확인
INFO
DBSIZE
MONITOR
```

### PostgreSQL 모니터링

```bash
# psql 접속
docker exec -it algotrading_postgres psql -U postgres -d AlgoTradingDb

# 활성 연결 확인
SELECT * FROM pg_stat_activity;

# 데이터베이스 크기
SELECT pg_size_pretty(pg_database_size('AlgoTradingDb'));

# TimescaleDB Hypertables 확인
SELECT * FROM timescaledb_information.hypertables;
```
