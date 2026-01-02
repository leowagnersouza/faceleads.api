Este projeto tem dois objetivos.
Primeiro, desenvolver uma API para um frontend React para gestão de leads omerciais.
Segundo, me preparar para uma entrevista de backnend, e portanto cada passo deve ser exeutado e explicado omo se fosse em uma entrevista.

Vamos usar miroserviços, kafka, redis, sql, azure, docker, etc.
## Próximo passo: adicionar autenticação

1. Escolher o tipo de autenticação (recomendado: JWT Bearer para APIs REST).
2. Adicionar pacote de autenticação ao projeto API:

```
dotnet add Faceleads.Leads.Api package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0-preview.*
```

3. Configurar `appsettings.json` / `appsettings.Development.json` com as chaves JWT (- `Issuer`, `Audience`, `Key`).

4. No `Program.cs` da API, adicionar serviços e middleware:

- `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`
- `app.UseAuthentication();`
- `app.UseAuthorization();`

5. Proteger endpoints exigindo autorização (`[Authorize]` em controllers ou `RequireAuthorization()` em minimal APIs).

6. Implementar emissão de tokens (endpoint de login) que valide credenciais e retorne um JWT assinado usando a `Key` do appsettings.

7. Testar fluxo:

- Criar usuário/credenciais de teste (ou usar Identity / provider externo).
- Obter token via endpoint de login.
- Chamar endpoints protegidos com header `Authorization: Bearer {token}`.

Notas:
- Para ambientes de produção use uma chave forte e gerencie segredos via Azure Key Vault ou variáveis de ambiente.
- Se preferir, podemos integrar `AspNetCore.Identity` ou um provedor externo (Azure AD / IdentityServer) em vez de implementar JWT manualmente.


## Como rodar o SQL Server local via Docker

1. Inicie o container via docker compose:

```
docker-compose up -d
```

2. Verifique se o container está saudável:

```
docker ps
docker logs faceleads-sqlserver
```

3. Connection string de exemplo para development (arquivo `Faceleads.Leads.Api/appsettings.Development.json`):

```
"LeadsDatabase": "Server=localhost,1433;Database=Faceleads;User Id=sa;Password=Your_strong_P@ssw0rd;TrustServerCertificate=True;"
```

4. Criar e aplicar migrations EF Core:

```
dotnet ef migrations add AddConsultorIsDeleted --project Faceleads.Leads.Infrastructure --startup-project Faceleads.Leads.Api
dotnet ef database update --project Faceleads.Leads.Infrastructure --startup-project Faceleads.Leads.Api
```
