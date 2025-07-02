## 1. 문제 분석

### 1.1 비즈니스 도메인 분석

첨부된 DB 스키마를 분석한 결과, 이 ERP 시스템은 IT 서비스업의 핵심 업무 프로세스를 다음과 같이 지원하고 있습니다:

#### **핵심 도메인 영역**
1. **클라이언트 관리** - 고객사 정보, 계약 가치, 규모별 분류
2. **프로젝트 관리** - 프로젝트 생명주기, 마일스톤, 기술 스택 관리
3. **인적 자원 관리** - 직원 정보, 스킬 매트릭스, 조직 구조
4. **리소스 할당** - 프로젝트별 인력 배치, 가용성 관리
5. **시간 추적** - 작업 시간 기록, 빌링 가능 시간 관리
6. **재무 관리** - 인보이스 발행, 프로젝트 수익성 추적
7. **보안 및 감사** - 사용자 권한, 변경 이력 추적

### 1.2 타겟 고객 분석 (50-200명 규모 IT 회사)

#### **주요 Pain Points**
- **프로젝트 수익성 관리**: 실시간 프로젝트 손익 파악 어려움
- **리소스 최적화**: 개발자 스킬과 프로젝트 요구사항 매칭
- **빌링 정확성**: 작업 시간 추적 및 인보이스 발행 자동화
- **인력 활용률**: 직원별 빌링 가능 시간 비율 관리
- **다중 프로젝트 관리**: 동시 진행 프로젝트의 통합 관리

### 1.3 기술적 요구사항 분석

#### **성능 요구사항**
- 동시 사용자: 50-200명 (피크 시간 기준 70% 활성)
- 응답 시간: 일반 조회 < 1초, 복잡한 리포트 < 5초
- 데이터 규모: 
  - 프로젝트: 연간 50-200개
  - 시간 기록: 일일 1,000-4,000건
  - 인보이스: 월 100-500건

#### **확장성 요구사항**
- 다중 테넌트 지원 (SaaS 모델)
- 수평적 확장 가능한 아키텍처
- 클라우드 배포 지원

### 1.4 현재 DB 설계의 강점

1. **포괄적인 도메인 커버리지**
   - IT 서비스업의 핵심 업무 프로세스를 모두 포함
   - 프로젝트 중심의 데이터 모델링

2. **유연한 데이터 구조**
   - JSON 타입 활용 (audit_logs)
   - ENUM 타입으로 표준화된 상태 관리
   - 기술 스택 및 스킬 관리를 위한 별도 테이블

3. **성능 최적화 고려**
   - 적절한 인덱스 설계
   - 집계 뷰 (project_profitability, employee_utilization)
   - UUID 사용으로 분산 환경 대응

### 1.5 개선이 필요한 영역

#### **기능적 개선사항**
1. **보고서 및 대시보드**
   - 경영진 대시보드를 위한 추가 집계 뷰 필요
   - 프로젝트 건전성 지표 관리

2. **알림 및 워크플로우**
   - 승인 프로세스 관리 테이블 부재
   - 알림 설정 및 이력 관리 필요

3. **문서 관리**
   - 프로젝트 문서, 계약서 관리 테이블 필요
   - 파일 첨부 기능 확장

#### **기술적 개선사항**
1. **다중 테넌트 지원**
   - tenant_id 컬럼 추가 및 파티셔닝 전략
   - 테넌트별 데이터 격리 및 보안

2. **캐싱 전략**
   - 자주 조회되는 데이터 식별
   - Redis 등 캐시 레이어 도입 고려

3. **API 설계 고려사항**
   - RESTful API 엔드포인트 매핑
   - GraphQL 도입 검토 (복잡한 조회용)

### 1.6 비즈니스 가치 제안

#### **핵심 차별화 요소**
1. **IT 서비스업 특화 기능**
   - 기술 스택 기반 리소스 매칭
   - 프로젝트 타입별 수익성 분석
   - 개발자 스킬 매트릭스 관리

2. **실시간 인사이트**
   - 프로젝트 수익성 실시간 모니터링
   - 직원 활용률 대시보드
   - 예측 분석 (프로젝트 지연, 예산 초과 위험)

3. **자동화**
   - 시간 기록 기반 자동 인보이스 생성
   - 리소스 충돌 감지 및 알림
   - 승인 워크플로우 자동화

### 1.7 리스크 분석

1. **기술적 리스크**
   - MySQL의 다중 테넌트 확장성 한계
   - 복잡한 집계 쿼리의 성능 이슈

2. **비즈니스 리스크**
   - 기존 ERP 솔루션과의 경쟁
   - 고객사별 커스터마이징 요구

3. **보안 리스크**
   - 다중 테넌트 환경에서의 데이터 격리
   - 민감한 재무 정보 보호

## 2. 아키텍처 설계

### 2.1 전체적인 아키텍처 개요

#### **시스템 아키텍처 패턴**
- **Clean Architecture** + **Domain-Driven Design (DDD)** 원칙 적용
- **Microservices-Ready Monolith** 구조 (향후 마이크로서비스 전환 가능)
- **Multi-Tenant SaaS** 아키텍처

