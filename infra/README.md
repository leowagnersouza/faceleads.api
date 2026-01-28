# Infra Bicep - Faceleads

Este diretório contém templates Bicep e exemplos para provisionamento de recursos Azure usados pelo projeto Faceleads.

Arquivos principais
- `create-rg.bicep` - cria um Resource Group (escopo de subscription). Usado uma vez por subscription/ambiente.
- `sql-server-db.bicep` - cria SQL Server + Database e regras de firewall (escopo de resource group).
- `params.dev.json` - exemplo de parâmetros para ambiente `dev` (não contém segredos reais).
- `params.prod.json` - exemplo de parâmetros para ambiente `prod` (não contém segredos reais).

Princípios
- Use Bicep para infraestrutura declarativa. Mantemos versões no Git para reprodutibilidade e auditoria.
- Não commit secrets. Use parâmetros ou variáveis de CI (GitHub Actions Secrets) quando automatizar.

Como usar (local)
1. Autentique-se no Azure:

   ```bash
   az login
   az account set --subscription "<SUBSCRIPTION_ID_OR_NAME>"
   ```

2. Criar Resource Group (uma vez):

   ```bash
   az deployment sub create \
     --location brazilsouth \
     --template-file infra/create-rg.bicep \
     --parameters rgName=rg-faceleads-dev location=brazilsouth
   ```

3. Deploy do SQL Server + Database (dentro do RG):

   - Usando parâmetros inline (não recomendado em CI):

   ```bash
   az deployment group create -g rg-faceleads-dev \
     --template-file infra/sql-server-db.bicep \
     --parameters sqlServerName=faceleads-sql-dev administratorLogin=sqladmin administratorLoginPassword='YourP@ssw0rd' databaseName=Faceleads firewallStartIp='<YOUR_IP>'
   ```

   - Usando arquivo de parâmetros (ex.: `infra/params.dev.json`):

   ```bash
   az deployment group create -g rg-faceleads-dev \
     --template-file infra/sql-server-db.bicep \
     --parameters @infra/params.dev.json
   ```

4. Verificar outputs do deployment:

   ```bash
   az deployment group show -g rg-faceleads-dev --name <deploymentName> --query properties.outputs -o json
   ```

Boas práticas / notas
- Para CI/CD (GitHub Actions) use `azure/login` e chame `az deployment group create` com parâmetros passados por secrets (`administratorLoginPassword`, `firewallStartIp`, etc.).
- Evite passar senhas na linha de comando em máquinas públicas; prefira variáveis de ambiente protegidas no runner.
- Para desenvolvimento local é aceitável usar o valor padrão temporário, mas remova antes de produção.
- Depois de provisionar, rode `deploy/migrate-db.sh` apontando para a connection string de produção/dev conforme necessário.

Próximos passos sugeridos
- Gerar Bicep para App Service e Static Web App.
- Criar GitHub Action que executa Bicep deploy para `dev` (automatic) e `prod` (workflow_dispatch manual).

Se quiser, eu gero agora o workflow de deploy para `dev` (push -> deploy) ou gero o Bicep para App Service/Static Web App. Diga qual prefere.
