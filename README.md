
# Template-CleanArchitecture-MultiTenancy

Este projeto é uma evolução do template original, adaptado para suportar **multitenancy** de forma eficiente. Ele implementa uma arquitetura modular robusta com base em **Domain-Driven Design (DDD)**, **Command Query Responsibility Segregation (CQRS)** e suporte a múltiplos bancos de dados, permitindo uma separação clara entre tenants.

[GitHub - Template-CleanArchitecture](https://github.com/joaopaulobiesek/Template-CleanArchitecture "Template-CleanArchitecture")

## Objetivo

Fornecer uma base para sistemas SaaS multitenancy que necessitam gerenciar múltiplos clientes com isolações robustas, alto desempenho e escalabilidade.

## Arquitetura

Este projeto adota os seguintes princípios e padrões arquiteturais:

- **Domain-Driven Design (DDD)**: Organização baseada no domínio central, isolando a lógica de negócios.
- **CQRS**: Separação de operações de leitura e escrita, garantindo otimização e manutenibilidade.
- **Multitenancy com Bancos de Dados Dedicados**: Cada tenant possui seu próprio banco de dados para máxima segurança e isolamento.

## Estrutura do Projeto

### 1. Template.Api

A camada **API** é responsável por expor os endpoints da aplicação, organizando os controladores e middlewares que permitem a interação com as camadas de domínio e infraestrutura. Abaixo está a estrutura da camada API, com detalhes sobre os controladores e configurações.

#### Estrutura da Camada API

```plaintext
Controllers
├── Core
│   ├── V1
│   │   ├── Clients
│   │   │   ├── ClientController.cs
├── System
│   ├── BaseController.cs
│   ├── HealthController.cs
├── Tenant
│   ├── V1
│   │   ├── Clients
│   │   │   ├── ClientController.cs
├── V1
│   ├── ExternalServices
│   │   ├── Files
│   │   │   ├── FileController.cs
│   │   ├── Google
│   │   │   ├── GoogleController.cs
│   ├── Identity
│   │   ├── Auth
│   │   │   ├── AuthController.cs
│   │   ├── Users
│   │   │   ├── UsersController.cs
Middlewares
├── ExceptionHandlingMiddleware.cs
├── TenantMiddleware.cs
Program.cs
```

### 2. Template.Application

A camada **Application** contém a lógica de aplicação e implementa os padrões de **CQRS (Command Query Responsibility Segregation)**, organizando os comandos e consultas que manipulam as operações principais do sistema. Abaixo está a estrutura da camada Application, com detalhes sobre os behaviors, serviços, interfaces e modelos.

#### Estrutura da Camada Application

```plaintext
Common
├── Behaviours
│   ├── AuthorizationBehaviour.cs
│   ├── HandlerBase.cs
│   ├── HandlerDependencies.cs
│   ├── LoggingBehaviour.cs
│   ├── PerformanceBehaviour.cs
│   ├── UnhandledExceptionBehaviour.cs
│   ├── ValidationBehaviour.cs
├── Exceptions
│   ├── ForbiddenAccessException.cs
├── Interfaces
│   ├── IRepositories
│   │   ├── Core
│   │   │   ├── Base
│   │   │   │   ├── IRepository.cs
│   │   │   ├── Implementations
│   │   │   │   ├── IClientRepository.cs
│   │   ├── Tenant
│   │   │   ├── Base
│   │   │   │   ├── IRepository.cs
│   │   │   ├── Implementations
│   │   │   │   ├── IClientRepository.cs
│   ├── Security
│   │   ├── ICurrentUser.cs
│   │   ├── IIdentityService.cs
│   │   ├── IUser.cs
│   ├── Services
│   │   ├── IGoogle.cs
│   │   ├── ISendGrid.cs
│   │   ├── IStorage.cs
├── Models
│   ├── ApiResponse.cs
│   ├── GoogleCalendarEvent.cs
│   ├── PaginatedList.cs
│   ├── User.cs
├── Persistence
│   ├── ICoreContext.cs
│   ├── ICoreDapperConnection.cs
│   ├── ITenantContext.cs
│   ├── ITenantDapperConnection.cs
├── Security
│   ├── AuthorizeAttribute.cs
DependencyInjection.cs
Domains
├── Core
│   ├── V1
│   │   ├── Clients
│   │   │   ├── Commands
│   │   │   │   ├── CreateClient
│   │   │   │   │   ├── CreateClientCommand.cs
│   │   │   │   │   ├── CreateClientCommandHandler.cs
│   │   │   │   │   ├── CreateClientCommandValidator.cs
│   │   │   │   ├── DeactivateClient
│   │   │   │   │   ├── DeleteUserCommandHandler.cs
│   │   │   │   ├── DeleteClient
│   │   │   │   │   ├── DeleteUserCommandHandler.cs
│   │   │   │   ├── UpdateClient
│   │   │   │   │   ├── UpdateClientCommand.cs
│   │   │   │   │   ├── UpdateClientCommandHandler.cs
│   │   │   │   │   ├── UpdateClientCommandValidator.cs
│   │   │   ├── Queries
│   │   │   │   ├── GetAll
│   │   │   │   │   ├── GetAllQuery.cs
│   │   ├── ViewModels
│   │   │   ├── ClientVM.cs
├── Tenant
│   ├── V1
│   │   ├── Clients
│   │   │   ├── Commands
│   │   │   │   ├── CreateClient
│   │   │   │   │   ├── CreateClientCommand.cs
│   │   │   │   │   ├── CreateClientCommandHandler.cs
│   │   │   │   │   ├── CreateClientCommandValidator.cs
│   │   │   │   ├── DeactivateClient
│   │   │   │   │   ├── DeleteUserCommandHandler.cs
│   │   │   │   ├── DeleteClient
│   │   │   │   │   ├── DeleteUserCommandHandler.cs
│   │   │   │   ├── UpdateClient
│   │   │   │   │   ├── UpdateClientCommand.cs
│   │   │   │   │   ├── UpdateClientCommandHandler.cs
│   │   │   │   │   ├── UpdateClientCommandValidator.cs
│   │   │   ├── Queries
│   │   │   │   ├── GetAll
│   │   │   │   │   ├── GetAllQuery.cs
│   │   ├── ViewModels
│   │   │   ├── ClientVM.cs
├── V1
│   ├── ExternalServices
│   │   ├── Google
│   │   │   ├── Queries
│   │   │   │   ├── CalendarEvents
│   │   │   │   │   ├── GetCalendarEventsQuery.cs
│   │   ├── Storage
│   │   │   ├── Commands
│   │   │   │   ├── Delete
│   │   │   │   │   ├── DeleteCommand.cs
│   │   │   │   ├── Upload
│   │   │   │   │   ├── UploadCommand.cs
│   │   │   ├── Queries
│   │   │   │   ├── DownloadFile
│   │   │   │   │   ├── DownloadFileQuery.cs
│   ├── Identity
│   │   ├── Auth
│   │   │   ├── Commands
│   │   │   │   ├── LoginUser
│   │   │   │   │   ├── LoginUserCommand.cs
│   │   │   │   │   ├── LoginUserCommandHandler.cs
│   │   │   │   │   ├── LoginUserCommandValidator.cs
│   │   ├── Users
│   │   │   ├── Commands
│   │   │   │   ├── CreateUsers
│   │   │   │   │   ├── CreateUserCommand.cs
│   │   │   │   │   ├── CreateUserCommandHandler.cs
│   │   │   │   │   ├── CreateUserCommandValidator.cs
│   │   │   │   ├── DeleteUsers
│   │   │   │   │   ├── DeleteUserCommandHandler.cs
│   │   │   │   ├── EditUsers
│   │   │   │   │   ├── EditUserCommand.cs
│   │   │   │   │   ├── EditUserCommandHandler.cs
│   │   │   │   │   ├── EditUserCommandValidator.cs
│   │   │   ├── Queries
│   │   │   │   ├── GetAll
│   │   │   │   │   ├── GetAllQuery.cs
│   │   │   │   ├── GetPolices
│   │   │   │   │   ├── GetPolicesQuery.cs
│   │   │   │   ├── GetRoles
│   │   │   │   │   ├── GetRolesQuery.cs
│   ├── ViewModels
│   │   ├── Storage
│   │   │   ├── UploadFileVM.cs
│   │   ├── Users
│   │   │   ├── LoginUserVm.cs
│   │   │   ├── UserVm.cs
ViewModels
├── Shared
│   ├── KeyValuePairVM.cs
```

### 3. Template.Domain

A camada **Domain** é responsável por encapsular as regras de negócio e a lógica central do sistema, implementando os conceitos de **Domain-Driven Design (DDD)**. Essa camada inclui entidades, validações, interfaces e constantes que são utilizados para gerenciar e validar informações específicas do domínio.

#### Estrutura da Camada Domain

```plaintext
Constants
├── Policies.cs
├── Roles.cs
Entity
├── Core
│   ├── Client.cs
├── Entity.cs
├── Tenant
│   ├── Client.cs
Interfaces
├── Core
│   ├── IClient.cs
├── Tenant
│   ├── IClient.cs
StringFormatter.cs
Validations
├── CNPJValidationAttribute.cs
├── CPFValidationAttribute.cs
├── DomainExceptionValidation.cs
```

### 3. Template.Infra

A camada **Infra** é responsável pela infraestrutura da aplicação, incluindo a persistência de dados, integração com serviços externos, autenticação e configurações. Esta camada implementa as interações de baixo nível necessárias para suportar a lógica de domínio e a camada de apresentação.

#### Estrutura da Camada Infra

```plaintext
DependencyInjection.cs
ExternalServices
├── Google
│   ├── DependencyInjection.cs
│   ├── Google.cs
├── SendEmails
│   ├── DependencyInjection.cs
│   ├── SendGrid.cs
├── Storage
│   ├── AzureStorage.cs
│   ├── DependencyInjection.cs
Identity
├── ContextUser.cs
├── CurrentUser.cs
├── CustomInitializerIdentity.cs
├── ITokenService.cs
├── IdentityResultExtensions.cs
├── IdentityService.cs
├── LocalizedIdentityErrorDescriber.cs
├── TokenService.cs
Persistence
├── Contexts
│   ├── BaseContext.cs
│   ├── Core
│   │   ├── CoreContext.cs
│   │   ├── CoreDapper.cs
│   ├── InicializarContext.cs
│   ├── Schema.cs
│   ├── Tenant
│   │   ├── TenantContext.cs
│   │   ├── TenantDapper.cs
├── DependencyInjection.cs
├── Migrations_Core
├── Migrations_Tenant
├── Repositories
│   ├── Core
│   │   ├── Base
│   │   │   ├── Repository.cs
│   │   ├── Implementations
│   │   │   ├── ClientRepository.cs
│   ├── DependencyInjection.cs
│   ├── Tenant
│   │   ├── Base
│   │   │   ├── Repository.cs
│   │   ├── Implementations
│   │   │   ├── ClientRepository.cs
Settings
├── Configurations
│   ├── GetTenantConnectionConfiguration.cs
│   ├── GoogleConfiguration.cs
│   ├── IdentityConfiguration.cs
│   ├── JwtConfiguration.cs
│   ├── SendGridConfiguration.cs
│   ├── StorageConfiguration.cs
├── Maps
│   ├── Core
│   │   ├── ClientMap.cs
│   ├── Tenant
│   │   ├── ClientMap.cs
│   ├── UserBaseContextMap.cs
```

## Configuração do Google Authentication

### Passos para Configuração

1. Acesse o [Google Cloud Console](https://console.cloud.google.com/).
2. Crie um novo projeto ou selecione um existente.
3. Ative a **API do Google Calendar** navegando até a opção `APIs e Serviços` > `Biblioteca` e pesquisando por "Google Calendar API".
4. Configure a tela de consentimento OAuth em `APIs e Serviços` > `Tela de consentimento OAuth`.
   - Preencha todas as informações obrigatórias.
   - Adicione escopos necessários, como `openid`, `email`, `profile` e `https://www.googleapis.com/auth/calendar.readonly`.
5. Crie as credenciais OAuth em `APIs e Serviços` > `Credenciais` > `Criar Credenciais` > `ID do cliente OAuth`.
   - Escolha o tipo de aplicativo: **Aplicativo da Web**.
   - Adicione o URI de redirecionamento autorizado (ex.: `https://localhost:7048/auth/google/callback`).
6. Caso esteja em desenvolvimento, adicione seu e-mail na seção de **Testadores autorizados**.

### Colocando em Produção

1. Certifique-se de que todas as informações na tela de consentimento OAuth estão completas e corretas.
2. Solicite a verificação do aplicativo no Google Cloud Console.
   - Após a aprovação, o app poderá ser usado por qualquer usuário.

### Integração no Projeto

No projeto, a autenticação do Google foi configurada seguindo as etapas abaixo:

- A classe `Google` foi implementada em `Template.Infra.ExternalServices.Google` para lidar com o fluxo de autenticação OAuth.
- A função `AuthenticateUserAsync` é responsável por trocar o código de autorização por tokens e salvar o token de acesso no banco.
- A URL de autenticação é gerada pela função `GenerateAuthenticationUrl`.

### Recuperação de Eventos do Google Calendar

Com o token de acesso salvo, você pode recuperar os eventos do Google Calendar do usuário autenticado.

#### Passos:
1. Utilize a função `GetGoogleCalendarEventsAsync` na classe `Google`.
2. Esta função utiliza o token armazenado para acessar a API do Google Calendar e retorna os eventos.

Exemplo de URL da API usada:
```
https://www.googleapis.com/calendar/v3/calendars/primary/events
```

## Telas Necessárias no Google Cloud Console

- **Tela de consentimento OAuth**: Configure os escopos necessários e finalize a configuração para publicar o app.
- **Biblioteca de APIs**: Certifique-se de ativar a **API do Google Calendar**.
- **Credenciais**: Gere o ID do cliente OAuth e configure os URIs de redirecionamento.

Para mais detalhes, consulte a [documentação oficial do Google](https://developers.google.com/identity).

---

### Observação

Neste projeto, optou-se por não utilizar **MediatR** e **AutoMapper**, apesar de serem recomendados em boas práticas de desenvolvimento. Essa decisão reflete uma escolha intencional do design da aplicação para manter controle direto sobre os fluxos de dados e as operações, conforme preferências e necessidades específicas.

## Executando o Projeto

1. **Clone o repositório** e navegue até o diretório principal:
   ```bash
   git clone <url-do-repositorio>
   cd Template.Project
   ```
2. **Configuração**: Edite o arquivo `appsettings.json` para ajustar as strings de conexão e configurações do ambiente.
3. **Aplicação de Migrations**: Execute o seguinte comando para aplicar as migrações do Entity Framework e criar as tabelas no banco de dados:
   ```bash
   dotnet ef database update
   ```
4. **Inicie a aplicação**:
   ```bash
   dotnet run --project Template.Api
   ```

## Contribuindo

Para contribuir com o projeto, siga os passos abaixo:

1. Crie uma branch para a sua feature ou correção.
2. Envie um pull request com uma descrição detalhada das suas alterações, incluindo o objetivo e o impacto.

Este projeto ainda possui espaço para melhorias adicionais, mas várias boas práticas foram aplicadas para garantir uma base sólida e escalável. O objetivo é servir como um modelo para arquiteturas limpas e de fácil manutenção. Contribuições são bem-vindas para aprimorar ainda mais a funcionalidade e estrutura.