#### **주요 아키텍처 구성**

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │   Web UI    │  │  Mobile App  │  │   API Gateway   │   │
│  │  (Blazor)   │  │    (MAUI)    │  │   (Ocelot)      │   │
│  └─────────────┘  └──────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              ASP.NET Core Web API                    │   │
│  │  ┌───────────┐  ┌───────────┐  ┌────────────────┐  │   │
│  │  │Controllers│  │ MediatR   │  │  FluentValid.  │  │   │
│  │  └───────────┘  └───────────┘  └────────────────┘  │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐    │
│  │   Entities   │  │ Value Objects│  │Domain Services│    │
│  │              │  │              │  │               │    │
│  └──────────────┘  └──────────────┘  └───────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                          │
│  ┌────────────┐  ┌──────────────┐  ┌─────────────────┐    │
│  │ EF Core 8  │  │Redis Cache   │  │ Azure Services  │    │
│  │ MySQL      │  │              │  │ (Blob, Queue)   │    │
│  └────────────┘  └──────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### **핵심 아키텍처 결정사항**

1. **API-First Design**
   - 모든 기능은 REST API로 노출
   - OpenAPI (Swagger) 문서 자동 생성
   - GraphQL 엔드포인트 (복잡한 조회용)

2. **Event-Driven Architecture**
   - Domain Events를 통한 느슨한 결합
   - 이벤트 소싱 준비 (향후 확장)
   - 비동기 처리를 위한 메시지 큐

3. **Multi-Tenancy Strategy**
   - Database-per-tenant (격리성 우선)
   - Shared Database with Row-Level Security (비용 효율)
   - Hybrid 접근 (대형 고객은 별도 DB)

4. **보안 아키텍처**
   - JWT 기반 인증
   - OAuth 2.0 / OpenID Connect
   - API Rate Limiting
   - 테넌트별 데이터 격리

### 2.2 세부적인 아키텍처 설계

#### **2.2.1 Backend (C#/.NET 8)**

**프로젝트 구조**
```
ERP.Solution/
├── src/
│   ├── ERP.Domain/                 # 도메인 계층
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Interfaces/
│   │   └── Events/
│   │
│   ├── ERP.Application/            # 애플리케이션 계층
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Services/
│   │
│   ├── ERP.Infrastructure/         # 인프라 계층
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   ├── Repositories/
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   ├── Caching/
│   │   └── External/
│   │
│   ├── ERP.API/                    # 프레젠테이션 계층
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   └── GraphQL/
│   │
│   └── ERP.SharedKernel/           # 공통 코드
│       ├── Abstractions/
│       └── Utilities/
```

**주요 기술 스택**
- **.NET 8** + **C# 12**
- **Entity Framework Core 8** (ORM)
- **MediatR** (CQRS 패턴)
- **FluentValidation** (유효성 검증)
- **AutoMapper** (객체 매핑)
- **Serilog** (구조화된 로깅)
- **Polly** (복원력 패턴)
- **HotChocolate** (GraphQL)

**도메인 모델 예시**
```csharp
// Domain Layer
public class Project : AggregateRoot
{
    public ProjectId Id { get; private set; }
    public ClientId ClientId { get; private set; }
    public ProjectCode Code { get; private set; }
    public ProjectStatus Status { get; private set; }
    public Money Budget { get; private set; }
    public DateRange Period { get; private set; }
    
    private readonly List<ProjectAssignment> _assignments;
    private readonly List<Milestone> _milestones;
    
    // Domain logic here...
}

// Application Layer - CQRS
public class CreateProjectCommand : IRequest<ProjectDto>
{
    public string Name { get; set; }
    public Guid ClientId { get; set; }
    public decimal Budget { get; set; }
}

// Infrastructure Layer - EF Configuration
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        // Multi-tenant support
        builder.HasQueryFilter(p => p.TenantId == _currentTenantId);
    }
}
```

#### **2.2.2 Frontend (ASP.NET Blazor)**

**프로젝트 구조**
```
ERP.BlazorWeb/
├── Pages/               # 페이지 컴포넌트
│   ├── Dashboard/
│   ├── Projects/
│   ├── Employees/
│   ├── Invoices/
│   └── Reports/
│
├── Components/          # 재사용 가능한 컴포넌트
│   ├── Common/
│   ├── Forms/
│   └── Charts/
│
├── Shared/             # 공유 레이아웃
│   ├── MainLayout.razor
│   └── NavMenu.razor
│
├── Services/           # API 통신 서비스
│   ├── IProjectService.cs
│   ├── IEmployeeService.cs
│   └── IAuthService.cs
│
├── Models/             # DTO 및 ViewModels
│   ├── DTOs/
│   └── ViewModels/
│
├── State/              # 상태 관리 (Fluxor)
│   ├── ProjectState/
│   └── AppState/
│
└── wwwroot/           # 정적 파일
    ├── css/
    └── js/
```

#### **2.2.3 Database (MySQL)**

