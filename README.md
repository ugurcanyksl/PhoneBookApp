# 📞 PhoneBook Microservices Project

Bu proje, mikroservis mimarisi ile geliştirilen bir **Rehber Uygulaması örneğidir. Kullanıcıların kişisel bilgilerini (isim, soyisim, firma, iletişim bilgileri) saklayabileceği ve konum bazlı raporlar oluşturabileceği bir sistemdir.

## 📦 Proje Bileşenleri

Bu çözümde iki ana mikroservis bulunmaktadır:

- **ContactService**: Kişi ve iletişim bilgilerini yönetir.
- **ReportService**: Kafka ile gelen rapor isteklerini alır, işler ve konum bazlı raporlar üretir.

## 🔧 Kullanılan Teknolojiler

| Teknoloji         	| Açıklama                                  |
|-----------------------|-------------------------------------------|
| .NET Core 7.0    	| Mikroservis altyapısı                     |
| PostgreSQL       	| Veritabanı olarak kullanılır              |
| Entity Framework Core | ORM katmanı                         	    |
| Kafka (Confluent) 	| Mikroservisler arası asenkron iletişim    |
| AutoMapper       	| DTO ve Model mapleme                      |
| xUnit & Moq      	| Birim testleri için                       |
| Docker (Opsiyonel) 	| Dağıtım ve container ortamı (isteğe bağlı)|

## 🗃️ Katmanlı Yapı

### ContactService

- `Controllers`: API uçları (endpointler)
- `Services`: İş mantığı (`ContactImplementationService`)
- `Repositories`: Veritabanı işlemleri (`IContactRepository`)
- `Models`: Entity sınıfları (`Person`, `ContactInfo`)
- `DTOs`: Veri transfer nesneleri (`CreateContactDto`, `ContactDto`)
- `Tests`: xUnit ile yazılmış birim testleri

### ReportService

- `Controllers`: Rapor isteklerinin işlendiği API
- `Services`: Rapor oluşturma mantığı (`IReportImplementationService`)
- `Kafka`: Kafka Consumer & HostedService sınıfları (`ReportConsumer`, `ReportConsumerHostedService`)
- `DTOs`: Rapor ve detay DTO'ları (`ReportRequestDto`, `ReportDto`)
- `AutoMapper`: DTO ↔ Model dönüşümleri (`ConfigureAutoMapper`)

---

## 🧪 Testler

### ContactService Unit Testleri

Testler `ContactService.Tests` projesi altında yer almakta olup, servis katmanı için yazılmıştır:

| Test 								   | Açıklama 				      |
|------------------------------------------------------------------|------------------------------------------|
| `CreateAsync_ShouldReturnPerson_WhenValidData` 		   | Geçerli DTO ile kişi oluşturulması       |
| `CreateAsync_ShouldThrowArgumentNullException_WhenDtoIsNull`     | Boş DTO durumunda exception fırlatılması |
| `GetByIdAsync_ShouldReturnPerson_WhenPersonExists` 	           | Kişi varsa geri döner 		      |
| `GetByIdAsync_ShouldReturnNull_WhenPersonDoesNotExist` 	   | Kişi yoksa null döner 		      |
| `GetAllAsync_ShouldReturnListOfPersons` 		           | Tüm kişilerin listelenmesi 	      |
| `DeleteAsync_ShouldReturnTrue_WhenPersonDeleted` 		   | Silme başarılıysa true döner 	      |
| `DeleteAsync_ShouldReturnFalse_WhenPersonNotFound` 		   | Silme başarısızsa false döner 	      |
| `AddContactInfoAsync_ShouldReturnTrue_WhenContactInfoAdded` 	   | İletişim bilgisi eklenmesi 	      |
| `RemoveContactInfoAsync_ShouldReturnTrue_WhenContactInfoRemoved` | İletişim bilgisi silinmesi 	      |

Testler `Moq` kullanılarak bağımlılıkların izole edilmesiyle yazılmıştır.

---

## 📨 Kafka Entegrasyonu

### Senaryo

1. `ReportService.Report.API` bir **Kafka Consumer** içerir.
2. `report-request-topic` adlı topic'ten veri dinler.
3. Tüketilen veri `ReportRequestDto` nesnesine dönüştürülür.
4. `CreateAsync` metodu çağrılarak rapor hazırlanır.

### Dosyalar

- `ReportConsumer.cs`: Kafka mesajlarını dinleyen ana sınıf.
- `ReportConsumerHostedService.cs`: Background service olarak Kafka consumer’ı başlatır ve durdurur.

---

## 🧭 API Uç Noktaları

### ContactService

| HTTP 	  | Endpoint                                         | Açıklama 	            |
|---------|--------------------------------------------------|------------------------------|
| `POST`  | `/api/contacts` 				     | Yeni kişi ekleme             |
| `GET`   | `/api/contacts/{id}` 			     | Belirli kişiyi getir         |
| `GET`   | `/api/contacts` 				     | Tüm kişileri getir 	    |
| `DELETE`| `/api/contacts/{id}` 		             | Kişi sil 		    |
| `POST`  | `/api/contacts/{id}/contactInfo` 		     | Kişiye iletişim bilgisi ekle |
| `DELETE`| `/api/contacts/{id}/contactInfo/{contactInfoId}` | Kişiden iletişim bilgisi sil |

### ReportService

| HTTP  | Endpoint           | Açıklama                                      |
|-------|--------------------|-----------------------------------------------|
| `POST`| `/api/report`      | Belirli bir konum için rapor isteği oluşturur |
| `GET` | `/api/report/{id}` | Raporu ID ile getirir                         |

---

## 🧑‍💻 Nasıl Çalıştırılır?

### 1. Veritabanı Kurulumu

```bash
# PostgreSQL kurulu olmalı, ardından migration ve update yapılır
dotnet ef database update --project ContactService.Contact.API
dotnet ef database update --project ReportService.Report.API
