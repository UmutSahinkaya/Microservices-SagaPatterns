# Microservices Saga Patterns - E-Ticaret Sipariş Yönetim Sistemi

Bu proje, **Saga Pattern** kullanarak mikroservis mimarisinde dağıtık işlemlerin yönetimini gösteren kapsamlı bir e-ticaret sipariş yönetim sistemidir. Proje, ASP.NET Core 8.0, MassTransit, RabbitMQ, Entity Framework Core ve MongoDB teknolojileri kullanılarak geliştirilmiştir.

## 🏗️ Proje Mimarisi

### Saga Pattern Nedir?

Saga Pattern, mikroservislerde dağıtık işlemleri (distributed transactions) yönetmek için kullanılan bir tasarım desenidir. Bu desen, uzun süreli işlemleri daha küçük, bağımsız adımlara böler ve her adımın başarısızlık durumunda geri alınabilmesini (compensating actions) sağlar.

### Mimari Diyagram

```
┌─────────────┐    ┌──────────────────┐    ┌─────────────┐
│   Client    │───▶│   Order.API      │───▶│ Order DB    │
│ (HTTP/REST) │    │  (Port: 7085)    │    │ (SQL Server)│
└─────────────┘    └─有────────────────┘    └─────────────┘
                      │
                      ▼ OrderStartedEvent
              ┌──────────────────┐
              │ SagaStateMachine │◄─── RabbitMQ Events
              │     Service      │
              │  (Orchestrator)  │
              └─────────┬────────┘
                        │
        ┌───────────────┼───────────────┐
        ▼               ▼               ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│ Stock.API   │ │Payment.API  │ │   RabbitMQ  │
│(Port: 7163) │ │(Port: 7193) │ │ (CloudAMQP) │
└──────┬──────┘ └─────────────┘ └─────────────┘
       ▼
┌─────────────┐
│ MongoDB     │
│ (Stock DB)  │
└─────────────┘
```

### Proje Yapısı

```
Microservices-SagaPatterns/
├── Order.API/                 # Sipariş yönetimi mikroservisi
├── Stock.API/                 # Stok yönetimi mikroservisi  
├── Payment.API/               # Ödeme işlemleri mikroservisi
├── SagaStateMachine.Service/  # Saga orkestratör servisi
└── Shared/                    # Ortak kullanılan mesajlar ve modeller
```

## 🎯 İş Akışı (Business Flow)

### Saga İşlem Akışı Diyagramı

```
Sipariş Oluşturma İsteği
         │
         ▼
    ┌─────────┐      OrderStartedEvent     ┌─────────────────┐
    │Order.API│─────────────────────────▶│SagaStateMachine │
    └─────────┘                          └─────────┬───────┘
         │                                         │
         ▼                                         ▼
   Sipariş veritabanına                   OrderCreatedEvent
      kaydedilir                                   │
                                                   ▼
                                           ┌─────────────┐
                                           │ Stock.API   │
                                           └──────┬──────┘
                                                  │
                                    Stok kontrolü ve rezervasyon
                                                  │
                              ┌───────────────────┴───────────────────┐
                              ▼                                       ▼
                    StockReservedEvent                     StockNotReservedEvent
                              │                                       │
                              ▼                                       ▼
                    ┌─────────────────┐                     ┌─────────────────┐
                    │SagaStateMachine │                     │SagaStateMachine │
                    └─────────┬───────┘                     └─────────┬───────┘
                              │                                       │
                              ▼                                       ▼
                   PaymentStartedEvent                        OrderFailedEvent
                              │                                       │
                              ▼                                       ▼
                    ┌─────────────┐                            ┌─────────┐
                    │Payment.API  │                            │Order.API│
                    └──────┬──────┘                            └─────────┘
                           │                                        │
                    Ödeme işlemi                            Sipariş başarısız
                           │                                 olarak işaretlenir
            ┌──────────────┴──────────────┐
            ▼                             ▼
  PaymentCompletedEvent          PaymentFailedEvent
            │                             │
            ▼                             ▼
  ┌─────────────────┐             ┌─────────────────┐
  │SagaStateMachine │             │SagaStateMachine │
  └─────────┬───────┘             └─────────┬───────┘
            │                               │
            ▼                               ▼
   OrderCompletedEvent              StockRollbackMessage
            │                               │
            ▼                               ▼
      ┌─────────┐                    ┌─────────────┐
      │Order.API│                    │ Stock.API   │
      └─────────┘                    └─────────────┘
            │                               │
            ▼                               ▼
   Sipariş tamamlandı              Stok rezervasyonu iptal
   olarak işaretlenir                     edilir
```