**Multi-Tenant 스키마 전략**
```sql
-- 1. Shared Database with Tenant Column
ALTER TABLE projects ADD COLUMN tenant_id CHAR(36) NOT NULL;
ALTER TABLE projects ADD INDEX idx_tenant (tenant_id);

-- 2. Row Level Security View
CREATE VIEW tenant_projects AS
SELECT * FROM projects 
WHERE tenant_id = GET_CURRENT_TENANT_ID();

-- 3. Partitioning for Performance
ALTER TABLE time_entries 
PARTITION BY RANGE (YEAR(date)) (
    PARTITION p2024 VALUES LESS THAN (2025),
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027)
);
```

**성능 최적화**
```sql
-- Materialized View for Dashboard
CREATE TABLE dashboard_metrics (
    tenant_id CHAR(36),
    metric_date DATE,
    active_projects INT,
    total_revenue DECIMAL(15,2),
    billable_hours DECIMAL(10,2),
    employee_utilization DECIMAL(5,2),
    updated_at TIMESTAMP,
    PRIMARY KEY (tenant_id, metric_date)
);

-- Stored Procedure for Complex Calculations
DELIMITER //
CREATE PROCEDURE CalculateProjectProfitability(
    IN p_tenant_id CHAR(36),
    IN p_project_id CHAR(36)
)
BEGIN
    -- Complex profitability calculation
END //
DELIMITER ;
```

#### **2.2.4 Infrastructure as Code (Terraform)**

```hcl
# Azure Infrastructure
resource "azurerm_mysql_flexible_server" "main" {
  name                = "erp-mysql-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  
  sku_name = "B_Standard_B2s"
  version  = "8.0.21"
  
  high_availability {
    mode = "ZoneRedundant"
  }
}

resource "azurerm_container_group" "api" {
  name                = "erp-api-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  
  container {
    name   = "api"
    image  = "erpacr.azurecr.io/erp-api:${var.api_version}"
    cpu    = "2"
    memory = "4"
    
    environment_variables = {
      "ConnectionStrings__Default" = azurerm_mysql_flexible_server.main.connection_string
      "Redis__ConnectionString"    = azurerm_redis_cache.main.connection_string
    }
  }
}
```

#### **2.2.5 DevOps (GitHub Actions)**

```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
          
      - name: Build and Test
        run: |
          dotnet build
          dotnet test
          
      - name: Build Docker Image
        run: |
          docker build -t ${{ secrets.ACR_REGISTRY }}/erp-api:${{ github.sha }} .
          docker push ${{ secrets.ACR_REGISTRY }}/erp-api:${{ github.sha }}
          
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: erp-api-prod
          images: ${{ secrets.ACR_REGISTRY }}/erp-api:${{ github.sha }}
```

#### **2.2.6 Monitoring & Observability**

**Application Insights 통합**
```csharp
// Startup configuration
services.AddApplicationInsightsTelemetry();
services.AddSingleton<ITelemetryInitializer, TenantTelemetryInitializer>();

// Custom metrics
public class ProjectMetricsService
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackProjectCreated(Project project)
    {
        _telemetryClient.TrackEvent("ProjectCreated", new Dictionary<string, string>
        {
            ["TenantId"] = project.TenantId,
            ["ProjectType"] = project.Type.ToString(),
            ["Budget"] = project.Budget.ToString()
        });
    }
}
```


