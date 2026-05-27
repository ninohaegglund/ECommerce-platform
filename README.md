# ECommerce Microservices

Backend solution for an ecommerce application built with ASP.NET Core microservices. The services share a SQL Server LocalDB database during development and expose Swagger endpoints for testing.

## Services

| Service | Purpose | HTTPS | HTTP |
| --- | --- | --- | --- |
| CatalogService.API | Products, categories, and product images | `https://localhost:7019` | `http://localhost:5063` |
| IdentityService.API | Users, authentication, and JWT issuing | `https://localhost:5001` | `http://localhost:5084` |
| OrderService.API | Orders and checkout flow | `https://localhost:7043` | `http://localhost:5188` |
| PaymentService.API | Payments and Stripe PaymentIntents | `https://localhost:7082` | `http://localhost:5270` |
| InventoryService.API | Stock and inventory tracking | `https://localhost:7078` | `http://localhost:5294` |
| NotificationService.API | Notifications | `https://localhost:7117` | `http://localhost:5205` |

## Requirements

- .NET SDK 10 preview
- SQL Server LocalDB
- Visual Studio 2022 or another IDE that can run ASP.NET Core projects
- Stripe test account for payment testing

## Getting Started

Restore and build the solution:

```powershell
dotnet restore .\ECommerceMicroservices.sln
dotnet build .\ECommerceMicroservices.sln
```

Apply migrations for each service:

```powershell
dotnet ef database update --project .\CatalogService.API\CatalogService.API.csproj
dotnet ef database update --project .\IdentityService.API\IdentityService.API.csproj
dotnet ef database update --project .\OrderService.API\OrderService.API.csproj
dotnet ef database update --project .\PaymentService.API\PaymentService.API.csproj
dotnet ef database update --project .\InventoryService.API\InventoryService.API.csproj
dotnet ef database update --project .\NotificationService.API\NotificationService.API.csproj
```

Run a service:

```powershell
dotnet run --project .\CatalogService.API\CatalogService.API.csproj
```

Swagger is available at `/swagger` for each running service, for example:

```text
https://localhost:7082/swagger
```

## Configuration

The services use `appsettings.json` for non-secret defaults such as connection strings and logging.

Development connection strings currently point to:

```text
Server=(localdb)\MSSQLLocalDB;Database=EcommerceDb
```

Do not commit real API keys or production secrets to `appsettings.json`.

## Stripe Setup

PaymentService uses Stripe to create PaymentIntents. Store the Stripe secret key with user-secrets during local development:

```powershell
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..." --project .\PaymentService.API\PaymentService.API.csproj
```

The frontend should use the Stripe publishable key only:

```env
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_...
```

If Stripe webhooks are used, also store the webhook signing secret:

```powershell
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..." --project .\PaymentService.API\PaymentService.API.csproj
```

The current payment flow is:

1. Create a local payment row in PaymentService.
2. Call `POST /api/payments/{paymentId}/stripe/payment-intent`.
3. Return the Stripe `clientSecret` to the frontend.
4. Confirm the payment in the frontend with Stripe.js.
5. Update local payment status from a Stripe webhook or a backend status sync flow.

## Resend Email Setup

NotificationService sends transactional email through Resend. The current app flow sends:

1. A welcome email when IdentityService creates a user account.
2. A payment confirmation email when PaymentService captures a payment.
3. Newsletter emails to active subscribers in NotificationService.

Store the Resend API key with user-secrets during local development:

```powershell
dotnet user-secrets set "Resend:ApiKey" "re_..." --project .\NotificationService.API\NotificationService.API.csproj
```

The default sender is configured in `NotificationService.API/appsettings.json` as `onboarding@resend.dev` for development. For a verified domain, override the sender email and name:

```powershell
dotnet user-secrets set "Resend:FromEmail" "orders@example.com" --project .\NotificationService.API\NotificationService.API.csproj
dotnet user-secrets set "Resend:FromName" "ECommerce Platform" --project .\NotificationService.API\NotificationService.API.csproj
```

If Resend rejects a send request, NotificationService stores the notification log with `Failed` status and returns the notification response with HTTP 502.

IdentityService and PaymentService call NotificationService through:

```json
"Services": {
  "NotificationServiceUrl": "http://localhost:5205"
}
```

Run the NotificationService and apply the latest migrations before testing the full email flow:

```powershell
dotnet ef database update --project .\NotificationService.API\NotificationService.API.csproj
dotnet ef database update --project .\PaymentService.API\PaymentService.API.csproj
dotnet run --project .\NotificationService.API\NotificationService.API.csproj
```

Newsletter endpoints are available in NotificationService:

```text
POST /api/newsletter/subscribe       Public
POST /api/newsletter/unsubscribe     Public
GET  /api/newsletter/subscribers     Admin JWT required
POST /api/newsletter/send-test       Admin JWT required
POST /api/newsletter/send            Admin JWT required
```

Admin newsletter endpoints use the same JWT settings as IdentityService. Send the admin token as:

```http
Authorization: Bearer <token>
```

Example subscribe payload:

```json
{
  "email": "customer@example.com",
  "firstName": "Ada",
  "lastName": "Lovelace"
}
```

Example send payload:

```json
{
  "subject": "News from Spelvalvet",
  "body": "We have new games in stock."
}
```

Example test send payload:

```json
{
  "recipientEmail": "admin@example.com",
  "subject": "News from Spelvalvet",
  "body": "We have new games in stock."
}
```

## Useful Commands

List stored development secrets for PaymentService:

```powershell
dotnet user-secrets list --project .\PaymentService.API\PaymentService.API.csproj
```

Build only PaymentService:

```powershell
dotnet build .\PaymentService.API\PaymentService.API.csproj
```

Run PaymentService:

```powershell
dotnet run --project .\PaymentService.API\PaymentService.API.csproj
```