### Saga State Machine Durumları

- **OrderCreated**: Sipariş oluşturuldu
- **StockReserved**: Stok rezerve edildi
- **StockNotReserved**: Stok rezerve edilemedi
- **PaymentCompleted**: Ödeme tamamlandı
- **PaymentFailed**: Ödeme başarısız
- **OrderCompleted**: Sipariş tamamlandı
- **OrderFailed**: Sipariş başarısız

## 🛠️ Teknolojiler

### Backend Teknolojileri
- **.NET 8.0**: Ana framework
- **ASP.NET Core**: Web API geliştirme
- **MassTransit**: Message broker entegrasyonu ve Saga orkestratörü
- **RabbitMQ**: Mesaj kuyruğu sistemi
- **Entity Framework Core**: ORM (SQL Server için)
- **MongoDB**: NoSQL veritabanı (Stock servisi için)

### Veritabanları
- **SQL Server LocalDB**: Order ve SagaStateMachine servisleri için
- **MongoDB**: Stock servisi için
- **RabbitMQ (CloudAMQP)**: Mesaj kuyruğu servisi

## 📋 Sistem Gereksinimleri

### Zorunlu Gereksinimler
- **.NET 8.0 SDK** - [İndir](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** (LocalDB yeterli) - [İndir](https://www.microsoft.com/sql-server/sql-server-downloads)
- **MongoDB Community Edition** - [İndir](https://www.mongodb.com/try/download/community)
- **Visual Studio 2022** veya **VS Code** - [VS İndir](https://visualstudio.microsoft.com/) | [VS Code İndir](https://code.visualstudio.com/)

### İsteğe Bağlı Araçlar
- **MongoDB Compass** - MongoDB GUI aracı
- **SQL Server Management Studio (SSMS)** - SQL Server yönetimi için
- **Postman** - API testleri için
- **Docker Desktop** - Container tabanlı geliştirme için

### Donanım Gereksinimleri
- **RAM**: Minimum 8GB (16GB önerilen)
- **İşlemci**: 2+ çekirdek
- **Disk Alanı**: Minimum 2GB boş alan
- **İşletim Sistemi**: Windows 10/11, macOS 10.15+, Ubuntu 18.04+

### Port Kullanımı
Aşağıdaki portların boş olduğundan emin olun:
- **5222, 7085** - Order.API
- **5189, 7163** - Stock.API  
- **5101, 7193** - Payment.API
- **27017** - MongoDB
- **1433** - SQL Server LocalDB



## 🚀 Kurulum ve Çalıştırma

### 1. Projeyi Klonlama

```bash
git clone https://github.com/UmutSahinkaya/Microservices-SagaPatterns.git
cd Microservices-SagaPatterns
```

### 2. Çözümü Derleme

```bash
dotnet restore
dotnet build
```

### 3. Veritabanı Migrasyonları

#### Order.API Migrasyonları
```bash
cd Order.API
dotnet ef database update
```

#### SagaStateMachine.Service Migrasyonları
```bash
cd SagaStateMachine.Service
dotnet ef database update
```

### 4. MongoDB Kurulumu

MongoDB'nin yerel makinenizde kurulu olduğundan emin olun. Stock.API servisi otomatik olarak test verileri oluşturacaktır.

### 5. Servisleri Çalıştırma

Her servisi ayrı terminal penceresinde çalıştırın:

#### Order.API Servisi
```bash
cd Order.API
dotnet run
```
- HTTP: `http://localhost:5222`
- HTTPS: `https://localhost:7085`
- Swagger UI: `https://localhost:7085/swagger`

#### Stock.API Servisi
```bash
cd Stock.API
dotnet run
```
- HTTP: `http://localhost:5189`
- HTTPS: `https://localhost:7163`
- Swagger UI: `https://localhost:7163/swagger`

#### Payment.API Servisi
```bash
cd Payment.API
dotnet run
```
- HTTP: `http://localhost:5101`
- HTTPS: `https://localhost:7193`
- Swagger UI: `https://localhost:7193/swagger`

#### SagaStateMachine.Service
```bash
cd SagaStateMachine.Service
dotnet run
```

## 📡 API Kullanımı

### Sipariş Oluşturma

**Endpoint**: `POST /create-order`

**Request Body**:
```json
{
  "buyerId": 1,
  "orderItems": [
    {
      "productId": 1,
      "count": 2,
      "price": 100.50
    },
    {
      "productId": 2,
      "count": 1,
      "price": 50.25
    }
  ]
}
```

**Response**: Sipariş başarıyla oluşturulur ve Saga süreci başlatılır.

### Test Senaryoları

#### Başarılı Sipariş
```bash
curl -X POST "https://localhost:7085/create-order" \
  -H "Content-Type: application/json" \
  -d '{
    "buyerId": 1,
    "orderItems": [
      {
        "productId": 1,
        "count": 5,
        "price": 100.00
      }
    ]
  }'
```

#### Stok Yetersizliği Senaryosu
```bash
curl -X POST "https://localhost:7085/create-order" \
  -H "Content-Type: application/json" \
  -d '{
    "buyerId": 1,
    "orderItems": [
      {
        "productId": 5,
        "count": 10,
        "price": 100.00
      }
    ]
  }'
```

## 🔄 Saga Pattern Detayları

### State Machine Yapısı

```csharp
public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    // Events
    public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
    public Event<StockReservedEvent> StockReservedEvent { get; set; }
    public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
    
    // States
    public State OrderCreated { get; set; }
    public State StockReserved { get; set; }
    public State PaymentCompleted { get; set; }
}
```

### Mesaj Akışı

1. **OrderStartedEvent** → SagaStateMachine
2. **OrderCreatedEvent** → Stock.API
3. **StockReservedEvent/StockNotReservedEvent** → SagaStateMachine  
4. **PaymentStartedEvent** → Payment.API
5. **PaymentCompletedEvent/PaymentFailedEvent** → SagaStateMachine
6. **OrderCompletedEvent/OrderFailedEvent** → Order.API

### Rollback Mekanizması

Saga pattern'in en önemli özelliklerinden biri, başarısızlık durumunda yapılan işlemleri geri alabilmesidir:

- **Ödeme başarısız**: Rezerve edilen stok serbest bırakılır
- **Stok yetersiz**: Sipariş iptal edilir
- **Herhangi bir hata**: Tüm işlemler geri alınır ve sipariş başarısız olarak işaretlenir

## 🗄️ Veritabanı Şemaları

### Order.API - SQL Server

**Orders Tablosu**:
- Id (int, PK)
- BuyerId (int)
- Status (int) - OrderStatus enum
- CreatedDate (datetime2)
- TotalPrice (decimal)

**OrderItems Tablosu**:
- Id (int, PK)
- ProductId (int)
- Count (int)
- Price (decimal)
- OrderId (int, FK)

### SagaStateMachine.Service - SQL Server

**OrderStateInstance Tablosu**:
- CorrelationId (uniqueidentifier, PK)
- CurrentState (nvarchar)
- OrderId (int)
- BuyerId (int)
- TotalPrice (decimal)
- CreatedDate (datetime2)

### Stock.API - MongoDB

**Stock Collection**:
```json
{
  "_id": ObjectId,
  "ProductId": 1,
  "Count": 100
}
```

## 🔧 Yapılandırma

### RabbitMQ Kuyruıkları

```csharp
public static class RabbitMQSettings
{
    public const string StateMachineQueue = "state-machine-queue";
    public const string Stock_OrderCreatedEventQueue = "stock-order-created-event-queue";
    public const string Order_OrderCompletedEventQueue = "order-order-completed-event-queue";
    public const string Order_OrderFailedEventQueue = "order-order-failed-event-queue";
    public const string Stock_RollbackMessageQueue = "stock-rollback-message-queue";
    public const string Payment_StartedEventQueue = "payment-started-event-queue";
}
```

### Bağlantı Dizeleri

**appsettings.json** dosyalarında yapılandırılmıştır:

- **SQL Server**: LocalDB kullanılmaktadır
- **RabbitMQ**: CloudAMQP servis kullanılmaktadır
- **MongoDB**: Yerel MongoDB instance kullanılmaktadır

## 🧪 Test Verileri

Sistem başlatıldığında otomatik olarak test verileri oluşturulur:

**Stock Verileri**:
- ProductId: 1, Count: 100
- ProductId: 2, Count: 200
- ProductId: 3, Count: 30
- ProductId: 4, Count: 40
- ProductId: 5, Count: 5

## 🐛 Hata Ayıklama ve Sorun Giderme

### Logları İnceleme

Her servis kendi loglarını tutar. Hata durumlarında:

1. SagaStateMachine.Service loglarını kontrol edin
2. İlgili mikroservis loglarını inceleyin
3. RabbitMQ yönetim panelinden mesaj kuyruklarını kontrol edin

### Yaygın Sorunlar ve Çözümleri

#### 1. RabbitMQ Bağlantı Hatası
**Hata**: `MassTransit.RabbitMqTransport.RabbitMqConnectionException`

**Çözüm**: 
- appsettings.json'da RabbitMQ connection string'ini kontrol edin
- CloudAMQP hesabınızın aktif olduğundan emin olun
- İnternet bağlantınızı kontrol edin

#### 2. Veritabanı Bağlantı Hatası
**Hata**: `Microsoft.Data.SqlClient.SqlException`

**Çözüm**:
```bash
# Migration'ları yeniden çalıştırın
cd Order.API
dotnet ef database drop --force
dotnet ef database update

cd ../SagaStateMachine.Service
dotnet ef database drop --force
dotnet ef database update
```

#### 3. MongoDB Bağlantı Hatası
**Hata**: `MongoDB.Driver.MongoConnectionException`

**Çözüm**:
```bash
# MongoDB servisini başlatın (Windows)
net start MongoDB

# MongoDB servisini başlatın (Linux/macOS)
sudo service mongod start

# veya Docker ile MongoDB çalıştırın
docker run -d -p 27017:27017 --name mongodb mongo:latest
```

#### 4. Port Çakışması
**Hata**: `System.IO.IOException: Failed to bind to address`

**Çözüm**:
- Servislerin farklı portlarda çalıştığından emin olun
- Kullanılan portları kontrol edin: `netstat -an | findstr :7085`
- launchSettings.json dosyasından port numaralarını değiştirin

#### 5. Saga State Takibi
Saga durumunu takip etmek için:

```sql
-- SagaStateMachine veritabanında saga durumlarını görüntüle
SELECT 
    CorrelationId,
    CurrentState,
    OrderId,
    BuyerId,
    TotalPrice,
    CreatedDate
FROM OrderStateInstance
ORDER BY CreatedDate DESC
```

### Debug Modunda Çalıştırma

Detaylı log çıktısı için servisleri debug modunda çalıştırın:

```bash
# Detaylı loglar için
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --verbosity detailed
```

### RabbitMQ Yönetim Paneli

CloudAMQP yönetim panelinden mesaj kuyruklarını izleyin:
- Queue Durumu: Bekleyen mesaj sayısı
- Consumer Aktivitesi: Mesaj işleme durumu
- Error Logs: Hata logları

### Performans İzleme

#### Sistem Metrikleri
- **Throughput**: Saniye başına işlenen sipariş sayısı
- **Latency**: Sipariş tamamlanma süresi
- **Error Rate**: Başarısız işlem oranı
- **Resource Usage**: CPU, RAM, Disk kullanımı

#### Saga Metrikleri
```sql
-- Başarı oranı analizi
SELECT 
    CurrentState,
    COUNT(*) as Count,
    AVG(DATEDIFF(SECOND, CreatedDate, GETDATE())) as AvgDurationSeconds
FROM OrderStateInstance 
GROUP BY CurrentState
```

### Docker ile Çalıştırma (Opsiyonel)

Projeyi Docker container'ları ile çalıştırmak için:

```dockerfile
# Örnek Dockerfile (Order.API için)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "Order.API.dll"]
```

```yaml
# docker-compose.yml örneği
version: '3.8'
services:
  orderapi:
    build: ./Order.API
    ports:
      - "7085:80"
  stockapi:
    build: ./Stock.API
    ports:
      - "7163:80"
  # ... diğer servisler
```

## 🤝 Katkıda Bulunma

1. Repository'yi fork edin
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some AmazingFeature'`)
4. Branch'inizi push edin (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Yazar

**Umut Şahinkaya**

## 📚 Kaynaklar

- [Saga Pattern Documentation](https://microservices.io/patterns/data/saga.html)
- [MassTransit Documentation](https://masstransit-project.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

---

⭐ Bu projeyi beğendiyseniz, lütfen yıldız verin!