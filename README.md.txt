# PhoneBook Microservices

PhoneBook Microservices projesi, kişilerin iletişim bilgilerini yönetmek için geliştirilmiş bir microservices mimarisine sahip bir uygulamadır. Bu proje, RESTful API kullanarak kişileri eklemek, güncellemek, silmek ve listelemek gibi işlemleri gerçekleştiren bir sistemdir. Kafka ile asenkron mesajlaşma altyapısı da entegre edilmiştir.

## Proje Yapısı

Bu projede, her bir mikro hizmet bağımsız olarak çalışabilir. Aşağıdaki bileşenler mevcuttur:

- **Contact Service**: Kişi bilgilerini yöneten ve CRUD işlemlerini gerçekleştiren servis.
- **Kafka Producer/Consumer**: Asenkron mesajlaşma için Kafka kullanarak, sistemin farklı bölümleri arasında iletişimi sağlar.

## Gereksinimler

Projenin çalışabilmesi için aşağıdaki yazılımların sisteminizde kurulu olması gerekmektedir:

- .NET 6.0 veya daha yeni bir sürüm
- PostgreSQL (Veritabanı için)
- Kafka (Mesajlaşma için)
- Docker (Opsiyonel, Kafka'yı konteyner içinde çalıştırmak için)
- Swagger (API dokümantasyonu için)

## Projeyi Çalıştırmak

### 1. Bağımlılıkları Yükleme

Projenin kök dizininde terminali açın ve aşağıdaki komutu çalıştırarak gerekli NuGet paketlerini yükleyin:

```bash
dotnet restore
