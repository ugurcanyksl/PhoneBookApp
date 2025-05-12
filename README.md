📞 PhoneBook Microservices Project
Bu proje, mikroservis mimarisi ile geliştirilen bir Rehber Uygulaması örneğidir. Kullanıcıların kişisel bilgilerini (isim, soyisim, firma, iletişim bilgileri) saklayabileceği ve konum bazlı raporlar oluşturabileceği bir sistemdir.
📦 Proje Bileşenleri

ContactService: Kişi ve iletişim bilgilerini yönetir.
ReportService: Kafka ile gelen rapor isteklerini alır, işler ve konum bazlı raporlar üretir.

🔧 Kullanılan Teknolojiler



Teknoloji
Açıklama



.NET Core 8.0
Mikroservis altyapısı


PostgreSQL
Veritabanı olarak kullanılır


Entity Framework Core
ORM katmanı


Kafka (Confluent)
Mikroservisler arası asenkron iletişim


AutoMapper
DTO ve Model mapleme


xUnit & Moq
Birim testleri için


Docker (Opsiyonel)
Dağıtım ve container ortamı (isteğe bağlı)


🗃️ Katmanlı Yapı
ContactService

Controllers: API uçları (endpointler)
Services: İş mantığı (ContactImplementationService)
Repositories: Veritabanı işlemleri (IContactRepository)
Tests: xUnit ile yazılmış birim testleri

ReportService

Controllers: Rapor isteklerinin işlendiği API
Services: Rapor oluşturma mantığı (IReportImplementationService)
Kafka: Kafka Consumer & HostedService sınıfları (ReportConsumer, ReportConsumerHostedService)

🚀 Kurulum ve Çalıştırma
1. Bağımlılıklar

.NET 8.0 SDK
PostgreSQL
Kafka ve ZooKeeper: localhost:9092 üzerinde çalışmalı.

2. Veritabanı Kurulumu
dotnet ef database update --project ContactService.Contact.API
dotnet ef database update --project ReportService.Report.API

3. Kafka Kurulumu

Kafka ve ZooKeeper’ı localhost:9092 üzerinde çalıştır.
Kullanılan topic’ler: phonebook-reports, report-request-topic, report-created-event.

4. Servisleri Çalıştır
dotnet run --project ContactService.Contact.API
dotnet run --project ReportService.Report.API

🌐 API Uç Noktaları
ContactService



HTTP
Endpoint
Açıklama



POST
/api/v1/Contact
Yeni kişi ekleme


GET
/api/v1/Contact/{id}
Belirli kişiyi getir


GET
/api/v1/Contact?page={page}&pageSize={pageSize}
Tüm kişileri getir (sayfalama)


DELETE
/api/v1/Contact/{id}
Kişi sil


POST
/api/v1/Contact/{personId}/contact-info
Kişiye iletişim bilgisi ekle


DELETE
/api/v1/Contact/{personId}/contact-info/{infoId}
Kişiden iletişim bilgisi sil


GET
/api/v1/Contact/location/{location}
Konuma göre kişileri getir


ReportService



HTTP
Endpoint
Açıklama



POST
/api/Report
Belirli bir konum için rapor isteği oluşturur


GET
/api/Report/{id}
Raporu ID ile getirir


GET
/api/Report?page={page}&pageSize={pageSize}
Tüm raporları getir (sayfalama)


🧪 Testler
Testleri çalıştırmak için:
dotnet test

Coverage ölçümü için:
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport -reporttypes:Html

📋 Ortam Değişkenleri

ConnectionStrings__ContactDbConnection: ContactService için PostgreSQL bağlantı string’i.
ConnectionStrings__ReportDbConnection: ReportService için PostgreSQL bağlantı string’i.
Kafka: Kafka:BootstrapServers (localhost:9092).