```
C:.
│  .editorconfig
│  .gitignore
│  TechFlowERP.sln
│
└─src
    ├─ERP.API
    │  │  appsettings.Development.json
    │  │  appsettings.json
    │  │  ERP.API.csproj
    │  │  ERP.API.http
    │  │  Program.cs
    │  │
    │  ├─bin
    │  │  └─Debug
    │  │      └─net8.0
    │  │              appsettings.Development.json
    │  │              appsettings.json
    │  │              AutoMapper.dll
    │  │              AutoMapper.Extensions.Microsoft.DependencyInjection.dll
    │  │              ERP.API.deps.json
    │  │              ERP.API.dll
    │  │              ERP.API.exe
    │  │              ERP.API.pdb
    │  │              ERP.API.runtimeconfig.json
    │  │              ERP.API.staticwebassets.endpoints.json
    │  │              ERP.Application.dll
    │  │              ERP.Application.pdb
    │  │              ERP.Domain.dll
    │  │              ERP.Domain.pdb
    │  │              ERP.Infrastructure.dll
    │  │              ERP.Infrastructure.pdb
    │  │              ERP.SharedKernel.dll
    │  │              ERP.SharedKernel.pdb
    │  │              FluentValidation.DependencyInjectionExtensions.dll
    │  │              FluentValidation.dll
    │  │              MediatR.Contracts.dll
    │  │              MediatR.dll
    │  │              Microsoft.AspNetCore.Authentication.JwtBearer.dll
    │  │              Microsoft.AspNetCore.OpenApi.dll
    │  │              Microsoft.EntityFrameworkCore.Abstractions.dll
    │  │              Microsoft.EntityFrameworkCore.dll
    │  │              Microsoft.EntityFrameworkCore.Relational.dll
    │  │              Microsoft.Extensions.Caching.StackExchangeRedis.dll
    │  │              Microsoft.Extensions.DependencyModel.dll
    │  │              Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore.dll
    │  │              Microsoft.IdentityModel.Abstractions.dll
    │  │              Microsoft.IdentityModel.JsonWebTokens.dll
    │  │              Microsoft.IdentityModel.Logging.dll
    │  │              Microsoft.IdentityModel.Protocols.dll
    │  │              Microsoft.IdentityModel.Protocols.OpenIdConnect.dll
    │  │              Microsoft.IdentityModel.Tokens.dll
    │  │              Microsoft.OpenApi.dll
    │  │              MySqlConnector.dll
    │  │              Pipelines.Sockets.Unofficial.dll
    │  │              Pomelo.EntityFrameworkCore.MySql.dll
    │  │              Serilog.AspNetCore.dll
    │  │              Serilog.dll
    │  │              Serilog.Extensions.Hosting.dll
    │  │              Serilog.Extensions.Logging.dll
    │  │              Serilog.Formatting.Compact.dll
    │  │              Serilog.Settings.Configuration.dll
    │  │              Serilog.Sinks.Console.dll
    │  │              Serilog.Sinks.Debug.dll
    │  │              Serilog.Sinks.File.dll
    │  │              StackExchange.Redis.dll
    │  │              Swashbuckle.AspNetCore.Swagger.dll
    │  │              Swashbuckle.AspNetCore.SwaggerGen.dll
    │  │              Swashbuckle.AspNetCore.SwaggerUI.dll
    │  │              System.IdentityModel.Tokens.Jwt.dll
    │  │
    │  ├─obj
    │  │  │  ERP.API.csproj.nuget.dgspec.json
    │  │  │  ERP.API.csproj.nuget.g.props
    │  │  │  ERP.API.csproj.nuget.g.targets
    │  │  │  project.assets.json
    │  │  │  project.nuget.cache
    │  │  │
    │  │  └─Debug
    │  │      └─net8.0
    │  │          │  .NETCoreApp,Version=v8.0.AssemblyAttributes.cs
    │  │          │  apphost.exe
    │  │          │  ERP.API.AssemblyInfo.cs
    │  │          │  ERP.API.AssemblyInfoInputs.cache
    │  │          │  ERP.API.assets.cache
    │  │          │  ERP.API.csproj.AssemblyReference.cache
    │  │          │  ERP.API.csproj.CoreCompileInputs.cache
    │  │          │  ERP.API.csproj.FileListAbsolute.txt
    │  │          │  ERP.API.csproj.Up2Date
    │  │          │  ERP.API.dll
    │  │          │  ERP.API.GeneratedMSBuildEditorConfig.editorconfig
    │  │          │  ERP.API.genruntimeconfig.cache
    │  │          │  ERP.API.GlobalUsings.g.cs
    │  │          │  ERP.API.MvcApplicationPartsAssemblyInfo.cache
    │  │          │  ERP.API.MvcApplicationPartsAssemblyInfo.cs
    │  │          │  ERP.API.pdb
    │  │          │  rjsmcshtml.dswa.cache.json
    │  │          │  rjsmrazor.dswa.cache.json
    │  │          │  rpswa.dswa.cache.json
    │  │          │  staticwebassets.build.endpoints.json
    │  │          │  staticwebassets.build.json
    │  │          │  staticwebassets.build.json.cache
    │  │          │
    │  │          ├─ref
    │  │          │      ERP.API.dll
    │  │          │
    │  │          ├─refint
    │  │          │      ERP.API.dll
    │  │          │
    │  │          └─staticwebassets
    │  └─Properties
    │          launchSettings.json
    │
    ├─ERP.Application
    │  │  ERP.Application.csproj
    │  │
    │  ├─bin
    │  │  └─Debug
    │  │      └─net8.0
    │  │              ERP.Application.deps.json
    │  │              ERP.Application.dll
    │  │              ERP.Application.pdb
    │  │              ERP.Domain.dll
    │  │              ERP.Domain.pdb
    │  │              ERP.SharedKernel.dll
    │  │              ERP.SharedKernel.pdb
    │  │
    │  └─obj
    │      │  ERP.Application.csproj.nuget.dgspec.json
    │      │  ERP.Application.csproj.nuget.g.props
    │      │  ERP.Application.csproj.nuget.g.targets
    │      │  project.assets.json
    │      │  project.nuget.cache
    │      │
    │      └─Debug
    │          └─net8.0
    │              │  .NETCoreApp,Version=v8.0.AssemblyAttributes.cs
    │              │  ERP.Appl.DD782D7E.Up2Date
    │              │  ERP.Application.AssemblyInfo.cs
    │              │  ERP.Application.AssemblyInfoInputs.cache
    │              │  ERP.Application.assets.cache
    │              │  ERP.Application.csproj.AssemblyReference.cache
    │              │  ERP.Application.csproj.CoreCompileInputs.cache
    │              │  ERP.Application.csproj.FileListAbsolute.txt
    │              │  ERP.Application.dll
    │              │  ERP.Application.GeneratedMSBuildEditorConfig.editorconfig
    │              │  ERP.Application.GlobalUsings.g.cs
    │              │  ERP.Application.pdb
    │              │
    │              ├─ref
    │              │      ERP.Application.dll
    │              │
    │              └─refint
    │                      ERP.Application.dll
    │
    ├─ERP.Domain
    │  │  ERP.Domain.csproj
    │  │
    │  ├─bin
    │  │  └─Debug
    │  │      └─net8.0
    │  │              ERP.Domain.deps.json
    │  │              ERP.Domain.dll
    │  │              ERP.Domain.pdb
    │  │              ERP.SharedKernel.dll
    │  │              ERP.SharedKernel.pdb
    │  │
    │  └─obj
    │      │  ERP.Domain.csproj.nuget.dgspec.json
    │      │  ERP.Domain.csproj.nuget.g.props
    │      │  ERP.Domain.csproj.nuget.g.targets
    │      │  project.assets.json
    │      │  project.nuget.cache
    │      │
    │      └─Debug
    │          └─net8.0
    │              │  .NETCoreApp,Version=v8.0.AssemblyAttributes.cs
    │              │  ERP.Domain.AssemblyInfo.cs
    │              │  ERP.Domain.AssemblyInfoInputs.cache
    │              │  ERP.Domain.assets.cache
    │              │  ERP.Domain.csproj.AssemblyReference.cache
    │              │  ERP.Domain.csproj.CoreCompileInputs.cache
    │              │  ERP.Domain.csproj.FileListAbsolute.txt
    │              │  ERP.Domain.csproj.Up2Date
    │              │  ERP.Domain.dll
    │              │  ERP.Domain.GeneratedMSBuildEditorConfig.editorconfig
    │              │  ERP.Domain.GlobalUsings.g.cs
    │              │  ERP.Domain.pdb
    │              │
    │              ├─ref
    │              │      ERP.Domain.dll
    │              │
    │              └─refint
    │                      ERP.Domain.dll
    │
    ├─ERP.Infrastructure
    │  │  ERP.Infrastructure.csproj
    │  │
    │  ├─bin
    │  │  └─Debug
    │  │      └─net8.0
    │  │              ERP.Application.dll
    │  │              ERP.Application.pdb
    │  │              ERP.Domain.dll
    │  │              ERP.Domain.pdb
    │  │              ERP.Infrastructure.deps.json
    │  │              ERP.Infrastructure.dll
    │  │              ERP.Infrastructure.pdb
    │  │              ERP.Infrastructure.runtimeconfig.json
    │  │              ERP.SharedKernel.dll
    │  │              ERP.SharedKernel.pdb
    │  │
    │  └─obj
    │      │  ERP.Infrastructure.csproj.nuget.dgspec.json
    │      │  ERP.Infrastructure.csproj.nuget.g.props
    │      │  ERP.Infrastructure.csproj.nuget.g.targets
    │      │  project.assets.json
    │      │  project.nuget.cache
    │      │
    │      └─Debug
    │          └─net8.0
    │              │  .NETCoreApp,Version=v8.0.AssemblyAttributes.cs
    │              │  ERP.Infr.92BEE6A2.Up2Date
    │              │  ERP.Infrastructure.AssemblyInfo.cs
    │              │  ERP.Infrastructure.AssemblyInfoInputs.cache
    │              │  ERP.Infrastructure.assets.cache
    │              │  ERP.Infrastructure.csproj.AssemblyReference.cache
    │              │  ERP.Infrastructure.csproj.CoreCompileInputs.cache
    │              │  ERP.Infrastructure.csproj.FileListAbsolute.txt
    │              │  ERP.Infrastructure.dll
    │              │  ERP.Infrastructure.GeneratedMSBuildEditorConfig.editorconfig
    │              │  ERP.Infrastructure.genruntimeconfig.cache
    │              │  ERP.Infrastructure.GlobalUsings.g.cs
    │              │  ERP.Infrastructure.pdb
    │              │
    │              ├─ref
    │              │      ERP.Infrastructure.dll
    │              │
    │              └─refint
    │                      ERP.Infrastructure.dll
    │
    └─ERP.SharedKernel
        │  ERP.SharedKernel.csproj
        │
        ├─bin
        │  └─Debug
        │      └─net8.0
        │              ERP.SharedKernel.deps.json
        │              ERP.SharedKernel.dll
        │              ERP.SharedKernel.pdb
        │
        └─obj
            │  ERP.SharedKernel.csproj.nuget.dgspec.json
            │  ERP.SharedKernel.csproj.nuget.g.props
            │  ERP.SharedKernel.csproj.nuget.g.targets
            │  project.assets.json
            │  project.nuget.cache
            │
            └─Debug
                └─net8.0
                    │  .NETCoreApp,Version=v8.0.AssemblyAttributes.cs
                    │  ERP.SharedKernel.AssemblyInfo.cs
                    │  ERP.SharedKernel.AssemblyInfoInputs.cache
                    │  ERP.SharedKernel.assets.cache
                    │  ERP.SharedKernel.csproj.AssemblyReference.cache
                    │  ERP.SharedKernel.csproj.CoreCompileInputs.cache
                    │  ERP.SharedKernel.csproj.FileListAbsolute.txt
                    │  ERP.SharedKernel.dll
                    │  ERP.SharedKernel.GeneratedMSBuildEditorConfig.editorconfig
                    │  ERP.SharedKernel.GlobalUsings.g.cs
                    │  ERP.SharedKernel.pdb
                    │
                    ├─ref
                    │      ERP.SharedKernel.dll
                    │
                    └─refint
                            ERP.SharedKernel.dll
```


# IT 서비스업 특화 ERP 시스템 데이터베이스 스키마

## 목차
1. [개요](#개요)
2. [클라이언트 관리](#1-클라이언트-관리)
3. [조직 구조](#2-조직-구조)
4. [직원 관리](#3-직원-관리)
5. [프로젝트 관리](#4-프로젝트-관리)
6. [리소스 관리](#5-리소스-관리)
7. [재무 관리](#6-재무-관리)
8. [보안 및 감사](#7-보안-및-감사)
9. [추가 기능](#8-추가-기능)
10. [유용한 뷰](#9-유용한-뷰)

## 개요

이 문서는 50-200명 규모의 IT 개발/서비스 회사를 위한 ERP 시스템 데이터베이스 스키마를 정의합니다.

### 주요 특징
- **타겟**: IT 서비스업 특화
- **규모**: 50-200명 규모 기업
- **모델**: SaaS 구독 서비스
- **기능**: 프로젝트 관리, 리소스 관리, 시간 추적, 재무 관리

---

## 1. 클라이언트 관리

고객사 정보를 관리하는 테이블입니다.

```sql
CREATE TABLE clients (
    client_id CHAR(36) PRIMARY KEY,
    company_name VARCHAR(200) NOT NULL,
    industry VARCHAR(100),
    size_category ENUM('Small','Medium','Large'),
    contact_person VARCHAR(100),
    contact_email VARCHAR(100),
    contact_phone VARCHAR(50),
    address TEXT,
    contract_value DECIMAL(15,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_company_name (company_name),
    INDEX idx_industry (industry)
);
```

### 필드 설명
- `client_id`: 고객사 고유 ID (UUID)
- `company_name`: 회사명
- `industry`: 산업 분야
- `size_category`: 회사 규모 분류
- `contract_value`: 총 계약 금액

---

## 2. 조직 구조

### 부서 테이블

```sql
CREATE TABLE departments (
    department_id CHAR(36) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    manager_id CHAR(36),
    budget DECIMAL(15,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

### 직급 테이블

```sql
CREATE TABLE positions (
    position_id CHAR(36) PRIMARY KEY,
    title VARCHAR(100) NOT NULL,
    level INT,
    min_salary DECIMAL(10,2),
    max_salary DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## 3. 직원 관리

### 직원 정보 테이블

```sql
CREATE TABLE employees (
    employee_id CHAR(36) PRIMARY KEY,
    email VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    department_id CHAR(36),
    position_id CHAR(36),
    manager_id CHAR(36),
    hire_date DATE NOT NULL,
    salary DECIMAL(10,2),
    leave_balance INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (department_id) REFERENCES departments(department_id),
    FOREIGN KEY (position_id) REFERENCES positions(position_id),
    FOREIGN KEY (manager_id) REFERENCES employees(employee_id),
    INDEX idx_department (department_id),
    INDEX idx_active (is_active),
    INDEX idx_manager (manager_id)
);
```

### 직원 기술 테이블

```sql
CREATE TABLE employee_skills (
    skill_id CHAR(36) PRIMARY KEY,
    employee_id CHAR(36) NOT NULL,
    technology VARCHAR(50) NOT NULL,
    skill_level ENUM('Beginner','Intermediate','Expert'),
    years_experience INT,
    last_used_date DATE,
    certification VARCHAR(200),
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id),
    INDEX idx_employee_tech (employee_id, technology),
    INDEX idx_tech_level (technology, skill_level)
);
```

---

## 4. 프로젝트 관리

### 프로젝트 테이블

```sql
CREATE TABLE projects (
    project_id CHAR(36) PRIMARY KEY,
    project_code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(200) NOT NULL,
    client_id CHAR(36) NOT NULL,
    project_type ENUM('Fixed','TimeAndMaterial','Retainer') DEFAULT 'TimeAndMaterial',
    start_date DATE NOT NULL,
    end_date DATE,
    budget DECIMAL(15,2),
    profit_margin DECIMAL(5,2),
    risk_level ENUM('Low','Medium','High') DEFAULT 'Medium',
    status ENUM('Planning','Active','OnHold','Completed','Cancelled'),
    manager_id CHAR(36) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (client_id) REFERENCES clients(client_id),
    FOREIGN KEY (manager_id) REFERENCES employees(employee_id),
    INDEX idx_client (client_id),
    INDEX idx_status (status),
    INDEX idx_dates (start_date, end_date),
    INDEX idx_project_code (project_code)
);

ALTER TABLE projects ADD COLUMN description TEXT;
ALTER TABLE projects ADD COLUMN technologies JSON;

```

### 프로젝트 기술 스택

```sql
CREATE TABLE project_technologies (
    project_id CHAR(36) NOT NULL,
    technology VARCHAR(50) NOT NULL,
    version VARCHAR(20),
    PRIMARY KEY (project_id, technology),
    FOREIGN KEY (project_id) REFERENCES projects(project_id)
);
```

### 프로젝트 마일스톤

```sql
CREATE TABLE project_milestones (
    milestone_id CHAR(36) PRIMARY KEY,
    project_id CHAR(36) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    due_date DATE NOT NULL,
    completion_date DATE,
    deliverables TEXT,
    payment_percentage DECIMAL(5,2),
    status ENUM('Pending','InProgress','Completed','Delayed') DEFAULT 'Pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(project_id),
    INDEX idx_project_due (project_id, due_date)
);
```

---

## 5. 리소스 관리

### 프로젝트 할당

```sql
CREATE TABLE project_assignments (
    assignment_id CHAR(36) PRIMARY KEY,
    project_id CHAR(36) NOT NULL,
    employee_id CHAR(36) NOT NULL,
    role VARCHAR(100),
    allocation_percentage INT DEFAULT 100,
    start_date DATE NOT NULL,
    end_date DATE,
    hourly_rate DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(project_id),
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id),
    INDEX idx_employee_period (employee_id, start_date, end_date),
    INDEX idx_project_employee (project_id, employee_id),
    UNIQUE KEY unique_assignment (project_id, employee_id, start_date)
);
```

### 시간 기록

```sql
CREATE TABLE time_entries (
    entry_id CHAR(36) PRIMARY KEY,
    employee_id CHAR(36) NOT NULL,
    project_id CHAR(36) NOT NULL,
    task_description TEXT,
    hours DECIMAL(4,2) NOT NULL,
    date DATE NOT NULL,
    billable BOOLEAN DEFAULT TRUE,
    approved BOOLEAN DEFAULT FALSE,
    approved_by CHAR(36),
    approved_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id),
    FOREIGN KEY (project_id) REFERENCES projects(project_id),
    FOREIGN KEY (approved_by) REFERENCES employees(employee_id),
    INDEX idx_employee_date (employee_id, date),
    INDEX idx_project_date (project_id, date),
    INDEX idx_billable_approved (billable, approved)
);
```

---

## 6. 재무 관리

### 인보이스 테이블

```sql
CREATE TABLE invoices (
    invoice_id CHAR(36) PRIMARY KEY,
    invoice_number VARCHAR(50) UNIQUE NOT NULL,
    project_id CHAR(36) NOT NULL,
    client_id CHAR(36) NOT NULL,
    amount DECIMAL(15,2) NOT NULL,
    tax_amount DECIMAL(15,2) DEFAULT 0,
    currency CHAR(3) DEFAULT 'USD',
    due_date DATE NOT NULL,
    payment_date DATE,
    payment_method ENUM('BankTransfer','CreditCard','Check','Cash','Other'),
    status ENUM('Draft','Sent','Paid','Overdue','Cancelled'),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(project_id),
    FOREIGN KEY (client_id) REFERENCES clients(client_id),
    INDEX idx_project (project_id),
    INDEX idx_status_due (status, due_date),
    INDEX idx_client_status (client_id, status)
);
```

---

## 7. 보안 및 감사

### 사용자 계정

```sql
CREATE TABLE users (
    user_id CHAR(36) PRIMARY KEY,
    employee_id CHAR(36) UNIQUE,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('Admin','Manager','Employee','Finance','HR','ReadOnly') NOT NULL,
    last_login TIMESTAMP NULL,
    is_locked BOOLEAN DEFAULT FALSE,
    failed_attempts INT DEFAULT 0,
    password_changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id),
    INDEX idx_username (username),
    INDEX idx_role (role)
);
```

### 감사 로그

```sql
CREATE TABLE audit_logs (
    log_id CHAR(36) PRIMARY KEY,
    user_id CHAR(36),
    action VARCHAR(100) NOT NULL,
    table_name VARCHAR(50),
    record_id CHAR(36),
    old_values JSON,
    new_values JSON,
    ip_address VARCHAR(45),
    user_agent VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_user_time (user_id, created_at),
    INDEX idx_table_record (table_name, record_id),
    INDEX idx_action (action),
    INDEX idx_created_at (created_at)
);
```

---

## 8. 추가 기능

### 프로젝트 비용

```sql
CREATE TABLE project_expenses (
    expense_id CHAR(36) PRIMARY KEY,
    project_id CHAR(36) NOT NULL,
    employee_id CHAR(36),
    category VARCHAR(100),
    description TEXT,
    amount DECIMAL(10,2) NOT NULL,
    expense_date DATE NOT NULL,
    approved BOOLEAN DEFAULT FALSE,
    receipt_url VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(project_id),
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id),
    INDEX idx_project_date (project_id, expense_date)
);
```

### 휴가 관리

```sql
CREATE TABLE leave_requests (
    request_id CHAR(36) PRIMARY KEY,
    employee_id CHAR(36) NOT NULL,
    leave_type ENUM('Annual','Sick','Personal','Unpaid') NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    days INT NOT NULL,
    reason TEXT,
    status ENUM('Pending','Approved','Rejected','Cancelled') DEFAULT 'Pending',
    approved_by CHAR(36),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id),
    FOREIGN KEY (approved_by) REFERENCES employees(employee_id),
    INDEX idx_employee_dates (employee_id, start_date, end_date),
    INDEX idx_status (status)
);
```

---

## 9. 유용한 뷰

### 프로젝트 수익성 분석

```sql
CREATE VIEW project_profitability AS
SELECT 
    p.project_id,
    p.project_code,
    p.name as project_name,
    c.company_name as client_name,
    p.budget,
    COALESCE(SUM(te.hours * pa.hourly_rate), 0) as total_cost,
    p.budget - COALESCE(SUM(te.hours * pa.hourly_rate), 0) as profit,
    CASE 
        WHEN p.budget > 0 THEN 
            ((p.budget - COALESCE(SUM(te.hours * pa.hourly_rate), 0)) / p.budget * 100)
        ELSE 0 
    END as profit_percentage
FROM projects p
LEFT JOIN clients c ON p.client_id = c.client_id
LEFT JOIN time_entries te ON p.project_id = te.project_id
LEFT JOIN project_assignments pa ON te.project_id = pa.project_id 
    AND te.employee_id = pa.employee_id
    AND te.date BETWEEN pa.start_date AND COALESCE(pa.end_date, CURDATE())
GROUP BY p.project_id;
```

### 직원 활용도 분석

```sql
CREATE VIEW employee_utilization AS
SELECT 
    e.employee_id,
    e.name,
    YEAR(te.date) as year,
    MONTH(te.date) as month,
    SUM(te.hours) as total_hours,
    SUM(CASE WHEN te.billable = TRUE THEN te.hours ELSE 0 END) as billable_hours,
    (SUM(CASE WHEN te.billable = TRUE THEN te.hours ELSE 0 END) / SUM(te.hours) * 100) as billable_percentage
FROM employees e
LEFT JOIN time_entries te ON e.employee_id = te.employee_id
WHERE e.is_active = TRUE
GROUP BY e.employee_id, YEAR(te.date), MONTH(te.date);
```
## 추가
```sql
-- 모든 주요 테이블에 tenant_id 컬럼 추가
ALTER TABLE projects ADD COLUMN tenant_id VARCHAR(36) NOT NULL DEFAULT 'default-tenant';
ALTER TABLE clients ADD COLUMN tenant_id VARCHAR(36) NOT NULL DEFAULT 'default-tenant';
ALTER TABLE employees ADD COLUMN tenant_id VARCHAR(36) NOT NULL DEFAULT 'default-tenant';
ALTER TABLE users ADD COLUMN tenant_id VARCHAR(36) NOT NULL DEFAULT 'default-tenant';

-- 다른 테이블들도 필요시 추가
ALTER TABLE time_entries ADD COLUMN tenant_id VARCHAR(36) NOT NULL DEFAULT 'default-tenant';
ALTER TABLE project_assignments ADD COLUMN tenant_id VARCHAR(36) NOT NULL DEFAULT 'default-tenant';

-- 인덱스 추가 (성능 향상)
ALTER TABLE projects ADD INDEX idx_tenant (tenant_id);
ALTER TABLE clients ADD INDEX idx_tenant (tenant_id);
ALTER TABLE employees ADD INDEX idx_tenant (tenant_id);
ALTER TABLE users ADD INDEX idx_tenant (tenant_id);

```


---

## 다중 테넌트 지원 (SaaS)

SaaS 모델을 위해 모든 테이블에 `tenant_id`를 추가해야 합니다.

```sql
-- 예시
ALTER TABLE projects ADD COLUMN tenant_id CHAR(36) NOT NULL;
ALTER TABLE projects ADD INDEX idx_tenant (tenant_id);
```

---

## 보안 고려사항

1. **데이터 암호화**: 민감한 정보(급여, 개인정보)는 암호화 저장
2. **접근 제어**: 역할 기반 접근 제어(RBAC) 구현
3. **감사 추적**: 모든 중요한 변경사항은 audit_logs에 기록
4. **비밀번호 정책**: 강력한 비밀번호 정책 적용

---

## 성능 최적화

1. **인덱싱**: 자주 조회되는 컬럼에 인덱스 생성
2. **파티셔닝**: 대용량 테이블(time_entries, audit_logs)은 날짜 기준 파티셔닝 고려
3. **쿼리 최적화**: 복잡한 집계는 뷰나 저장 프로시저 활용
4. **캐싱**: 자주 조회되는 데이터는 캐싱 적용
